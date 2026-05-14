using System.Collections;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{   
    public abstract class PrimitiveDrop : MonoBehaviour
    {
        public AudioManager audMan;
        public SpriteRenderer sprite;
        public Entity entity;
        private MaintenanceMachine machine;

        protected float height = 5f, endHeight = 0f, gravity = -5f;
        private bool ready = false;

        public void Spawn(MaintenanceMachine machine)
        {
            this.machine = machine;
            entity.Initialize(machine.ec, machine.transform.position);
            entity.SetGrounded(false);
            Initialize();
            Fall(endHeight, gravity, endHeight == 0f);
        }

        private void Update()
        {
            if (!ready)
            {
                if (_fallRoutine != null) return;
                ready = true;
            }
            VirtualUpdate();
        }

        private float _signEnd;
        private Coroutine _fallRoutine;
        protected void Fall(float end, float gravity, bool setGrounded = true)
        {
            if (_fallRoutine != null)
                StopCoroutine(_fallRoutine);

            _fallRoutine = StartCoroutine(FallRoutine(end,gravity,setGrounded));
        }

        private IEnumerator FallRoutine(float end, float gravity, bool setGrounded)
        {
            _signEnd = Mathf.Sign(height-end)*end;
            entity.SetGrounded(false);
            while (height < _signEnd)
            {
                height += gravity * Time.deltaTime * entity.Ec.EnvironmentTimeScale;
                entity.UpdateInternalMovement(Vector3.zero);
                entity.SetHeight(height);
                yield return null;
            }
            height = end;
            entity.SetHeight(height);
            entity.SetGrounded(setGrounded);
            _fallRoutine = null;
        }

        protected virtual void Initialize() {}

        protected virtual void VirtualUpdate() {}

        private void OnDisable()
        {
            if (machine)
            {
                machine.RemoveEntity(entity);
                Destroy(gameObject);
            }
        }
    }
}