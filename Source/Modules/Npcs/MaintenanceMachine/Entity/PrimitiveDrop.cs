using System.Collections;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{   
    public abstract class PrimitiveDrop : MonoBehaviour, IEntityTrigger
    {
        public AudioManager audMan;
        public SpriteRenderer sprite;
        public Entity entity;
        private MaintenanceMachine machine;

        protected float height = 5f, endHeight = 0f, gravity = -25f;
        protected bool Ready {get; private set;} = false;
        private bool dead = false;

        public void Spawn(MaintenanceMachine machine)
        {
            this.machine = machine;
            entity.Initialize(machine.ec, machine.transform.position);
            entity.CopyStatusEffects(machine.Entity);
            entity.SetGrounded(false);
            Initialize();
            Fall(endHeight, gravity, endHeight == 0f);
        }

        protected void SetDead() => dead = true;

        private void Update()
        {
            if (!Ready)
            {
                if (Falling) return;
                Ready = true;
            }
            VirtualUpdate();
            if (dead && !audMan.AnyAudioIsPlaying)
                Destroy(gameObject);
        }

        private float _sign, _ySpeed;
        private Coroutine _fallRoutine;
        public bool Falling => _fallRoutine != null;
        protected void Fall(float end, float gravity, bool setGrounded = true)
        {
            if (_fallRoutine != null)
                StopCoroutine(_fallRoutine);

            _fallRoutine = StartCoroutine(FallRoutine(end,gravity,setGrounded));
        }
        

        private IEnumerator FallRoutine(float end, float gravity, bool setGrounded)
        {
            _sign = Mathf.Sign(height-end);
            entity.SetGrounded(false);
            _ySpeed = 0f;
            while (_sign*height > _sign*end)
            {
                _ySpeed += gravity * Time.deltaTime * entity.Ec.EnvironmentTimeScale;
                height += _ySpeed * Time.deltaTime * entity.Ec.EnvironmentTimeScale;
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

        protected virtual void ShapeTriggerEnter(Entity ent, bool validCollision) {}
        protected virtual void ShapeTriggerStay(Entity ent, bool validCollision) {}
        protected virtual void ShapeTriggerExit(Entity ent, bool validCollision) {}


        private void OnDisable()
        {
            if (machine)
            {
                machine.RemoveEntity(entity);
                Destroy(gameObject);
            }
        }

        public void EntityTriggerEnter(Entity otherEntity, Collider other, bool validCollision)
        {
            if (Ready && !dead && otherEntity != machine.Entity)
                ShapeTriggerEnter(otherEntity, validCollision);
        }

        public void EntityTriggerStay(Entity otherEntity, Collider other, bool validCollision)
        {
            if (Ready && !dead && otherEntity != machine.Entity)
                ShapeTriggerStay(otherEntity, validCollision);
        }

        public void EntityTriggerExit(Entity otherEntity, Collider other, bool validCollision)
        {
            if (Ready && !dead && otherEntity != machine.Entity)
                ShapeTriggerExit(otherEntity, validCollision);
        }
    }
}