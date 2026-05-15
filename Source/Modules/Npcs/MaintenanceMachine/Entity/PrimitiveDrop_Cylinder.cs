using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_Cylinder : PrimitiveDrop, IEntityTrigger
    {
        public float time = 30f;
        public SoundObject audSlipLoop;

        private bool slipping;
        private float speed = 30f, endAngle = 20f;
        private Entity slippingEntity;
        private Vector3 direction;
        private readonly MovementModifier moveMod = new(default, 0f);

        protected override void Initialize()
        {
            slipping = false;
        }

        private void OnDestroy()
        {
            if (slippingEntity)
			    slippingEntity.ExternalActivity.moveMods.Remove(moveMod);
        }

        protected override void VirtualUpdate()
        {
            if (slipping)
            {
                if (entity.ExternalActivity.Addend.magnitude > 0f)
                    direction = entity.ExternalActivity.Addend.ZeroOutY().normalized;
                    
                entity.UpdateInternalMovement(direction * speed * entity.Ec.EnvironmentTimeScale);
                moveMod.movementAddend = entity.ExternalActivity.Addend + direction * speed * entity.Ec.EnvironmentTimeScale;
                time -= Time.deltaTime * entity.Ec.EnvironmentTimeScale;
                return;
            }
            entity.UpdateInternalMovement(Vector3.zero);
        }

        public void EntityTriggerStay(Entity otherEntity, Collider other, bool validCollision)
        {
            if (validCollision && Ready && !slipping && otherEntity && otherEntity.Grounded && otherEntity.Velocity.magnitude > 0f)
            {
                entity.OnEntityMoveInitialCollision += OnEntityMoveCollision;
                entity.Teleport(otherEntity.transform.position);
                otherEntity.ExternalActivity.moveMods.Add(moveMod);
                slippingEntity = otherEntity;
                slipping = true;
                entity.ExternalActivity.ignoreFrictionForce = true;
                direction = otherEntity.transform.forward;
                audMan.FlushQueue(true);
                audMan.QueueAudio(audSlipLoop);
                audMan.SetLoop(true);
            }
        }

        private void OnEntityMoveCollision(RaycastHit hit)
        {
            if (!slipping) return;
            if (Vector3.Angle(-direction, hit.normal) <= endAngle || time < 0f)
                Destroy(gameObject);
            direction = Vector3.Reflect(direction, hit.normal);
        }

        public void EntityTriggerExit(Entity otherEntity, Collider other, bool validCollision)
        {
            if (validCollision && slipping && (!slippingEntity || otherEntity == slippingEntity))
                Destroy(gameObject);
        }

        public void EntityTriggerEnter(Entity otherEntity, Collider other, bool validCollision) {}
    }
}