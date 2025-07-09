using System;
using System.Collections;
using System.Collections.Generic;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class EveyBsodaa : NPC
    {
        internal static Character charEnum = (Character)(-1);
        internal static Dictionary<string, Sprite[]> animations;

        public float normalSpeed;
        public float restockSpeed = 24f;

        public CustomSpriteAnimator animator;

        public AudioManager audMan;
        public EveyBsodaaSpray projectilePre;

        public SoundObject audCharging;
        public SoundObject[] audSuccess;
        public SoundObject audReloaded;

        public List<RoomController> BsodaaRooms { get; private set; } = [];
        private DijkstraMap dijkstraMap;
        private bool autoRestock = false;

        public float ChargeTime { get; private set; } = 2f;

        public override void Initialize()
        {
            base.Initialize();

            normalSpeed = Navigator.maxSpeed;

            animator.PopulateAnimations(animations, 8);
            animator.Play("Idle",1f);

            // Used for dictating what room is closest
            dijkstraMap = new DijkstraMap(ec, PathType.Nav, int.MaxValue, transform);

            behaviorStateMachine.ChangeState(new EveyBsodaa_Statebase(this));
        }

        private void Start()
        {
            if (autoRestock)
            {
                behaviorStateMachine.ChangeState(new EveyBsodaa_WanderingReady(this));
                return;
            }

            bool insideRoom = false;

            Cell currentCell = ec.CellFromPosition(transform.position);
            RoomController room;
            BsodaaRoomFunction roomFunction;
            if (currentCell != null && (room = currentCell.room) != null && room.functionObject.TryGetComponent(out roomFunction))
            {
                insideRoom = true;

                if (roomFunction.HelperInStock)
                    BsodaaRooms.Add(room);
            }
            foreach (RoomController room2 in ec.rooms)
            {
                if (BsodaaRooms.Contains(room2)) continue;
                if (room2.functionObject != null && room2.functionObject.TryGetComponent(out roomFunction) && roomFunction.HelperInStock)
                    BsodaaRooms.Add(room2);
            }

            if (insideRoom)
                behaviorStateMachine.ChangeState(new EveyBsodaa_LeavingRoom(this, currentCell.room));
            else
                behaviorStateMachine.ChangeState(new EveyBsodaa_WanderingReady(this));
            if (BsodaaRooms.Count == 0)
                autoRestock = true;
        }

        public void BsodaFinished(bool hit)
        {
            animator.Play("Upset", 1f); // Set to upset sprite if no NPC/Player was hit
            if (hit) 
            {
                animator.Play("Happy", 1f); // Set to happy sprite
                audMan.QueueAudio(audSuccess[UnityEngine.Random.Range(0, audSuccess.Length)]); // Play one of the happy lines
            }

            RestockAction();
        }

        public void Shoot(Transform target)
        {
            EveyBsodaaSpray bsoda = Instantiate(projectilePre);
            bsoda.bsodaa = this;
            bsoda.transform.rotation = Directions.DirFromVector3(target.position-transform.position, 45f).ToRotation();
            bsoda.transform.position = transform.position + bsoda.transform.forward * 2f;

            behaviorStateMachine.ChangeState(new EveyBsodaa_Statebase(this));
            animator.Play("Shoot", 1f);
        }

        public void Charge()
        {
            audMan.FlushQueue(true);
            audMan.PlaySingle(audCharging);
            animator.Play("Charge", 1f);
        }

        public bool NpcInSight(NPC npc)
        {
            if (Navigator.Entity.Blinded)
                return false;

            looker.Raycast(npc.transform, Mathf.Min((transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude, looker.distance, npc.ec.MaxRaycast), out bool sighted);
            return sighted;
        }

        private void RestockAction()
        {
            if (autoRestock)
            {
                behaviorStateMachine.ChangeState(new EveyBsodaa_TimedRestock(this, 20f));
                return;
            }
            RoomsToRestock();
        }

        public bool RoomsToRestock()
        {
            if (BsodaaRooms.Count == 0)
            {
                animator.Play("Upset", 1f);
                return false;
            }

            // Force calculate dijkstra map in order to grab values
            dijkstraMap.Calculate();

            int dist = int.MaxValue;
            RoomController currentRoom = BsodaaRooms[0];
            foreach (RoomController room in BsodaaRooms)
            {
                if (dijkstraMap.Value(room.position) > dist) continue;
                currentRoom = room;
                dist = dijkstraMap.Value(room.position);
            }
            behaviorStateMachine.ChangeState(new EveyBsodaa_AttemptRestock(this, currentRoom));
            return true;
        }

        public void LeaveCurrentRoom()
        {
            Cell cell = ec.CellFromPosition(transform.position);
            if (cell == null || cell.room == ec.nullRoom || cell.room.type == RoomType.Hall)
            {
                behaviorStateMachine.ChangeState(new EveyBsodaa_WanderingReady(this));
                return;
            }
            behaviorStateMachine.ChangeState(new EveyBsodaa_LeavingRoom(this, cell.room));
        }
    }

    public class EveyBsodaa_Statebase(EveyBsodaa bsodaa) : NpcState(bsodaa)
    {
        protected readonly EveyBsodaa bsodaa = bsodaa;
    }

    public class EveyBsodaa_Wandering(EveyBsodaa bsodaa) : EveyBsodaa_Statebase(bsodaa)
    {
        public override void Enter()
        {
            base.Enter();
            bsodaa.animator.Play("Idle", 1f);
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }
    }

    public class EveyBsodaa_LeavingRoom(EveyBsodaa bsodaa, RoomController room) : EveyBsodaa_Wandering(bsodaa)
    {
        private readonly RoomController roomToLeave = room;

        public override void OnRoomExit(RoomController room)
        {
            base.OnRoomExit(room);
            if (room == roomToLeave)
                npc.behaviorStateMachine.ChangeState(new EveyBsodaa_WanderingReady(bsodaa));
        }
    }

    public class EveyBsodaa_WanderingReady(EveyBsodaa bsodaa) : EveyBsodaa_Wandering(bsodaa)
    {
        public override void InPlayerSight(PlayerManager player)
        {
            base.InPlayerSight(player);
            npc.behaviorStateMachine.ChangeState(new EveyBsodaa_PreCharge(bsodaa, player));
        }

        public override void Update()
        {
            base.Update();
            foreach (NPC npc in npc.ec.npcs)
            {
                if (npc == bsodaa) continue;
                if (bsodaa.NpcInSight(npc))
                {
                    npc.behaviorStateMachine.ChangeState(new EveyBsodaa_PreCharge(bsodaa, npc));
                    return;
                }
            }
        }
    }

    public class EveyBsodaa_PreCharge : EveyBsodaa_Statebase
    {
        private readonly Transform targetTransform;

        private readonly NPC targetNpc;
        private readonly PlayerManager player;

        private readonly Func<bool> InSight;
        private EveyBsodaa_PreCharge(EveyBsodaa bsodaa, Transform target) : base(bsodaa)
        {
            targetTransform = target;
        }
        public EveyBsodaa_PreCharge(EveyBsodaa bsodaa, PlayerManager target) : this(bsodaa, target.transform)
        {
            player = target;
            InSight = PlayerInSight;
        }
        public EveyBsodaa_PreCharge(EveyBsodaa bsodaa, NPC target) : this(bsodaa, target.transform)
        {
            targetNpc = target;
            InSight = NpcInSight;
        }

        private bool PlayerInSight() => npc.looker.PlayerInSight(player);
        private bool NpcInSight() => bsodaa.NpcInSight(targetNpc);

        public override void Enter()
        {
            base.Enter();
            npc.Navigator.FindPath(npc.transform.position, targetTransform.position);
            ChangeNavigationState(new NavigationState_TargetPosition(npc, 63, npc.Navigator.NextPoint));
        }

        public override void DestinationEmpty()
        {
            if (InSight())
            {
                base.DestinationEmpty();
                npc.behaviorStateMachine.ChangeState(new EveyBsodaa_Charging(bsodaa, targetTransform, bsodaa.ChargeTime));
                return;
            }
            npc.behaviorStateMachine.ChangeState(new EveyBsodaa_WanderingReady(bsodaa));
        }
    }

    public class EveyBsodaa_Charging : EveyBsodaa_Statebase
    {
        private readonly Transform targetTransform;
        private float timeLeft;

        public EveyBsodaa_Charging(EveyBsodaa bsodaa, Transform target, float chargeTime) : base(bsodaa)
        {
            targetTransform = target;
            timeLeft = chargeTime;
        }

        public override void Enter()
        {
            base.Enter();
            bsodaa.Charge();
            ChangeNavigationState(new NavigationState_DoNothing(npc, 0));
        }

        public override void Update()
        {
            base.Update();
            timeLeft -= Time.deltaTime * npc.TimeScale;
            if (timeLeft <= 0f)
            {
                bsodaa.Shoot(targetTransform);
                npc.behaviorStateMachine.ChangeState(new EveyBsodaa_Statebase(bsodaa));
            }
        }
    }

    public class EveyBsodaa_EmptyHanded(EveyBsodaa bsodaa) : EveyBsodaa_Statebase(bsodaa)
    {
        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_DoNothing(npc, 127));
        }
    }

    // Fallback state if no valid Bsodaa rooms are in the level
    public class EveyBsodaa_TimedRestock(EveyBsodaa bsodaa, float time) : EveyBsodaa_Wandering(bsodaa)
    {
        private float timeLeft = time;

        public override void Update()
        {
            base.Update();
            timeLeft -= Time.deltaTime * npc.TimeScale;
            if (timeLeft <= 0f)
                npc.behaviorStateMachine.ChangeState(new EveyBsodaa_WanderingReady(bsodaa));
        }
    }

    public class EveyBsodaa_AttemptRestock : EveyBsodaa_Statebase
    {
        private readonly BsodaaRoomFunction bsodaaRoom;
        private Vector3 targetLocation;

        public EveyBsodaa_AttemptRestock(EveyBsodaa bsodaa, RoomController room) : base(bsodaa)
        {
            targetLocation = npc.ec.RealRoomMid(room); // Fallback value

            bsodaaRoom = room.functionObject.GetComponent<BsodaaRoomFunction>();
            if (bsodaaRoom == null) return;

            targetLocation = bsodaaRoom.Helper.transform.position;
            targetLocation += (npc.ec.RealRoomMid(room)-targetLocation).normalized * 5f;
        }

        public override void Enter()
        {
            base.Enter();
            bsodaa.Navigator.maxSpeed = bsodaa.restockSpeed;
            ChangeNavigationState(new NavigationState_TargetPosition(npc, 127, targetLocation));
        }

        // Reached the end of the room
        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            bsodaa.StartCoroutine(Restock());
        }

        private IEnumerator Restock()
        {
            yield return new WaitForSecondsNPCTimescale(npc, 0.5f);
            bsodaa.Navigator.maxSpeed = bsodaa.normalSpeed;
            if (bsodaaRoom.HelperInStock)
            {
                bsodaaRoom.Helper.Restock();
                yield return new WaitForSecondsNPCTimescale(npc, 0.3f);
                bsodaa.LeaveCurrentRoom();
                yield break;
            }
            bsodaa.BsodaaRooms.Remove(bsodaaRoom.Room);
            if (!bsodaa.RoomsToRestock())
            {
                ChangeNavigationState(new NavigationState_TargetPosition(npc, 127, npc.ec.RealRoomMid(bsodaaRoom.room)));
                npc.behaviorStateMachine.ChangeState(new EveyBsodaa_EmptyHanded(bsodaa));
                yield break;
            }
        }
    }
}
