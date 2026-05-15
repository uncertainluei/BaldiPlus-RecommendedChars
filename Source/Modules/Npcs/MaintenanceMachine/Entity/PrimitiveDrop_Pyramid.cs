using System.Linq;
using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_Pyramid : PrimitiveDrop
    {
        public float flipTime = 10f;

        protected override void ShapeTriggerEnter(Entity ent, bool validCollision)
        {
            if (validCollision && ent)
            {
                ent.ActivateReusableEffect<PyramidFlip>(flipTime);
                Destroy(gameObject);
            }
        }
    }

    public class PyramidFlip : ReusableEffect
    {
        protected override Sprite GaugeIcon => RecommendedCharsPlugin.AssetMan.Get<Sprite>("StatusSpr/PyramidFlip");

        private static GravityEvent _gravityEvent;
        
        private void Flip()
        {
            if (!_gravityEvent || !_gravityEvent.Active)
            {
                if (!BaseGameManager.Instance.Ec.CurrentEventTypes.Contains(RandomEventType.Gravity))
                {
                    // Just flip the entity if there is no gravity event
                    Entity.Flip();
                    return;
                }
                // Grab active gravity event
                _gravityEvent = (GravityEvent)BaseGameManager.Instance.Ec.currentEvents.First(x => x is GravityEvent);
            }
            switch (EntType)
            {
                case EntityType.Player:
                    _gravityEvent.FlipPlayer();
                    return;
                case EntityType.Npc:
                    _gravityEvent.FlipNPC(Npc);
                    return;
                default:
                    Entity.Flip();
                    return;
            }
        }

        protected override void Activated() => Flip();
        protected override void Deactivated() => Flip();

        protected override void Reactivated() {}
        protected override void ActiveUpdate() {}
    }
}