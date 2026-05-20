using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PrimitiveDrop_Sphere : PrimitiveDrop
    {
        public QuickExplosion popPre;
        public Vector2 speedRange = new(15f, 25f);

        private Vector3 direction;
        private float speed, maxHeight = 7.5f, yVel = 0f;
        private readonly float knockBackSpeed = 40f, knockBackAccel = -36f;

        protected override void Initialize()
        {
            transform.eulerAngles = new(0f, Random.Range(-2,2)*90f+Random.Range(15f,75f), 0f);
            direction = transform.forward;
            speed = Random.Range(speedRange[0], speedRange[1]);
            entity.OnEntityMoveInitialCollision += OnEntityMoveCollision;
        }

        protected override void VirtualUpdate()
        {
            yVel += gravity * Time.deltaTime * entity.Ec.EnvironmentTimeScale;
            height += yVel * Time.deltaTime * entity.Ec.EnvironmentTimeScale;

            if (height < 0f)
            {
                height = 0f;
                yVel = 20f;
            }
            else if (height > maxHeight)
                height = maxHeight;
            
            entity.SetHeight(height);
            entity.UpdateInternalMovement(direction * speed);
        }

        private void OnEntityMoveCollision(RaycastHit hit)
        {
            direction = Vector3.Reflect(direction, hit.normal);
            transform.forward = direction;
        }

        protected override void ShapeTriggerEnter(Entity ent, bool validCollision)
        {
            if (ent.CompareTag("Player") || ent.CompareTag("NPC"))
            {
                Instantiate(popPre, transform.parent).transform.position = entity.rendererBase.position;
                if (ent.CompareTag("Player"))
                    entity.Ec.MakeNoise(transform.position, 10);
                if (!entity.Squished)
                    ent.AddForce(new((ent.transform.position-transform.position).normalized, knockBackSpeed, knockBackAccel));
                SetDead();
            }
        }
    }
}