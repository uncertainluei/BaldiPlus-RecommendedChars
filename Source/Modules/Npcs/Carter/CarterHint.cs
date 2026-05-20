using MTM101BaldAPI;

using System.Collections;
using System.Collections.Generic;

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

        public bool ShowMode { get; private set;}

        private void Awake()
        {
            prefab = RecommendedCharsPlugin.ObjMan.Get<CarterPaper>("Obj_CarterPaper");
            audTurn = RecommendedCharsPlugin.AssetMan.Get<SoundObject>("Sfx/MapTurn");
            ShowMode = false;
        }

        private void FixedUpdate()
        {
            if (papers.Count == 0 || !InputManager.Instance.GetDigitalInput("Map", false))
            {
                ShowMode = false;
                return;
            }
            if (!ShowMode)
                CoreGameManager.Instance.audMan.PlaySingle(audTurn);

            ShowMode = true;
        }

        public CarterPaper ActivatePaper(RoomController location)
        {
            CarterPaper paper = Instantiate(prefab, transform);
            papers.Add(paper);
            paper.manager = this;
            SortPapers();

            string key = GetLocationKey(location);
            paper.text.text = ("Hud_RecChars_CarterHint_"+key).Localize(string.Format("Hud_RecChars_CarterHint_Fallback".Localize(), key));
            return paper;
        }

        private string GetLocationKey(RoomController location)
        {
            if (location.category == RoomCategory.Special)
            {
                // Fallback if Custom Posters isn't installed
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
                if (location.activity.name == "Activity_BalloonBuster(Clone)")
                    return "Class_BalloonBuster";
                if (location.activity.name == "Activity_Match(Clone)")
                    return "Class_Match";
            }

            string key = location.category.ToStringExtended();
            if (key.StartsWith("RecChars_"))
                return key.Substring(9);

            return key;
        }

        public void RemovePaper(CarterPaper paper)
        {
            papers.Remove(paper);
            SortPapers();
        }

        private void SortPapers()
        {
            int c = papers.Count, d = Mathf.Max(130-c*2, 98), s = (c-1)*d/-2;
            for (int i = 0; i < c; i++)
            {
                papers[i].SetX(s);
                s+=d;
            }
        }
    }

    public class CarterPaper : MonoBehaviour
    {
        public CarterHudManager manager;
        public SoundObject audZoom, audWhoosh, audThump;
        public TMP_Text text;

        private new RectTransform transform;
        private Vector2 pos, newPos;
        private float targetX, targetY;
        private bool updatePos = false;
        private bool active = false;

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
            updatePos = false;
            pos.x = newPos.x = targetX;
            transform.anchoredPosition = new(newPos.x, 0);

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
            yield return SmoothGlide(new(newPos.x,25),15);

            CoreGameManager.Instance.audMan.PlaySingle(audThump);
            yield return SmoothGlide(new(newPos.x,-228),20);
            yield return SmoothGlide(new(newPos.x,-220),20);

            yield return new WaitForSeconds(0.5f);

            newPos.y = pos.y = -220;
            transform.anchoredPosition = pos;
            updatePos = true;
            active = true;
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

        public void SetX(int value)
        {
            targetX = value;
            if (active)
            {
                newPos.x = pos.x;
                updatePos = true;
            }
        }

        private void Update()
        {
            if (!active)
                return;

            targetY = manager.ShowMode ? -150 : -220;
            newPos.y = Mathf.Lerp(newPos.y, targetY, 20*Time.deltaTime);
            pos.y = Mathf.Round(newPos.y);
            transform.anchoredPosition = pos;

            if (!updatePos)
                return;

            newPos.x = Mathf.Lerp(newPos.x, targetX, 10*Time.deltaTime);
            pos.x = Mathf.Round(newPos.x);
            transform.anchoredPosition = pos;

            if (pos.x == targetX)
                updatePos = false;
        }

        public void Deactivate()
        {
            manager.RemovePaper(this);
            active = false;
            StopAllCoroutines();

            StartCoroutine(LeaveRoutine());
        }

        private IEnumerator LeaveRoutine()
        {
            updatePos = false;

            newPos.x = transform.anchoredPosition.x;
            pos.y = transform.anchoredPosition.y;
            yield return SmoothGlide(new(newPos.x,pos.y+16),15);
            yield return SmoothGlide(new(newPos.x,-256),20);
            Destroy(gameObject);
        }
    }
}