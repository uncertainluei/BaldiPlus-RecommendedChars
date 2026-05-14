using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_Spike : PrimitiveDrop, IEntityTrigger
    {
        public float slowdownTime = 10f;

        public void EntityTriggerEnter(Entity otherEntity, Collider other, bool validCollision)
        {
            if (validCollision)
                otherEntity.ActivateReusableEffect<SpikeSlowdown>(10f);
        }

        public void EntityTriggerExit(Entity otherEntity, Collider other, bool validCollision)
        {
        }

        public void EntityTriggerStay(Entity otherEntity, Collider other, bool validCollision)
        {
        }
    }

    public class SpikeSlowdown : ReusableEffect
    {
        protected override bool Immune
            => EntType == EntityType.Generic; // Cannot stun Non-"NPC" Entities
        protected override Sprite GaugeIcon => RecommendedCharsPlugin.AssetMan.Get<Sprite>("StatusSpr/SpikeSlowdown");

        private readonly MovementModifier moveMod = new(default, 0.5f);

        protected override void Activated()
        {
            if (!Entity.ExternalActivity.moveMods.Contains(moveMod))
                Entity.ExternalActivity.moveMods.Add(moveMod);
        }

        protected override void Reactivated()
        {
        }

        protected override void ActiveUpdate()
        {
        }

        protected override void Deactivated()
        {
            if (Entity.ExternalActivity.moveMods.Contains(moveMod))
                Entity.ExternalActivity.moveMods.Remove(moveMod);
        }
    }
}