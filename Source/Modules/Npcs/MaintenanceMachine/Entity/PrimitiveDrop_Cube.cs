using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_Cube : PrimitiveDrop, IEntityTrigger
    {
        public float squishTime = 10f;
        public SoundObject audSquish;

        private bool up;
        private readonly List<Entity> entities = [];

        protected override void Initialize()
        {
            entity.ignoreOrientation = true;
            gravity = 15f;
            endHeight = 7.5f;
            up = true;
        }

        protected override void VirtualUpdate()
        {
            if (Falling || up) return;

            audMan.PlaySingle(audSquish);
            foreach (Entity ent in entities)
            {
                if (!ent || ent.Flipped != entity.Flipped) continue;
                ent.Squish(squishTime);
            }
            entities.Clear();
            up = true;
            Fall(endHeight, gravity);
        }

        public void EntityTriggerStay(Entity otherEntity, Collider other, bool validCollision)
        {
            if (!entities.Contains(otherEntity))
                entities.Add(otherEntity);
            if (up && !Falling && otherEntity.Flipped == entity.Flipped)
            {
                // Set to fall
                up = false;
                Fall(0f, -70f, true);
            }
        }

        public void EntityTriggerExit(Entity otherEntity, Collider other, bool validCollision)
        {
            if (entities.Contains(otherEntity))
                entities.Remove(otherEntity);
        }

        public void EntityTriggerEnter(Entity otherEntity, Collider other, bool validCollision) {}
    }
}