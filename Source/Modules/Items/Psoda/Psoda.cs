using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_Psoda : ITM_BSODA, IEntityTrigger
    {
        public LayerMaskObject layerMask;
        public SoundObject boing;
        public AudioManager audMan;

        public override bool Use(PlayerManager pm)
        {
            base.Use(pm);
            this.pm = pm;

            entity.OnEntityMoveInitialCollision += OnEntityMoveCollision;
            // Remove the ITM_BSODA EntityTrigger as it is grabbed by the Entity regardless of the component being disabled
            //Entity.iEntityTrigger = Entity.iEntityTrigger.Where(x => x is not ITM_BSODA).ToArray();

            this.pm.plm.am.moveMods.Add(moveMod);
            activityMods.Add(this.pm.plm.am);
            launching = true;
            return true;
        }

        private new void Update()
        {
            moveMod.movementAddend = entity.ExternalActivity.Addend + transform.forward * speed * ec.EnvironmentTimeScale;
            entity.MoveWithCollision(transform.forward * speed * ec.EnvironmentTimeScale * Time.deltaTime);

            if (activityMods.Count > 0)
                time -= activityMods.Count * Time.deltaTime * ec.EnvironmentTimeScale;
            if (time > 0f) return;
            Destroy();
        }


        private Collider _lastCollider;
        private bool _lastIsBsoda;

        private bool IsColliderBsoda(Collider collider)
        {
            if (collider != _lastCollider)
            {
                _lastCollider = collider;
                _lastIsBsoda = collider.GetComponent<ITM_BSODA>();
            }
            return _lastIsBsoda;
        }

        public new void EntityTriggerEnter(Entity ent, Collider other, bool validCollision)
        {
            if (!IsColliderBsoda(other))
                base.EntityTriggerEnter(ent, other, validCollision);
        }

        public new void EntityTriggerExit(Entity ent, Collider other, bool validCollision)
        {
            if (IsColliderBsoda(other)) return;
            base.EntityTriggerExit(ent, other, validCollision);
            if (other.CompareTag("Player") && other.transform == pm.transform && pm.plm.am.moveMods.Contains(moveMod))
            {
                activityMods.Remove(pm.plm.am);
                pm.plm.am.moveMods.Remove(moveMod);
            }
        }

        public new void EntityTriggerStay(Entity ent, Collider other, bool validCollision)
        {
        }

        protected void Destroy()
        {
            foreach (ActivityModifier activityMod in activityMods)
                activityMod?.moveMods.Remove(moveMod);

            Destroy(gameObject);
        }
        
        private void OnEntityMoveCollision(RaycastHit hit)
        {
            if (!layerMask.Contains(hit.collider.gameObject.layer) || IsColliderBsoda(hit.collider))
                return;
            
            audMan.PlaySingle(boing);
            transform.forward = Vector3.Reflect(transform.forward, hit.normal);
            moveMod.movementAddend = entity.ExternalActivity.Addend + transform.forward * speed * ec.EnvironmentTimeScale;
            entity.MoveWithCollision(transform.forward * speed * ec.EnvironmentTimeScale * Time.deltaTime);
        }
    }
}
