using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_RoundedCuboid : PrimitiveDrop, IEntityTrigger
    {
        public float setTime = 20f;
        public SoundObject audSlip;
        public SpriteRenderer puddleSprite;

        private readonly float slipSpeed = 40f, slipAccel = -32f;

        private bool molten;
        private float time, multiplier;

        protected override void Initialize()
        {
            time = setTime;
            puddleSprite.enabled = false;
        }

        protected override void VirtualUpdate()
        {
            if (molten) return;

            time -= Time.deltaTime * entity.Ec.EnvironmentTimeScale;
            multiplier = time/setTime;
            if (time <= 0f)
            {
                molten = true;
                sprite.enabled = false;
                entity.SetFrozen(true);
            }
        }

        private void LateUpdate()
        {
            if (!Ready || molten) return;

            entity._propertyBlock ??= new();
            sprite.GetPropertyBlock(entity._propertyBlock);
			entity._propertyBlock.SetFloat("_PercentInvisible", 1-(1-entity.hiddenPercent)*multiplier);
			sprite.SetPropertyBlock(entity._propertyBlock);

            multiplier = Mathf.Max(0f,2f-multiplier*2f-1.5f);
            if (!puddleSprite.enabled && multiplier > 0f)
                puddleSprite.enabled = true;

            puddleSprite.GetPropertyBlock(entity._propertyBlock);
			entity._propertyBlock.SetFloat("_PercentInvisible", 1-(1-entity.hiddenPercent)*multiplier);
			puddleSprite.SetPropertyBlock(entity._propertyBlock);
        }

        public void EntityTriggerEnter(Entity otherEntity, Collider other, bool validCollision)
        {
            if (molten && otherEntity && entity.Flipped == otherEntity.Flipped)
            {
                audMan.PlaySingle(audSlip);
                otherEntity.AddForce(new Force(otherEntity.Velocity.normalized, slipSpeed, slipAccel));
            }
        }

        public void EntityTriggerExit(Entity otherEntity, Collider other, bool validCollision) {}
        public void EntityTriggerStay(Entity otherEntity, Collider other, bool validCollision) {}
    }
}