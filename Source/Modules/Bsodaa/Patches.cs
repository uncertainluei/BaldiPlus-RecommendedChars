using HarmonyLib;
using MTM101BaldAPI;
using System.Collections.Generic;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    static class BsodaaSavePatches
    {
        [HarmonyPatch(typeof(BaseGameManager), "RestartLevel"), HarmonyPostfix]
        private static void ResetHelperExhaust()
        {
            ModuleSaveSystem_Bsodaa.Instance.helperExhausted = false;
        }

        [HarmonyPatch(typeof(BaseGameManager), "LoadNextLevel"), HarmonyPostfix]
        private static void SetToDietMode()
        {
            if (ModuleSaveSystem_Bsodaa.Instance.helperExhausted)
                ModuleSaveSystem_Bsodaa.Instance.helperDietMode = true;

            ModuleSaveSystem_Bsodaa.Instance.helperExhausted = false;
        }
    }

    [HarmonyPatch(typeof(BaldisBasicsPlusAdvanced.Patches.UI.Elevator.ElevatorExpelHammerPatch), "GetPotentialCharacters")]
    static class BsodaaHelperExpelBlacklist
    {
        private static void Postfix(ref List<NPC> __result)
        {
            __result.RemoveAll(x => x is BsodaaHelperDummyNpc);
        }
    }
}
