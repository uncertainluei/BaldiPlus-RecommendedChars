using HarmonyLib;
using MTM101BaldAPI;

using BaldiLevelEditor;
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
        public static event Action<PlusLevelEditor> OnEditorInit;

        [HarmonyPatch(typeof(EditorLevel), "InitializeDefaultTextures"), HarmonyPostfix]
        private static void InitializeRoomTextures(EditorLevel __instance)
        {
            OnRoomInit?.Invoke(__instance.defaultTextures);
        }

        [HarmonyPatch(typeof(PlusLevelEditor), "Initialize"), HarmonyPostfix]
        private static void AddObjects(PlusLevelEditor __instance)
        {
            OnEditorInit?.Invoke(__instance);
        }
    }
}
