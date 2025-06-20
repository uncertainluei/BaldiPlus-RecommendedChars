using System;
using System.Collections;
using System.Collections.Generic;

using MTM101BaldAPI.Registers;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public static class ManMemeCoinEvents
    {
        public static readonly List<WeightedSelection<AbstractManMemeAction>> Events = new List<WeightedSelection<AbstractManMemeAction>>();

        public static readonly ManMemeItemAction ItemsYtps = new ManMemeItemAction(3, 5, true);
        public static readonly ManMemeItemAction ItemsRare = new ManMemeItemAction(1, 2);
        public static readonly ManMemeItemAction ItemsUnique = new ManMemeUniqueItemAction();

        private static readonly ManMemeItemAction ItemsMap = new ManMemeItemAction();

        public static void AddToEvents(this AbstractManMemeAction action, int weight)
        {
            Events.Add(action.Weighted<AbstractManMemeAction, WeightedSelection<AbstractManMemeAction>>(weight));
        }

        public static int GetPlayerInventoryCost(int player)
        {
            ItemManager itmMan = CoreGameManager.Instance.GetPlayer(player).itm;
            int val = 0;
            for (int i = 0; i < itmMan.maxItem; i++)
                val += itmMan.items[i].price;

            return val;
        }

        private static void AddModdedItem(this ManMemeItemAction itemAction, string key, int weight = 100)
        {
            ItemObject itm = RecommendedCharsPlugin.AssetMan.Get<ItemObject>(key);
            if (itm != default)
                itemAction.AddItem(itm, weight);
        }

        internal static void InitializeBaseEvents()
        {
            ItemsYtps.SetInclusionCriteria((i) => CoreGameManager.Instance.GetPoints(i) < 250); // || GetPlayerInventoryCost(i) > 2500
            ItemsYtps.AddItem(ItemMetaStorage.Instance.GetPointsObject(25, true), 45);
            ItemsYtps.AddItem(ItemMetaStorage.Instance.GetPointsObject(50, true), 25);
            ItemsYtps.AddItem(ItemMetaStorage.Instance.GetPointsObject(100, true), 8);
            AddToEvents(ItemsYtps, 75);

            //ItemsRare.SetInclusionCriteria((i) => GetPlayerInventoryCost(i) < 2500);
            ItemsRare.AddItem(Items.Quarter, 29);
            ItemsRare.AddItem(Items.Nametag, 28);
            ItemsRare.AddModdedItem("NerfGunItem", 28);
            ItemsRare.AddItem(Items.DoorLock, 26);
            ItemsRare.AddModdedItem("PieItem", 26);
            ItemsRare.AddItem(Items.Bsoda, 26);
            ItemsRare.AddItem(Items.PortalPoster, 25);
            ItemsRare.AddItem(Items.GrapplingHook, 22);
            ItemsRare.AddItem(ItemMetaStorage.Instance.GetPointsObject(100, true), 20);
            ItemsRare.AddItem(Items.Apple, 18);
            AddToEvents(ItemsRare, 80);

            //ItemsUnique.SetInclusionCriteria((i) => GetPlayerInventoryCost(i) < 1500);
            ItemsUnique.AddModdedItem("CherryBsodaItem", 20);
            ItemsUnique.AddModdedItem("ManglesItem", 22);
            ItemsUnique.AddModdedItem("FlaminHotCheetosItem", 18);
            ItemsUnique.AddModdedItem("UltimateAppleItem", 6);
            AddToEvents(ItemsUnique, 80);

            ItemsMap.SetInclusionCriteria((i) => !CoreGameManager.Instance.mapChallenge && !CoreGameManager.Instance.saveMapPurchased);
            ItemsMap.AddItem(Items.Map, 100);
            AddToEvents(ItemsMap, 60);
        }
    }

    public abstract class AbstractManMemeAction
    {
        private Func<int, bool> inclusionCriteria;

        public abstract void Invoke(ManMemeCoin coin, int player);
        public bool ShouldInclude(int player)
        {
            if (inclusionCriteria != null)
                return inclusionCriteria(player);
            return true;
        }

        public void SetInclusionCriteria(Func<int, bool> func)
        {
            if (inclusionCriteria == null)
                inclusionCriteria = func;
        }
    }

    public class ManMemeItemAction : AbstractManMemeAction
    {
        public ManMemeItemAction(byte min = 1, byte max = 1, bool repeat = false)
        {
            minItems = min;
            maxItems = max;
            maxItems++;
            repeatItems = repeat;
        }

        public void AddItem(Items itm, int weight) => AddItem(ItemMetaStorage.Instance.FindByEnum(itm).value, weight);
        public void AddItem(ItemObject itm, int weight) => potentialItems.Add(itm.Weighted(weight));

        private readonly List<WeightedItemObject> potentialItems = new List<WeightedItemObject>();
        private readonly byte minItems;
        private readonly byte maxItems;
        private readonly bool repeatItems;

        public override void Invoke(ManMemeCoin coin, int player)
        {
            List<WeightedSelection<ItemObject>> items = WeightedItemObject.Convert(potentialItems);
            List<Vector3> pickups = new List<Vector3>();
            Vector3 target;

            int amount = UnityEngine.Random.Range(minItems, maxItems), idx;

            for (int i = 0; i < amount && items.Count > 0; i++)
            {
                idx = WeightedSelection<ItemObject>.RandomIndexList(items);
                do
                {
                    target = coin.navigator.currentTile.CenterWorldPosition + new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f));
                }
                while (InDistanceOf(target, ref pickups, 1.5f));
                pickups.Add(target);

                CreatePickup(coin.currentRoom, items[idx].selection, new Vector2(target.x, target.z));
                if (!repeatItems)
                    items.RemoveAt(i);
            }
        }

        protected virtual Pickup CreatePickup(RoomController room, ItemObject itm, Vector2 pos)
        {
            Pickup pickup = room.ec.CreateItem(room, itm, pos);
            pickup.icon = room.ec.map.AddIcon(pickup.iconPre, pickup.transform, Color.white);
            return pickup;
        }

        private bool InDistanceOf(Vector3 pos, ref List<Vector3> pickups, float dist)
        {
            foreach (Vector3 pickup in pickups)
                if (Vector3.Distance(pos, pickup) < dist)
                    return true;

            return false;
        }
    }

    public class ManMemeUniqueItemAction : ManMemeItemAction
    {
        public ManMemeUniqueItemAction(byte min = 1, byte max = 1, bool repeat = false) : base(min, max, repeat)
        {
        }

        protected override Pickup CreatePickup(RoomController room, ItemObject itm, Vector2 pos)
        {
            Pickup pickup = base.CreatePickup(room, itm, pos);
            pickup.icon.spriteRenderer.color = Color.yellow;
            pickup.showDescription = true;
            return pickup;
        }
    }

    public class ManMemeCoroutineAction : AbstractManMemeAction
    {
        public delegate IEnumerator CoroutineAction(ManMemeCoin coin, int player);
        private readonly CoroutineAction runAsCoroutine;

        public ManMemeCoroutineAction(CoroutineAction enumerator, Func<int, bool> inclusionCriteria = null)
        {
            runAsCoroutine = enumerator;
            SetInclusionCriteria(inclusionCriteria);
        }

        public override void Invoke(ManMemeCoin coin, int player)
        {
            MonoBehaviour dummyBehavior = new GameObject("ManMemeCoin Event", typeof(MonoBehaviour)).GetComponent<MonoBehaviour>();
            dummyBehavior.StartCoroutine(RunInstruction(dummyBehavior, coin, player));
        }

        private IEnumerator RunInstruction(MonoBehaviour obj, ManMemeCoin coin, int player)
        {
            yield return obj.StartCoroutine(runAsCoroutine(coin, player));
            GameObject.Destroy(obj.gameObject);
        }
    }

    public class ManMemeProcedureAction : AbstractManMemeAction
    {
        private Action<ManMemeCoin, int> runAction;

        public ManMemeProcedureAction(Action<ManMemeCoin, int> action, Func<int, bool> inclusionCriteria = null)
        {
            runAction = action;
            SetInclusionCriteria(inclusionCriteria);
        }

        public override void Invoke(ManMemeCoin coin, int player)
        {
            runAction(coin, player);
        }
    }
}
