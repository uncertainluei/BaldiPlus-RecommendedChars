using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class BsodaaHelper : EnvironmentObject, IClickable<int>
    {
        public bool InStock { get; private set; }
        private byte bsodaCount = 3;
        private byte countUntilSmall = 3;

        private bool sprayed;
        private bool dropDiet;

        public BsodaaRoomFunction bsodaaRoom;

        public AudioManager audMan;
        public SoundObject[] audCount;
        public SoundObject audLaugh;
        public SoundObject audOops;
        public SoundObject audSad;

        public ItemObject itmSmallBsoda;
        public ItemObject itmDietBsoda;
        public ItemObject itmBsoda;

        public SpriteRenderer sprite;
        public Sprite sprEmpty;
        public Sprite sprSprayed;

        public bool ClickableHidden()
        {
            return !InStock;
        }

        public bool ClickableRequiresNormalHeight()
        {
            return true;
        }

        public void ClickableSighted(int player)
        {
            _player = CoreGameManager.Instance.GetPlayer(player);
        }

        public void ClickableUnsighted(int player)
        {
        }

        private int lastPlayer = -1;
        private PlayerManager _player;
        public void Clicked(int player)
        {
            if (ClickableHidden()) return;

            _player = CoreGameManager.Instance.GetPlayer(player);
            if (!_player.Tagged || (lastPlayer >= 0 && lastPlayer != player) || bsodaCount == 0 || _player.itm.InventoryFull())
            {
                if (!audMan.QueuedAudioIsPlaying)
                    audMan.PlaySingle(audOops);
                return;
            }

            lastPlayer = player;

            audMan.QueueAudio(audCount[3-bsodaCount]);
            bsodaCount--;

            // Give Diet BSODA Minis if the player takes more than three during a run
            if (countUntilSmall == 0)
                _player.itm.AddItem(itmSmallBsoda);
            else
            {
                countUntilSmall--;
                _player.itm.AddItem(itmDietBsoda);
            }

            if (bsodaCount > 0) return;
            if (bsodaaRoom == null || !bsodaaRoom.MachinesInStock)
            {
                InStock = false;
                sprite.sprite = sprEmpty;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!sprayed && other.isTrigger && other.GetComponent<VanillaBsodaComponent>() != null)
            {
                sprayed = true;
                InStock = false;
                audMan.FlushQueue(true);
                audMan.PlaySingle(audSad);
                sprite.sprite = sprSprayed;

                // Drop the two BSODAs here (Diet BSODAs if it's the second time)
                RoomController room = ec.CellFromPosition(transform.position).room;
                ItemObject itemToDrop = itmBsoda;
                if (dropDiet)
                    itemToDrop = itmDietBsoda;

                Vector3 dropLocation = transform.position + transform.forward * 2f + transform.right * 3f;
                Pickup pickup = ec.CreateItem(room, itemToDrop, new Vector2(dropLocation.x, dropLocation.z));
                pickup.icon = room.ec.map.AddIcon(pickup.iconPre, pickup.transform, Color.white);
                dropLocation += transform.right * -6f;
                pickup = ec.CreateItem(room, itemToDrop, new Vector2(dropLocation.x, dropLocation.z));
                pickup.icon = room.ec.map.AddIcon(pickup.iconPre, pickup.transform, Color.white);
            }
        }

        public override void LoadingFinished()
        {
            base.LoadingFinished();
            InStock = true;

            Cell currentCell = ec.CellFromPosition(transform.position);
            RoomController room;
            if (currentCell != null && (room = currentCell.room) != null)
                bsodaaRoom = room.functionObject.GetComponent<BsodaaRoomFunction>();
        }

        public void FixedUpdate()
        {
            // Reset player's counter once their Faculty Nametag(s) run out
            if (lastPlayer >= 0 && !CoreGameManager.Instance.GetPlayer(lastPlayer).Tagged)
            {
                lastPlayer = -1;
                bsodaCount = 3;
            }
        }

        public void Restock()
        {
            audMan.FlushQueue(true);
            audMan.PlaySingle(audLaugh);
        }
    }

    // Dummy component so we can keep track on what's a (regular) BSODA spray and not a variant
    public class VanillaBsodaComponent : MonoBehaviour
    {
    }

    public class BsodaaHelperDummyNpc : NPC
    {
        // Instantly despawn upon spawning
        public override void Initialize()
        {
            Despawn();
        }
    }
}
