using System.Collections;
using System.Collections.Generic;
using MTM101BaldAPI;
using TMPro;
using UncertainLuei.CaudexLib.Util.Extensions;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class CarterHudManager : MonoBehaviour
    {
        private static HudManager _hud;
        private static CarterHudManager _instance;
        public static CarterHudManager GetInstance(HudManager hud)
        {
            if (_hud == hud && _instance)
                return _instance;

            _hud = hud;
            _instance = hud.GetComponentInChildren<CarterHudManager>();
            return _instance;
        }

        public CarterPaper prefab;
        private readonly List<CarterPaper> papers = [];
        private SoundObject audTurn;

        private void Awake()
        {
            prefab = RecommendedCharsPlugin.ObjMan.Get<CarterPaper>("Obj_CarterPaper");
            audTurn = RecommendedCharsPlugin.AssetMan.Get<SoundObject>("Sfx/MapTurn");
        }

        public CarterPaper ActivatePaper(RoomController location)
        {
            CarterPaper paper = Instantiate(prefab, transform);
            papers.Add(paper);
            paper.manager = this;

            string key = GetLocationKey(location);
            paper.text.text = ("Hud_RecChars_CarterHint_"+key).Localize(string.Format("Hud_RecChars_CarterHint_Fallback".Localize(), key));
            return paper;
        }

        private string GetLocationKey(RoomController location)
        {
            if (location.category == RoomCategory.Special)
            {
                // Grab all of them
                if (location.doorMats.name == "SuppliesDoorSet")
                    return "Closet";
                if (location.doorMats.name == "DoctorDoorSet")
                    return "Clinic";
                if (location.functionObject.name.StartsWith("Cafeteria"))
                    return "Cafeteria";
                if (location.functionObject.name.StartsWith("Playground"))
                    return "Playground";
                if (location.functionObject.name.StartsWith("Library"))
                    return "Library";
                if (location.functionObject.name.Contains("Teleporter"))
                    return "Laboratory";
                if (location.functionObject.name.StartsWith("LightbulbTest"))
                    return "LightbulbTesting";
            }

            if (location.category == RoomCategory.Class)
            {
                if (location.activity.name == "Activity_BalloonBuster")
                    return "Class_BalloonBuster";
                if (location.activity.name == "Activity_Match")
                    return "Class_Match";
            }

            string key = location.category.ToStringExtended();
            if (key.StartsWith("RecChars_"))
                return key.Substring(9);

            return key;
        }
    }

    public class CarterPaper : MonoBehaviour
    {
        public CarterHudManager manager;
        public SoundObject audZoom, audWhoosh, audThump;
        public TMP_Text text;

        private new RectTransform transform;
        private Vector2 pos;
        private float targetX, newX;
        private bool updatePos = false;

        private void Awake()
        {
            transform = (RectTransform)base.transform;
            transform.pivot = Vector2.one/2;
            transform.anchorMin = transform.pivot;
            transform.anchorMax = transform.pivot;
            transform.localScale = Vector2.zero;
        }

        private void Start()
        {
            StartCoroutine(PopUpRoutine());
        }

        private IEnumerator PopUpRoutine()
        {
            Transform oldParent = transform.parent;
            transform.SetParent(transform.parent.parent, false);
            transform.anchoredPosition = Vector3.zero;

            CoreGameManager.Instance.audMan.PlaySingle(audZoom);
            
            Vector2 sizeVector = new(0f,0f);
            for (float size = 0, total = 128; size < 127f; size = Mathf.Lerp(size, total, 15f * Time.deltaTime))
            {
                sizeVector.x = sizeVector.y = Mathf.Round(size)/total;
                transform.localScale = sizeVector;
                yield return null;
            }
            sizeVector.x = sizeVector.y = 1;
            transform.localScale = sizeVector;

            yield return new WaitForSeconds(1f);

            CoreGameManager.Instance.audMan.PlaySingle(audWhoosh);
            yield return SmoothGlide(new(0,25),15);

            CoreGameManager.Instance.audMan.PlaySingle(audThump);
            yield return SmoothGlide(new(0,-228),20);
            yield return SmoothGlide(new(0,-220),20);

            yield return new WaitForSeconds(0.5f);

            transform.SetParent(oldParent, false);
            pos.x = newX = 0;
            pos.y = 0;
            transform.anchoredPosition = Vector3.zero;
            updatePos = true;
        }

        private IEnumerator SmoothGlide(Vector2 dest, float speed)
        {
            pos = transform.anchoredPosition;
            for (; Vector2.Distance(pos, dest) >= 0.5; pos = Vector2.Lerp(pos, dest, speed*Time.deltaTime))
            {
                transform.anchoredPosition = new(Mathf.Round(pos.x), Mathf.Round(pos.y));
                yield return null;
            }
            transform.anchoredPosition = dest;
        }

        public void SetX(float value)
        {
            newX = pos.x;
            targetX = value;
            updatePos = true;
        }

        private void Update()
        {
            if (!updatePos)
                return;

            newX = Mathf.Lerp(newX, targetX, 10*Time.deltaTime);
            pos.x = Mathf.Round(newX);

            if (pos.x == targetX)
                updatePos = false;
        }

        public void Deactivate()
        {
            // I don't know what to add so here's this instead
            Destroy(gameObject);
        }
    }
}