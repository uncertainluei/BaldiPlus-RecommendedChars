using BaldiLevelEditor;
using HarmonyLib;
using MTM101BaldAPI;
using PlusLevelFormat;

using System;
using System.Collections.Generic;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [ConditionalPatchMod(RecommendedCharsPlugin.LegacyEditorGuid)]
    [HarmonyPatch]
    static class LegacyEditorPatches
    {
        public static event Action<Dictionary<string,TextureContainer>> OnRoomInit;

        [HarmonyPatch(typeof(EditorLevel), "InitializeDefaultTextures"), HarmonyPostfix]
        private static void InitializeRoomTextures(EditorLevel __instance)
        {
            OnRoomInit?.Invoke(__instance.defaultTextures);
        }
    }
}
