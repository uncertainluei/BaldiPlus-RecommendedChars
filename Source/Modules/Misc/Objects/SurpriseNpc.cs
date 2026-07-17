using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public abstract class SurpriseNpcVisual
    {
        public float audMaxDistance = -1f;
        public Color audSubtitleColor;
        public SoundObject audSurprise;

        public void Set(SurpriseNpc npc)
        {
            npc.audMan.maxDistance = audMaxDistance;
            npc.audMan.subtitleColor = audSubtitleColor;
            npc.audSurprise = audSurprise;
            SetVisual(npc);
        }
        protected abstract void SetVisual(SurpriseNpc npc);

        public SurpriseNpcVisual(PropagatedAudioManager audMan, SoundObject surpriseAud)
        {
            if (audMan && surpriseAud)
            {
                audMaxDistance = audMan.maxDistance;
                audSubtitleColor = audMan.subtitleColor;
                audSurprise = surpriseAud;
            }
        }

        public SurpriseNpcVisual(NPC npc, SoundObject surpriseAud = null) : this(npc.GetComponent<PropagatedAudioManager>(), surpriseAud)
        {
        }
    }

    public class SurpriseNpcVisualSprite : SurpriseNpcVisual
    {
        public Sprite sprite;
        public float yOffset;

        protected override void SetVisual(SurpriseNpc npc)
        {
            npc.spriteRenderer.sprite = sprite;

            Vector3 position = npc.spriteRenderer.transform.localPosition;
            position.y = yOffset;
            npc.spriteRenderer.transform.localPosition = position;
        }

        public SurpriseNpcVisualSprite(NPC npc, SoundObject surpriseAud = null) : base(npc, surpriseAud)
        {
            sprite = npc.spriteRenderer[0].sprite;
            yOffset = npc.spriteRenderer[0].transform.localPosition.y;
        }

        public SurpriseNpcVisualSprite(NPC npc, Sprite overrideSprite, SoundObject surpriseAud = null) : this(npc, surpriseAud)
            => sprite = overrideSprite;

        public SurpriseNpcVisualSprite(Sprite sprite, float yOffset, PropagatedAudioManager audMan = null, SoundObject surpriseAud = null) : base(audMan, surpriseAud)
        {
            this.sprite = sprite;
            this.yOffset = yOffset;
        }
    }

    public class SurpriseNpcVisualRenderer(NPC npc, SoundObject surpriseAud = null) : SurpriseNpcVisual(npc, surpriseAud)
    {
        public GameObject rendererBase = npc.spriteBase;
        public Sprite sprite;

        protected override void SetVisual(SurpriseNpc npc)
        {
            GameObject.DestroyImmediate(npc.rendererBase);
            npc.rendererBase = GameObject.Instantiate(rendererBase, npc.transform);
            npc.spriteRenderer = npc.rendererBase.GetComponentInChildren<SpriteRenderer>();
            if (npc.spriteRenderer && sprite)
                npc.spriteRenderer.sprite = sprite;
        }

        public SurpriseNpcVisualRenderer(NPC npc, Sprite overrideSprite, SoundObject surpriseAud = null) : this(npc, surpriseAud)
        {
            sprite = overrideSprite;
        }
    }

    public class SurpriseNpcBase : MonoBehaviour
    {
        private RoomController room;

        [SerializeField] internal PropagatedAudioManager audMan;
        [SerializeField] internal SoundObject audSurprise;

        private void Start()
        {
            room = BaseGameManager.Instance.Ec.CellFromPosition(IntVector2.GetGridPosition(transform.position)).room;
        }

        public void Surprise()
        {
            gameObject.SetActive(true);

            if (audSurprise)
            {
                audMan.FlushQueue(true);
                audMan.QueueAudio(audSurprise, true);
            }
        }

        public IEnumerator Float()
        {
            audMan.FlushQueue(true);
            Vector3 spot = room.cells[Random.Range(0, room.cells.Count)].CenterWorldPosition+new Vector3(Random.Range(-2.5f,2.5f),0f,Random.Range(-2.5f, 2.5f));
            spot.y = 5f;
            transform.position = spot;
            while (spot.y < 40f)
            {
                spot.y = Mathf.Min(40f, spot.y + 20f * Time.deltaTime);
                transform.position = spot;
                yield return null;
            }
        }
    }

    public class SurpriseNpc : SurpriseNpcBase
    {
        internal static readonly List<SurpriseNpcVisual> possibleVisuals = [];
        public static void AddVisual(SurpriseNpcVisual visual)
        {
            if (!possibleVisuals.Contains(visual))
                possibleVisuals.Add(visual);
        }

        [SerializeField] internal SpriteRenderer spriteRenderer;
        [SerializeField] internal GameObject rendererBase;
    }
}
