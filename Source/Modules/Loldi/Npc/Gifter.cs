using System.Linq;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class Gifter : NPC
    {
        public SpriteRenderer sprite;
        public Sprite sprNoGift, sprGift, sprThrow, sprSad;

        public AudioManager audMan;
        public SoundObject audHumbling, audShocked, audSorry, audThrow;
        public SoundObject[] audGift, audOpened, audLeft;

        public ItemObject item;

        public float giftCooldown;

        public override void Initialize()
        {
            base.Initialize();
            items = ItemMetaStorage.Instance.GetAllWithoutFlags(ItemFlags.InstantUse);
            RerollGift();        
            behaviorStateMachine.ChangeState(new Gifter_Wander(this));
        }

        public float defectiveGiftChance = 0.4f;
        public bool DefectiveGift {get; private set;}

        public Vector2 valueRange = new(0,25);
        private ItemMetaData[] items;

        public void RerollGift(bool forceNonDefective = false)
        {
            if (!forceNonDefective && Random.value <= defectiveGiftChance)
            {
                DefectiveGift = true;
                return;
            }
            DefectiveGift = false;
            item = items[Random.Range(0,items.Length)].value;
        }

        public float idleSoundChance = 0.1f;
        public void HumblingChance()
        {
            if (!audMan.QueuedAudioIsPlaying && Random.value <= idleSoundChance)
                audMan.PlaySingle(audHumbling);
        }
    }

    public class Gifter_StateBase(Gifter gifter) : NpcState(gifter)
    {
        protected readonly Gifter gifter = gifter;
    }

    public class Gifter_Wander(Gifter gifter) : Gifter_StateBase(gifter)
    {

        public override void Enter()
        {
            base.Enter();
            npc.behaviorStateMachine.ChangeNavigationState(new NavigationState_WanderRandom(npc, 0, true));
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            gifter.HumblingChance();
        }
    }

    public class Gifter_WanderSimple(Gifter gifter) : Gifter_Wander(gifter)
    {
        public override void OnStateTriggerEnter(Collider other, bool validCollision)
        {
            base.OnStateTriggerEnter(other, validCollision);
            if (validCollision && other.CompareTag("Player") && !other.GetComponent<ItemManager>().InventoryFull())
                return;
        }
    }

    public class Gifter_GiveGift(Gifter gifter, PlayerManager player) : Gifter_StateBase(gifter)
    {
        protected readonly PlayerManager player = player;

        public override void Enter()
        {
            base.Enter();
            gifter.audMan.FlushQueue(true);
            gifter.audMan.PlaySingle(gifter.audGift[Random.Range(0,gifter.audGift.Length)]);
        }

        public override void Update()
        {
            base.Update();
        }
    }
}