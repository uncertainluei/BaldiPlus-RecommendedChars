using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class EveyBsodaaSpray : ITM_BSODA, IEntityTrigger
    {
        public EveyBsodaa bsodaa;

        private bool finished = false;
        private float unscaledSpeed;

        public void Start()
        {
            ec = bsodaa.ec;
            entity.Initialize(ec, transform.position);

            unscaledSpeed = speed;

            bsodaa.audMan.PlaySingle(sound);
            moveMod.priority = 1;
        }

        private new void Update()
        {
            speed = unscaledSpeed * bsodaa.TimeScale / ec.EnvironmentTimeScale;
            time += (ec.EnvironmentTimeScale - bsodaa.TimeScale) * Time.deltaTime;
            base.Update();

            if (!finished && time <= 0f)
                BsodaFinished(false);
        }

        public new void EntityTriggerEnter(Collider other, bool canCollide)
        {
            if (!canCollide || launching) return;
            if (!other.isTrigger && other.gameObject.layer == 1)
            {
                if (!finished)
                    BsodaFinished(false);
                Destroy(gameObject);
                return;
            }

            if (other.TryGetComponent(out Entity entity))
            {
                if (!finished)
                    BsodaFinished(true);

                entity.ExternalActivity.moveMods.Add(moveMod);
                activityMods.Add(entity.ExternalActivity);
            }
        }

        public new void EntityTriggerExit(Collider other, bool canCollide)
        {
            if (other.transform == bsodaa.transform)
                launching = false;

            if (canCollide && other.TryGetComponent(out Entity entity)) 
            {
                entity.ExternalActivity.moveMods.Remove(moveMod);
                activityMods.Remove(entity.ExternalActivity);
            }
        }

        private void BsodaFinished(bool success)
        {
            finished = true;
            bsodaa.BsodaFinished(success);
        }
    }
}
