using System.Collections;
using System.Linq;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class Gifter : NPC
    {
        public SpriteRenderer sprite;
        public Sprite sprNoGift, sprGift, sprThrow, sprSad;

        public AudioManager audMan;
        public SoundObject audHumming, audShocked, audSorry, audThrow;
        public SoundObject[] audGift, audOpened, audLeft;

        public ItemMetaData[] items;
        public ItemObject item;

        public QuickExplosion explosionPre;

        public float OriginalSpeed {get; private set;}

        public override void Initialize()
        {
            base.Initialize();
            OriginalSpeed = navigator.maxSpeed;

            items = ItemMetaStorage.Instance.All()
            .Where(x => !x.flags.HasFlag(ItemFlags.InstantUse) && !x.flags.HasFlag(ItemFlags.NoUses)
                && x.value.itemType.ToStringExtended() != "WPB" && !x.tags.Contains("lost_item")
                && !x.tags.Contains("shape_key") && !x.tags.Contains("shop_dummy")
                && !x.tags.Contains("recchars_gifter_blacklist")).ToArray();

            RerollGift();        
            behaviorStateMachine.ChangeState(new Gifter_WanderSimple(this));
        }

        public float defectiveGiftChance = 0.4f;
        public bool DefectiveGift {get; private set;}

        public Vector2 valueRange = new(0,25);

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
        public void HummingChance()
        {
            if (!audMan.QueuedAudioIsPlaying && Random.value <= idleSoundChance)
                audMan.PlaySingle(audHumming);
        }

        public float defaultGiftCooldown = 30f;
        private float giftCooldown = 0f;
        public bool OnCooldown {get; private set;} = false;
        public void ResetCooldown(float time)
        {
            if (time > giftCooldown)
            {
                OnCooldown = true;
                giftCooldown = time;
            }
        }

        public void TickCooldown()
        {
            if (OnCooldown)
            {
                giftCooldown -= Time.deltaTime * TimeScale;
                if (giftCooldown <= 0f)
                {
                    OnCooldown = false;
                    sprite.color = Color.white;
                    sprite.sprite = sprGift;
                }
            }
        }

        public void Explode()
        {
            Instantiate(explosionPre, sprite.transform).transform.localPosition += Vector3.forward * 0.025f;
            sprite.color = Color.gray;
            sprite.sprite = sprSad;
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
            gifter.HummingChance();
        }

        public override void Update()
        {
            base.Update();
            gifter.TickCooldown();
        }
    }

    public class Gifter_WanderSimple(Gifter gifter) : Gifter_Wander(gifter)
    {
        public override void OnStateTriggerEnter(Collider other, bool validCollision)
        {
            base.OnStateTriggerEnter(other, validCollision);
            if (!gifter.OnCooldown && validCollision && !gifter.Blinded && other.CompareTag("Player") &&
                other.TryGetComponent(out PlayerManager player) && npc.looker.PlayerInSight() && !player.Tagged && !player.itm.InventoryFull())
                npc.behaviorStateMachine.ChangeState(new Gifter_GiveGiftDirect(gifter, player));
        }
    }

    public class Gifter_GiveGiftDirect(Gifter gifter, PlayerManager player) : Gifter_StateBase(gifter)
    {
        protected readonly PlayerManager player = player;
        private IEnumerator giveRoutine;

        public override void Enter()
        {
            base.Enter();
            npc.Navigator.maxSpeed = 0f;
            npc.Navigator.Entity.AddForce(new Force(npc.transform.position-player.transform.position, 7f, -35f));
            gifter.audMan.FlushQueue(true);
            gifter.audMan.PlaySingle(gifter.audGift[Random.Range(0,gifter.audGift.Length)]);

            giveRoutine = GiveDelay();
            gifter.StartCoroutine(giveRoutine);
        }

        public override void Exit()
        {
            base.Exit();
            npc.Navigator.maxSpeed = gifter.OriginalSpeed;

            if (giveRoutine != null)
                gifter.StopCoroutine(giveRoutine);
        }

        private IEnumerator GiveDelay()
        {
            Vector3 forward = (player.transform.position-npc.transform.position).normalized * 2f;
            yield return new WaitWhile(() => gifter.audMan.QueuedAudioIsPlaying);
            if (gifter.DefectiveGift)
            {
                gifter.audMan.PlaySingle(gifter.audShocked);
                yield return new WaitForSecondsNPCTimescale(npc, 1.25f);
                gifter.Explode();
                yield return new WaitWhile(() => gifter.audMan.QueuedAudioIsPlaying);
                gifter.audMan.PlaySingle(gifter.audSorry);
            }
            else
            {
                gifter.sprite.sprite = gifter.sprNoGift;
                if (player.itm.InventoryFull()) // Edge case in case the sneaky player fills up their inventory midway
                {
                    forward = npc.transform.position+forward;
                    Pickup pickup = npc.ec.CreateItem(npc.ec.CellFromPosition(npc.transform.position).room, gifter.item, new(forward.x, forward.z));
                    Debug.Log(pickup.name);
                }
                else
                    player.itm.AddItem(gifter.item);
                
                gifter.audMan.PlaySingle(gifter.audOpened[Random.Range(0,gifter.audOpened.Length)]);
            }
            gifter.RerollGift(gifter.DefectiveGift);
            gifter.ResetCooldown(gifter.defaultGiftCooldown);
            npc.behaviorStateMachine.ChangeState(new Gifter_WanderSimple(gifter));
        }
    }
}