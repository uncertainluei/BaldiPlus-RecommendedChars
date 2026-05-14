using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class PaintingParticle : MonoBehaviour
    {
        private Sprite sprite;
        private EnvironmentController ec;
        private float lifeSpan = 2f, gravity = -16f, _delta;
        private Vector3 currentVelocity;

        public void Initialize(Vector3 position, Sprite baseSpr, EnvironmentController ec)
        {
            transform.position = position;
            this.ec = ec;
            currentVelocity = new(Random.Range(-4f,4f),Random.Range(3f,9f),Random.Range(-4f,4f));
            sprite = Sprite.Create(
                baseSpr.texture,
                new(baseSpr.rect.position+new Vector2(Random.Range(0,70), Random.Range(0,97)), Vector3.one*32f),
                Vector2.one*0.5f, baseSpr.pixelsPerUnit, 0,
                SpriteMeshType.FullRect 
            );
            GetComponent<SpriteRenderer>().sprite = sprite;
        }

        private void Update()
        {
            _delta =  Time.deltaTime * ec.EnvironmentTimeScale;
            currentVelocity.y += gravity * _delta;
            transform.position += currentVelocity * _delta;
            lifeSpan -= _delta;
            if (lifeSpan <= 0f)
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (sprite)
                Destroy(sprite);
        }
    }
}