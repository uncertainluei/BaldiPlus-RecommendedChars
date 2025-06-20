﻿using HarmonyLib;
using MTM101BaldAPI;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "MrDaycare")]
    [HarmonyPatch]
    class DaycareRoomPatches
    {
        private static StandardDoor _door;
        private static DaycareNotebookGate _daycareDoor;

        private static bool IsNotebookGate(StandardDoor door)
        {
            if (door != _door)
            {
                _door = door;
                _daycareDoor = _door.GetComponent<DaycareNotebookGate>();
            }
            return _daycareDoor != null;
        }

        [HarmonyPatch(typeof(StandardDoor), "ItemFits")]
        [HarmonyPostfix]
        private static void NotebookGateItemFits(StandardDoor __instance, ref bool __result)
        {
            if (IsNotebookGate(__instance))
                __result = false;
        }

        [HarmonyPatch(typeof(StandardDoor), "InsertItem")]
        [HarmonyPatch(typeof(StandardDoor), "OpenTimedWithKey")]
        [HarmonyPrefix]
        private static bool NotebookGateTryOpen(StandardDoor __instance) => !IsNotebookGate(__instance);

        [HarmonyPatch(typeof(BaseGameManager), "CollectNotebooks")]
        [HarmonyPostfix]
        private static void UpdateDaycareRooms()
        {
            DaycareRoomFunction.NotebookCollected();
        }
    }
}
