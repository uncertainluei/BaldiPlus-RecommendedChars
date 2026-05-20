using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_Dodecahedron : PrimitiveDrop
    {
        private bool ready;
        private Force force;
        private Collider blocker;

        public Vector2 forceRange = new(24f, 45f);
        public CoverCloud coverCloud;

        protected override void Initialize()
        {
            ready = false;
            coverCloud = Instantiate(coverCloud, entity.Ec.transform);
            coverCloud.Ec = entity.Ec;
            blocker = coverCloud.transform.GetChild(0).GetComponent<BoxCollider>();
            blocker.enabled = false;
        }

        public void InitPosAndForce(Vector3 position, Vector3 direction)
        {
            transform.position = position;
            transform.forward = direction;
            float forceStr = Random.Range(forceRange[0], forceRange[1]);
            force = new(direction, forceStr, forceStr*-0.8f);
            entity.AddForce(force);
            ready = true;
        }

        protected override void VirtualUpdate()
        {
            if (ready && force.Dead)
            {
                ready = false;
                Vector3 pos = transform.position;
                pos.y = entity.Ec.transform.position.y + 5f;
                coverCloud.transform.position = pos;
                blocker.enabled = true;
                coverCloud.StartDelay(1f);
            }
        }

        private void OnDestroy()
        {
            if (!coverCloud.Ec) return;
            if (coverCloud.isActiveAndEnabled && coverCloud.trigger.enabled)
            {
                coverCloud.StartEndTimer(5f);
                return;
            }
            Destroy(coverCloud.gameObject);
        }
    }

    public class PrimitiveDrop_DodecahedronLarge : PrimitiveDrop
    {
        public PrimitiveDrop_Dodecahedron smallPre;

        protected override void VirtualUpdate()
        {
            float dir = Random.Range(-180f, 180f);
            for (int i = 0; i < 12; i++, dir += 30f)
            {
                PrimitiveDrop_Dodecahedron small = Instantiate(smallPre, transform.parent);
                small.Spawn(machine);
                transform.eulerAngles = new(0f, dir, 0f);
                small.InitPosAndForce(transform.position, transform.forward);
                machine.AddEntity(small.entity);
            }
            SetDead();
        }
    }
}