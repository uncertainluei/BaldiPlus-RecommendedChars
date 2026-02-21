using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch(typeof(CharacterRadar.Hooks.NpcHooks), "AwakePostfix")]
    static class CharacterRadarColorPatch
    {
        internal static readonly Dictionary<Character, Color> colors = [];

        private static bool Prefix(NPC __0)
        {
            if (!__0.Navigator || !__0.Navigator.Entity) return false;
            if (!colors.ContainsKey(__0.character)) return true;

            BaseGameManager.Instance.Ec.map.AddArrow(__0.Navigator.Entity, colors[__0.character]);
            return false;
        }
    }
}
