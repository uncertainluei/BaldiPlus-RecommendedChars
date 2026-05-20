using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_Cylinder : PrimitiveDrop
    {
        public float time = 30f;
        public SoundObject audSlipLoop, audSlipEnd;

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
            if (!slipping)
            {
                entity.UpdateInternalMovement(Vector3.zero);
                return;
            }
            if (entity.ExternalActivity.Addend.magnitude > 0f)
                direction = entity.ExternalActivity.Addend.ZeroOutY().normalized;
                    
            entity.UpdateInternalMovement(direction * speed * entity.Ec.EnvironmentTimeScale);
            moveMod.movementAddend = entity.ExternalActivity.Addend + direction * speed * entity.Ec.EnvironmentTimeScale;
            time -= Time.deltaTime * entity.Ec.EnvironmentTimeScale;
        }

        protected override void ShapeTriggerStay(Entity ent, bool validCollision)
        {
            if (validCollision && !slipping && ent.Grounded && ent.Velocity.magnitude > 0f)
            {
                entity.OnEntityMoveInitialCollision += OnEntityMoveCollision;
                entity.Teleport(ent.transform.position);
                ent.ExternalActivity.moveMods.Add(moveMod);
                slippingEntity = ent;
                slipping = true;
                entity.ExternalActivity.ignoreFrictionForce = true;
                direction = ent.transform.forward;
                audMan.FlushQueue(true);
                audMan.QueueAudio(audSlipLoop);
                audMan.SetLoop(true);
            }
        }

        protected override void ShapeTriggerExit(Entity ent, bool validCollision)
        {
            if (validCollision && slipping && (!slippingEntity || ent == slippingEntity))
                End();
        }

        private void OnEntityMoveCollision(RaycastHit hit)
        {
            if (!slipping) return;
            if (Vector3.Angle(-direction, hit.normal) <= endAngle || time < 0f)
                End();
            direction = Vector3.Reflect(direction, hit.normal);
        }

        private void End()
        {
            entity.UpdateInternalMovement(-direction * speed * entity.Ec.EnvironmentTimeScale);
            SetDead();
            audMan.FlushQueue(true);
            audMan.PlaySingle(audSlipEnd);
            if (slippingEntity)
			    slippingEntity.ExternalActivity.moveMods.Remove(moveMod);
        }
    }
}