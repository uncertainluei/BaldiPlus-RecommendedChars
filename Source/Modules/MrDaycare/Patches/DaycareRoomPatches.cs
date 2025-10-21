using HarmonyLib;
using MTM101BaldAPI;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    class DaycareRoomPatches
    {
        [HarmonyPatch(typeof(StandardDoor), "ItemFits"), HarmonyPostfix]
        private static void NotebookGateItemFits(StandardDoor __instance, ref bool __result)
        {
            if (__instance is DaycareNotebookDoor daycareDoor && daycareDoor.IsNotebookGate)
                __result = false;
        }

        [HarmonyPatch(typeof(StandardDoor), "InsertItem")]
        [HarmonyPatch(typeof(StandardDoor), "OpenTimedWithKey")]
        [HarmonyPrefix]
        private static bool NotebookGateTryOpen(StandardDoor __instance) => __instance is not DaycareNotebookDoor daycareDoor || !daycareDoor.IsNotebookGate;
    }
}
