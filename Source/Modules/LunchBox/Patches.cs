using HarmonyLib;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    static class LunchBoxPatches
    {
        private static int[] priceVariations = [-150, -50, 0, 50, 150];

        [HarmonyPatch(typeof(StoreRoomFunction), "Restock"), HarmonyPostfix]
        private static void RandomizeLunchBoxPrice(SceneObject ___storeData, List<Pickup> ___pickups, PriceTag[] ___tag)
        {
            if (___storeData == null)
                return;

            for (int i = 0; i < ___pickups.Count; i++)
            {
                if (___pickups[i] == null ||
                    ___pickups[i].item.itemType != ITM_LunchBox.itemEnum)
                    continue;

                ___pickups[i].price += priceVariations[Random.Range(0,5)];
                ___tag[i].SetText(___pickups[i].price.ToString());
            }
        }

        [HarmonyPatch(typeof(LevelBuilder), "CreateItem", typeof(RoomController), typeof(ItemObject), typeof(Vector2), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        private static void OverrideDummy(ref ItemObject item, System.Random ___controlledRNG)
        {
            if (item.itemType == ITM_LunchBox.randomDummyEnum)
                item = WeightedItemObject.ControlledRandomSelection(ITM_LunchBox.weightedItems, ___controlledRNG);
        }

        [HarmonyPatch(typeof(EnvironmentController), "CreateItem", typeof(RoomController), typeof(ItemObject), typeof(Vector2)), HarmonyPrefix]
        private static void OverrideDummyEc(ref ItemObject item)
        {
            if (item.itemType == ITM_LunchBox.randomDummyEnum)
                item = WeightedItemObject.RandomSelection(ITM_LunchBox.weightedItems);
        }
    }
}
