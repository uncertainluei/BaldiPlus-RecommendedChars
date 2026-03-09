using System.Collections;
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
            RoomCategory.Office,
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

                LostItem = possibleItems[id];
                Player = player;
                Pickup = itms[Random.Range(0, itms.Count)];
                Pickup.AssignItem(LostItem);
                Pickup.Hide(hidden: false);
                return id;
            }
            return -1;
        }

        public ItemObject LostItem {get; protected set;}
        public Pickup Pickup {get; protected set;}

        public PlayerManager Player {get; protected set;}
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
            npc.Navigator.Entity.AddForce(new Force(npc.transform.position-carter.Player.transform.position, 4f, -25f));
        }

        public override void Update()
        {
            base.Update();
            if (carter.audMan.QueuedAudioIsPlaying)
                return;

            if (!mapIssued)
            {
                // ADD THE MAP PAPER TO THE HUD (this might be tricky atm so I'll hold off for now)

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
        }
    }
}