using MTM101BaldAPI.Registers;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class BsodaaHelper : EnvironmentObject, IClickable<int>
    {
        private bool sprayed;
        private byte bsodaCount = 3;

        public AudioManager audMan;
        public SoundObject[] sodaCount;
        public SoundObject audGiveSoda;
        public SoundObject audOutOf;
        public SoundObject audSad;

        public ItemObject itmDietBsoda;
        public ItemObject itmBsoda;

        public SpriteRenderer sprite;
        public Sprite sprEmpty;
        public Sprite sprSprayed;

        public bool ClickableHidden()
        {
            return sprayed;
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

        private PlayerManager _player;
        public void Clicked(int player)
        {
            if (ClickableHidden()) return;

            _player = CoreGameManager.Instance.GetPlayer(player);
            if (!_player.Tagged || bsodaCount == 0 || _player.itm.InventoryFull())
            {
                if (!audMan.QueuedAudioIsPlaying)
                    audMan.PlaySingle(audOutOf);
                return;
            }

            _player.itm.AddItem(itmDietBsoda);
            bsodaCount--;
            audMan.FlushQueue(true);
            audMan.QueueAudio(audGiveSoda);

            if (bsodaCount == 0)
                sprite.sprite = sprEmpty;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!sprayed && other.isTrigger && other.TryGetComponent(out ITM_BSODA bsoda) && other.name == "ITM_BSODA(Clone)")
            {
                sprayed = true;
                audMan.FlushQueue(true);
                audMan.PlaySingle(audSad);
                sprite.sprite = sprSprayed;

                // Drop the two BSODAs here
                RoomController room = ec.CellFromPosition(transform.position).room;

                Vector3 dropLocation = transform.position + transform.forward * 2f + transform.right * 3f;
                Pickup pickup = ec.CreateItem(room, itmBsoda, new Vector2(dropLocation.x, dropLocation.z));
                pickup.icon = room.ec.map.AddIcon(pickup.iconPre, pickup.transform, Color.white);
                dropLocation += transform.right * -6f;
                pickup = ec.CreateItem(room, itmBsoda, new Vector2(dropLocation.x, dropLocation.z));
                pickup.icon = room.ec.map.AddIcon(pickup.iconPre, pickup.transform, Color.white);
            }
        }
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
