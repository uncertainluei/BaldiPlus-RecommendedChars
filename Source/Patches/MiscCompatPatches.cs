using HarmonyLib;
using MTM101BaldAPI;
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

    [HarmonyPatch(typeof(PineDebug.PineDebugManager), "InitAssets")]
    static class PineDebugNpcIconPatch
    {
        internal static readonly Dictionary<Character, Texture2D> icons = [];
        private static bool initialized;

        private static void Postfix()
        {
            if (initialized) return;
            initialized = true;

            foreach (Character character in icons.Keys)
                PineDebug.PineDebugManager.pinedebugAssets.Add($"Border{character.ToStringExtended()}", icons[character]);
        }
    }
}
