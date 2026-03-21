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

        public new void EntityTriggerEnter(Entity ent, Collider other, bool valid)
        {
            if (!valid || launching) return;
            if (!other.isTrigger && other.gameObject.layer == 1)
            {
                if (!finished)
                    BsodaFinished(false);
                Destroy(gameObject);
                return;
            }

            if (ent)
            {
                if (!finished)
                    BsodaFinished(true);

                ent.ExternalActivity.moveMods.Add(moveMod);
                activityMods.Add(ent.ExternalActivity);
            }
        }

        public new void EntityTriggerExit(Entity ent, Collider other, bool valid)
        {
            if (other.transform == bsodaa.transform)
                launching = false;

            if (valid && ent)
            {
                ent.ExternalActivity.moveMods.Remove(moveMod);
                activityMods.Remove(ent.ExternalActivity);
            }
        }

        private void BsodaFinished(bool success)
        {
            finished = true;
            bsodaa.BsodaFinished(success);
        }
    }
}