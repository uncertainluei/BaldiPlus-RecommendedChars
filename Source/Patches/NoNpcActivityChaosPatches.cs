using HarmonyLib;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    static class NoNpcActivityChaosPatches
    {
        [HarmonyPatch(typeof(Playtime_Wandering), "OnStateTriggerEnter"), HarmonyPrefix]
        private static void OnStateTriggerEnter(Collider other, ref bool validCollision)
        {
            if (!RecommendedCharsConfig.onlyOneNpcActivity.Value) return;
            if (other.CompareTag("Player") && other.GetComponent<PlayerManager>().jumpropes.Count > 0)
                validCollision = false;
        }
    }
}