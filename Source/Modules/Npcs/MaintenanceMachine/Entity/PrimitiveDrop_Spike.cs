using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_Spike : PrimitiveDrop
    {
        public float slowdownTime = 10f;
        public SoundObject audTouch;

        protected override void ShapeTriggerEnter(Entity ent, bool validCollision)
        {
            if (validCollision && (ent.CompareTag("Player") || ent.CompareTag("NPC")))
            {
                SetDead();
                audMan.PlaySingle(audTouch);
                ent.ActivateReusableEffect<SpikeSlowdown>(slowdownTime);
            }
        }
    }

    public class SpikeSlowdown : ReusableEffect
    {
        protected override bool Immune
            => EntType == EntityType.Generic; // Cannot stun Non-"NPC" Entities
        protected override Sprite GaugeIcon => RecommendedCharsPlugin.AssetMan.Get<Sprite>("StatusSpr/SpikeSlowdown");

        private readonly MovementModifier moveMod = new(default, 0.4f);

        protected override void Activated()
        {
            if (!Entity.ExternalActivity.moveMods.Contains(moveMod))
                Entity.ExternalActivity.moveMods.Add(moveMod);
        }

        protected override void Deactivated()
        {
            if (Entity.ExternalActivity.moveMods.Contains(moveMod))
                Entity.ExternalActivity.moveMods.Remove(moveMod);
        }

        protected override void Reactivated() {}
        protected override void ActiveUpdate() {}
    }
}