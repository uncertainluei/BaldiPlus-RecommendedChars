using System.Collections;
using MTM101BaldAPI;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_Pie : Item, IEntityTrigger
    {
        public GameObject flyingSprite, splatSprite;
        public Material noBillboardMat;
        public Entity entity;

        public AudioManager audMan;
        public SoundObject audThrow, audSplat;

        private float speed = 30f, setTime = 15f;
        private bool flying = true;

        private EnvironmentController ec;
        private readonly MovementModifier moveMod = new(Vector3.zero, 0.8f);

        public override bool Use(PlayerManager pm)
        {
            ec = pm.ec;

            transform.position = pm.transform.position;
            transform.forward = CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward;

            entity.Initialize(ec, transform.position);
            entity.CopyStatusEffects(pm.plm.Entity);
            entity.OnEntityMoveInitialCollision += OnWallCollision;

            CoreGameManager.Instance.audMan.PlaySingle(audThrow);
            DaycareGuiltManager.GetInstance(pm).BreakRule("Throwing", 0.8f, 0.25f);
            moveMod.priority = 1;
            return true;
        }

        private void Update()
        {
            if (flying)
                entity.UpdateInternalMovement(transform.forward * speed * ec.EnvironmentTimeScale);
        }

        private Entity affectedEntity;
        public void EntityTriggerEnter(Entity ent, Collider other, bool valid)
        {
            if (!valid || !flying || !other.isTrigger || !other.CompareTag("NPC")) return;

            flying = false;
            affectedEntity = ent;
            flyingSprite.SetActive(false);
            splatSprite.SetActive(true);
            ent.SetBlinded(true);
            ent.ExternalActivity.moveMods.Add(moveMod);
            StartCoroutine(NpcHitTimer(setTime));
            audMan.FlushQueue(true);
            audMan.PlaySingle(audSplat);
            return;
        }

        private void OnWallCollision(RaycastHit hit)
        {
            if (flying && hit.transform.gameObject.layer != 2)
            {
                flying = false;
                entity.SetFrozen(true);
                affectedEntity = null;
                transform.rotation = Quaternion.LookRotation(hit.normal * -1f, Vector3.up);
                transform.position = hit.point;
                flyingSprite.SetActive(false);
                splatSprite.SetActive(true);
                splatSprite.GetComponent<SpriteRenderer>().sharedMaterial = noBillboardMat;
                splatSprite.transform.localPosition = Vector3.forward * 0.8f;
                StartCoroutine(WallHitTimer(setTime));
                audMan.FlushQueue(true);
                audMan.PlaySingle(audSplat);
            }
        }

        private IEnumerator NpcHitTimer(float time)
        {
            while (time > 0f)
            {
                if (!affectedEntity)
                {
                    Destroy(gameObject);
                    yield break;
                }
                time -= Time.deltaTime * ec.EnvironmentTimeScale;
                entity.UpdateInternalMovement(Vector3.zero);
                transform.position = affectedEntity.transform.position;
                yield return null;
            }
            affectedEntity.ExternalActivity.moveMods.Remove(moveMod);
            affectedEntity.SetBlinded(false);
            Destroy(gameObject);
        }

        private IEnumerator WallHitTimer(float time)
        {
            yield return new WaitForSecondsEnvironmentTimescale(ec, time);
            Destroy(gameObject);
        }

        public void EntityTriggerExit(Entity ent, Collider other, bool valid)
        {
        }
        public void EntityTriggerStay(Entity ent, Collider other, bool valid)
        {
        }
    }
}
