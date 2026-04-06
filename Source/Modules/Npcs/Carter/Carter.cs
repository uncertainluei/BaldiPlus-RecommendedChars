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

                if (paper) paper.Deactivate();
                paper = null;

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
            if (paper) paper.Deactivate();
            paper = null;
        }

        public ItemObject LostItem {get; protected set;}
        public Pickup Pickup {get; protected set;}

        public PlayerManager Player {get; protected set;}

        private RoomController room;
        private CarterPaper paper;

        private sbyte foundItemCount;
        public bool FoundItem => foundItemCount > 0;
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
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }
    }

    public class Carter_WanderDefault(Carter carter, float coolDown = 0f) : Carter_Wander(carter)
    {
        private float coolDown = coolDown;

        public override void Enter()
        {
            base.Enter();
            carter.sprite.sprite = carter.sprNormal;
        }

        public override void Update()
        {
            base.Update();
            if (coolDown > 0)
                coolDown -= Time.deltaTime * npc.TimeScale;
        }

        public override void OnStateTriggerStay(Entity ent, Collider other, bool valid)
        {
            base.OnStateTriggerStay(ent, other, valid);

            if (coolDown > 0 || !valid || npc.Blinded ||
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
            carter.audMan.QueueAudio(carter.audLost[Random.Range(0,carter.audLost.Length)]);
            carter.audMan.QueueAudio(carter.audItms[id]);
            carter.audMan.QueueAudio(carter.audHelp[Random.Range(0,carter.audHelp.Length)]);

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
        private bool following = false;
        private NavigationState_TargetPlayer targetPlayer = new(carter, 63, Vector3.zero);

        public override void Update()
        {
            base.Update();
            carter.CheckForItem();
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (!carter.FoundItem || player != carter.Player)
                return;

            targetPlayer.UpdatePosition(player.transform.position);
            if (!following)
                ChangeNavigationState(targetPlayer);
            
            following = true;
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            following = false;
            Enter();
        }

        public override void OnStateTriggerEnter(Entity ent, Collider other, bool valid)
        {
            base.OnStateTriggerEnter(ent, other, valid);
            if (!valid || !following) return;
            if (!other.CompareTag("Player") || !other.transform == carter.Player.transform) return;

            carter.audMan.FlushQueue(true);
            carter.audMan.PlaySingle(carter.audThanks);
            CoreGameManager.Instance.AddPoints(30, carter.Player.playerNumber, true);

            for (int i = 0; i <= carter.Player.itm.maxItem && i <= carter.Player.itm.items.Length; i++)
            {
                if (carter.Player.itm.items[i] != carter.LostItem) continue;
                carter.Player.itm.RemoveItem(i);
                break;
            }
            carter.behaviorStateMachine.ChangeState(new Carter_WanderDefault(carter, 30f));
        }
    }

    public class Carter_AngryStare(Carter carter) : Carter_StateBase(carter)
    {
        private float stareTime;

        // Silence layer
        private PlayerSilenceManager silencer;
        private bool silenceActive;

        public override void Initialize()
        {
            base.Initialize();

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
        public Carter Carter => carter;

        private Baldi baldi;

        public override void Enter()
        {
            base.Enter();            
            carter.sprite.sprite = carter.sprScreech;
            carter.audMan.FlushQueue(true);
            carter.audMan.QueueAudio(carter.audIntro);
            carter.audMan.QueueAudio(carter.audLoop);
            carter.audMan.loop = true;
            npc.Navigator.maxSpeed = 0f;

            baldi = carter.ec.GetBaldi();
            if (!baldi)
            {
                Done();
                return;
            }

            baldi.behaviorStateMachine.ChangeState(new Baldi_CarterResponse(baldi, this, baldi.behaviorStateMachine.CurrentState));
        }

        public override void Update()
        {
            base.Update();
            if (!baldi)
                Done();
        }

        public void Done()
        {
            carter.behaviorStateMachine.ChangeState(new Carter_AngryRecoil(carter));
        }
    }

    public class Baldi_CarterResponse : Baldi_SubState
    {
        private Carter_CallingBaldi carterState;
        private readonly NpcState inheritor;
        private int angerVal;


        public Baldi_CarterResponse(Baldi baldi, Carter_CallingBaldi carterState, NpcState previousState) : base(baldi, baldi, previousState)
        {
            this.carterState = carterState;

            // Grab inheriting state (ideally to get his/other teachers' slapping routines)
            inheritor = previousState;
            while (inheritor is Baldi_SubState state)
                inheritor = state.previousState;
        }

        public override void Initialize()
        {
            base.Initialize();

            angerVal = BaseGameManager.Instance.NotebookTotal;
            baldi.GetAngry(angerVal);
            baldi.Hear(null, carterState.Carter.transform.position, 127, true); // Force Baldicator
            ChangeNavigationState(new NavigationState_TargetPosition(npc, 127, carterState.Carter.transform.position));
        }

        public override void Enter()
        {
            base.Enter();
            inheritor.Enter();
        }

        public override void Update() => inheritor.Update();
        public override void DoorHit(StandardDoor door) => inheritor.DoorHit(door);
        public override void OnStateTriggerStay(Entity ent, Collider other, bool valid)
            => inheritor.OnStateTriggerStay(ent, other, valid);
        public override void OnStateTriggerEnter(Entity ent, Collider other, bool valid)
            => inheritor.OnStateTriggerEnter(ent, other, valid);
        public override void OnStateTriggerExit(Entity ent, Collider other, bool valid)
            => inheritor.OnStateTriggerExit(ent, other, valid);
        public override void NavigationStateChanged() => inheritor.NavigationStateChanged();


        public override void Hear(GameObject source, Vector3 position, int value)
        {
            // Baldi will ALWAYS treat other noises as 'low-priority' in this state
            int currentVal = baldi.currentSoundVal;
            baldi.currentSoundVal = Mathf.Max(currentVal, value)+1;
            inheritor.Hear(source, position, value);
            baldi.currentSoundVal = currentVal;
        }

        public override void PlayerInSight(PlayerManager player)
        {
            currentNavigationState.priority = 0;
            inheritor.PlayerInSight(player);
            npc.behaviorStateMachine.ChangeState(previousState);
        }

        public override void ActivateSlapAnimation()
        {
            if (inheritor is Baldi_StateBase state)
                state.ActivateSlapAnimation();
        }

        public override void Resume()
        {
            inheritor.Resume();
            npc.behaviorStateMachine.ChangeState(previousState);
        }

        public override void DestinationEmpty()
        {
            currentNavigationState.priority = 0;
            inheritor.DestinationEmpty();
            npc.behaviorStateMachine.ChangeState(previousState);
        }

        public override void Exit()
        {
            inheritor.Exit();

            currentNavigationState.priority = 0;
            baldi.GetAngry(-angerVal);
            
            if (carterState != null && carterState.Carter)
                carterState.Done();
            carterState = null;
        }
    }

    public class Carter_AngryRecoil(Carter carter) : Carter_StateBase(carter)
    {
        private float time;

        public override void Enter()
        {
            base.Enter();
            time = Random.Range(1f, 4f);
            carter.sprite.sprite = carter.sprAngry;
            carter.audMan.FadeOut(0.15f);
        }

        public override void Update()
        {
            base.Update();
            if (carter.audMan.QueuedAudioIsPlaying)
                return;

            time -= Time.deltaTime * npc.TimeScale;
            if (time <= 0f)
                carter.behaviorStateMachine.ChangeState(new Carter_WanderDefault(carter, 30f));
        }
    }
}