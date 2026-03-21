using HarmonyLib;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    static class CarterPatches
    {
        [HarmonyPatch(typeof(HudManager), "Awake"), HarmonyPostfix]
        private static void AddCarterHud(HudManager __instance)
        {
            CarterHudManager hud = new GameObject("CarterHud", typeof(RectTransform), typeof(CarterHudManager)).GetComponent<CarterHudManager>();
            hud.transform.parent = __instance.transform;

            // Position behind the YTPs display
            Transform pointsDisplay = __instance.transform.Find("PointsDisplay");
            if (pointsDisplay)
                hud.transform.SetSiblingIndex(pointsDisplay.GetSiblingIndex());

            hud.transform.localPosition = Vector3.zero;
        }
    }
}