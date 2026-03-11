using System.Collections.Generic;
using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class Carter : NPC
    {
        public SpriteRenderer sprite;
        public Sprite sprNormal, sprAngry, sprScreech;

        public AudioManager audMan;
        public SoundObject[] audLost, audItms, audHelp, audLeave;
        public SoundObject audCoords, audThanks, audIntro, audLoop;

        public ItemObject[] possibleItems;
        public List<RoomCategory> roomsToAvoid =
        [
            RoomCategory.Null,
            RoomCategory.Buffer,
            RoomCategory.Hall,
            RoomCategory.FieldTrip,
            RoomCategory.Mystery,
            RoomCategory.Store,
            RoomCategory.Test
        ];

        public float OriginalSpeed {get; private set;}

        public override void Initialize()
        {
            base.Initialize();
            OriginalSpeed = navigator.maxSpeed;
            behaviorStateMachine.ChangeState(new Carter_WanderDefault(this));
        }

        public int TrySpawnItem(PlayerManager player)
        {
            int id = Random.Range(0,possibleItems.Length), index;

            List<WeightedRoomController> weightedRooms = ec.RoomsWeightedByDistanceFromStartRoomAndValue(currentRoom, roomsToAvoid);

            while (weightedRooms.Count > 0)
            {
                index = WeightedSelection<RoomController>.RandomIndexList(WeightedRoomController.Convert(weightedRooms));
                RoomController selection = weightedRooms[index].selection;
                weightedRooms.RemoveAt(index);
                if (selection.AvailableItemRespawnPoints == 0) continue;
                
                List<Pickup> itms = [];
                foreach (Pickup pickup in selection.pickups)
                {
                    if (!pickup.isActiveAndEnabled)
                        itms.Add(pickup);
                }
                if (itms.Count == 0) continue;

                room = selection;
                foundItemCount = -1;
                LostItem = possibleItems[id];
                Player = player;
                Pickup = itms[Random.Range(0, itms.Count)];
                Pickup.AssignItem(LostItem);
                Pickup.Hide(hidden: false);
                return id;
            }
            return -1;
        }

        public void CheckForItem()
        {
            if (foundItemCount == -1)
            {
                // Stop method if pickup is still present
                if (Pickup && Pickup.isActiveAndEnabled && Pickup.item == LostItem)
                    return;

                foundItemCount = CountInventoryItems();
            }
            if (foundItemCount > 0 && CountInventoryItems() >= foundItemCount)
                return;

            // GET FUCKING MAD!!!!
            behaviorStateMachine.ChangeState(new Carter_AngryStare(this));
        }

        // How many items does the player now have??
        private sbyte CountInventoryItems()
        {
            sbyte count = 0;
            for (int i = 0; i <= Player.itm.maxItem && i <= Player.itm.items.Length; i++)
            {
                if (Player.itm.items[i] == LostItem)
                    count++;
            }
            return count;
        }

        public void IssueMap()
        {
            paper = CarterHudManager.GetInstance(CoreGameManager.Instance.GetHud(Player.playerNumber)).ActivatePaper(room);
        }

        private void OnDestroy()
        {
            if (paper)
                paper.Deactivate();
        }

        public ItemObject LostItem {get; protected set;}
        public Pickup Pickup {get; protected set;}

        public PlayerManager Player {get; protected set;}

        private RoomController room;
        private CarterPaper paper;

        private sbyte foundItemCount;
    }

    public class Carter_StateBase(Carter carter) : NpcState(carter)
    {
        protected readonly Carter carter = carter;
    }

    public abstract class Carter_Wander(Carter carter) : Carter_StateBase(carter)
    {
        public override void Enter()
        {
            base.Enter();
            npc.navigator.maxSpeed = carter.OriginalSpeed;
            npc.navigationStateMachine.ChangeState(new NavigationState_WanderRandom(npc, 0));
        }
    }

    public class Carter_WanderDefault(Carter carter, float coolDown = 0f) : Carter_Wander(carter)
    {
        private float coolDown = coolDown;

        public override void Update()
        {
            base.Update();
            if (coolDown > 0)
                coolDown -= Time.deltaTime * npc.TimeScale;
        }

        public override void OnStateTriggerStay(Collider other, bool validCollision)
        {
            base.OnStateTriggerStay(other, validCollision);

            if (coolDown > 0 || !validCollision || npc.Blinded ||
                !other.CompareTag("Player") || !other.TryGetComponent(out PlayerManager player) || player.Tagged)
                return;

            int id = carter.TrySpawnItem(player); // Grab ID for carter to reference
            if (id == -1) // No item found
            {
                // Give a 1 second cooldown so it doesn't constantly try to run this operation
                coolDown = 1f;
                return;
            }

            carter.audMan.FlushQueue(true);
            carter.audMan.QueueAudio(carter.audLost[Random.Range(0,carter.audLost.Length)], true);
            carter.audMan.QueueAudio(carter.audItms[id]);
            carter.audMan.QueueAudio(carter.audHelp[Random.Range(0,carter.audHelp.Length)]);

            if (!carter.audMan.QueuedAudioIsPlaying)
                carter.audMan.PlayQueue();

            carter.behaviorStateMachine.ChangeState(new Carter_Request(carter));
        }
    }

    public class Carter_Request(Carter carter) : Carter_StateBase(carter)
    {
        private bool mapIssued = false;

        public override void Enter()
        {
            base.Enter();
            npc.Navigator.maxSpeed = 0f;
            npc.Navigator.Entity.AddForce(new Force(npc.transform.position-carter.Player.transform.position, 6f, -25f));
        }

        public override void Update()
        {
            base.Update();
            carter.CheckForItem();
            if (carter.audMan.QueuedAudioIsPlaying || carter.behaviorStateMachine.CurrentState != this)
                return;

            if (!mapIssued)
            {
                carter.IssueMap();
                carter.audMan.QueueAudio(carter.audCoords, true);
                mapIssued = true;
                return;
            }

            carter.audMan.QueueAudio(carter.audLeave[Random.Range(0,carter.audLeave.Length)], true);
            carter.behaviorStateMachine.ChangeState(new Carter_WanderWaiting(carter));
        }
    }

    public class Carter_WanderWaiting(Carter carter) : Carter_Wander(carter)
    {
        public override void Update()
        {
            base.Update();
            carter.CheckForItem();
        }
    }

    public class Carter_AngryStare(Carter carter) : Carter_StateBase(carter)
    {
        private float stareTime;

        // Silence layer
        private PlayerSilenceManager silencer;
        private bool silenceActive;

        public override void Enter()
        {
            base.Enter();

            Vector3 targetPos = carter.Player.transform.position;
            float dist = 16f;
            if (Physics.Raycast(targetPos, carter.Player.cameraBase.transform.forward, out RaycastHit hit, dist, carter.looker.layerMask))
                dist = hit.distance;
            carter.Navigator.Entity.Teleport(targetPos+carter.Player.cameraBase.transform.forward*dist*0.8f);

            stareTime = Random.Range(1f,9.991f);
            silenceActive = false;
            silencer = carter.Player.GetComponent<PlayerSilenceManager>();
            
            carter.audMan.FlushQueue(true);
            carter.sprite.sprite = carter.sprAngry;
            npc.Navigator.maxSpeed = 0f;
        }

        public override void Unsighted()
        {
            base.Unsighted();
            if (!silenceActive)
            {
                silenceActive = true;
                silencer.Silence(true);
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (silenceActive)
            {
                silenceActive = false;
                silencer.Silence(false);
            }
        }

        public override void Update()
        {
            base.Update();
            stareTime -= Time.deltaTime * npc.TimeScale;
            if (stareTime <= 0f)
                carter.behaviorStateMachine.ChangeState(new Carter_CallingBaldi(carter));
        }
    } 

    public class Carter_CallingBaldi(Carter carter) : Carter_StateBase(carter)
    {
        public override void Enter()
        {
            base.Enter();            
            carter.sprite.sprite = carter.sprScreech;
            carter.audMan.FlushQueue(true);
            carter.audMan.QueueAudio(carter.audIntro);
            carter.audMan.QueueAudio(carter.audLoop);
            carter.audMan.loop = true;
            npc.Navigator.maxSpeed = 0f;
        }


        public override void Update()
        {
        }
    }
}