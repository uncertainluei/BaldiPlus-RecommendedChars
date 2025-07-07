using System.Linq;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_CherryBsoda : Item, IEntityTrigger
    {
        public ITM_BSODA bsoda;
        protected EnvironmentController Ec => bsoda.ec;
        protected MovementModifier MoveMod => bsoda.moveMod;
        protected Entity Entity => bsoda.entity;

        public LayerMaskObject layerMask;
        public SoundObject boing;

        protected PlayerManager currentPlayer;

        protected float Speed => bsoda.speed;
        public byte bouncesLeft = 5;

        public override bool Use(PlayerManager pm)
        {
            bsoda.Use(pm);
            currentPlayer = pm;

            Entity.OnEntityMoveInitialCollision += OnEntityMoveCollision;
            // Remove the ITM_BSODA EntityTrigger as it is grabbed by the Entity regardless of the component being disabled
            Entity.iEntityTrigger = Entity.iEntityTrigger.Where(x => x is not ITM_BSODA).ToArray();

            currentPlayer.plm.Entity.ExternalActivity.moveMods.Add(MoveMod);
            bsoda.launching = true;
            return true;
        }

        private void Update()
        {
            MoveMod.movementAddend = Entity.ExternalActivity.Addend + transform.forward * Speed * Ec.EnvironmentTimeScale;
            Entity.MoveWithCollision(transform.forward * Speed * Ec.EnvironmentTimeScale * Time.deltaTime);

            bsoda.time -= Time.deltaTime * Ec.EnvironmentTimeScale;
            if (bsoda.time > 0f) return;

            Destroy();
        }

        public void EntityTriggerEnter(Collider other)
        {
            bsoda.EntityTriggerEnter(other);
        }

        public void EntityTriggerExit(Collider other)
        {
            bsoda.EntityTriggerExit(other);
            if (other.CompareTag("Player") && other.transform == currentPlayer.transform && currentPlayer.plm.am.moveMods.Contains(MoveMod))
            {
                currentPlayer.plm.am.moveMods.Remove(MoveMod);
                bsoda.time *= 0.66f;
            }
        }

        public void EntityTriggerStay(Collider other)
        {
        }

        protected void Destroy()
        {
            if (currentPlayer.plm.am.moveMods.Contains(MoveMod))
                currentPlayer.plm.am.moveMods.Remove(MoveMod);

            foreach (ActivityModifier activityMod in bsoda.activityMods)
                activityMod?.moveMods.Remove(MoveMod);

            Destroy(gameObject);
        }
        
        private void OnEntityMoveCollision(RaycastHit hit)
        {
            if (layerMask.Contains(hit.collider.gameObject.layer))
            {
                bouncesLeft--;
                if (bouncesLeft == 0)
                    Destroy();

                CoreGameManager.Instance.audMan.PlaySingle(boing);
                transform.forward = transform.forward - (2f * Vector3.Dot(hit.normal, transform.forward) * hit.normal);
                MoveMod.movementAddend = Entity.ExternalActivity.Addend + transform.forward * Speed * Ec.EnvironmentTimeScale;
                Entity.MoveWithCollision(transform.forward * Speed * Ec.EnvironmentTimeScale * Time.deltaTime);
            }
        }
    }
}
