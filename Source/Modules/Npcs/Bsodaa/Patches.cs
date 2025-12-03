using HarmonyLib;

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
}
