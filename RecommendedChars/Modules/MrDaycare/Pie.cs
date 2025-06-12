using System.Collections;
using MTM101BaldAPI;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_Pie : Item, IEntityTrigger
    {
        public GameObject flyingSprite;
        public GameObject groundedSprite;
        public Material noBillboardMat;
        public Entity entity;

        public AudioManager audMan;
        public SoundObject audThrow;
        public SoundObject audSplat;

        private float speed = 30f;
        private float setTime = 15f;
        private bool flying = true;

        private EnvironmentController ec;
        private readonly MovementModifier moveMod = new MovementModifier(Vector3.zero, 0.3f);

        public override bool Use(PlayerManager pm)
        {
            ec = pm.ec;

            transform.position = pm.transform.position;
            transform.forward = CoreGameManager.Instance.GetCamera(pm.playerNumber).transform.forward;

            entity.Initialize(ec, transform.position);
            entity.OnEntityMoveInitialCollision += OnWallCollision;

            CoreGameManager.Instance.audMan.PlaySingle(audThrow);
            DaycareGuiltManager.GetInstance(pm).BreakRule("Throwing", 0.8f, 0.125f);
            moveMod.priority = 1;
            return true;
        }

        private void Update()
        {
            if (flying)
                entity.UpdateInternalMovement(transform.forward * speed * ec.EnvironmentTimeScale);
        }

        private ActivityModifier actMod;
        private Entity looker;
        public void EntityTriggerEnter(Collider other)
        {
            if (!flying || !other.isTrigger || !other.CompareTag("NPC")) return;

            actMod = other.GetComponent<ActivityModifier>();
            looker = other.GetComponent<Entity>();
            flying = false;
            flyingSprite.SetActive(false);
            groundedSprite.SetActive(true);
            looker.SetBlinded(true);
            actMod.moveMods.Add(moveMod);
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
                actMod = null;
                transform.rotation = Quaternion.LookRotation(hit.normal * -1f, Vector3.up);
                transform.position = hit.point;
                flyingSprite.SetActive(false);
                groundedSprite.SetActive(true);
                groundedSprite.GetComponent<SpriteRenderer>().sharedMaterial = noBillboardMat;
                groundedSprite.transform.localPosition = Vector3.forward * 0.8f;
                StartCoroutine(WallHitTimer(setTime));
                audMan.FlushQueue(true);
                audMan.PlaySingle(audSplat);
            }
        }

        private IEnumerator NpcHitTimer(float time)
        {
            while (time > 0f)
            {
                if (actMod == null)
                {
                    Destroy(gameObject);
                    yield break;
                }
                time -= Time.deltaTime * ec.EnvironmentTimeScale;
                entity.UpdateInternalMovement(Vector3.zero);
                transform.position = actMod.transform.position;
                yield return null;
            }
            actMod.moveMods.Remove(moveMod);
            looker.SetBlinded(false);
            Destroy(gameObject);
        }

        private IEnumerator WallHitTimer(float time)
        {
            yield return new WaitForSecondsEnvironmentTimescale(ec, time);
            Destroy(gameObject);
        }

        public void EntityTriggerExit(Collider other)
        {
        }
        public void EntityTriggerStay(Collider other)
        {
        }
    }
}
