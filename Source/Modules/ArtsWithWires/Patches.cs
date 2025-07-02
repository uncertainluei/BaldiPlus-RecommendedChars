using HarmonyLib;
using MTM101BaldAPI;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "ArtsWWires")]
    static class ArtsWithWiresPatches
    {
        [HarmonyPatch(typeof(ITM_Scissors), "Use")]
        [HarmonyPostfix]
        private static void OnScissorsUse(PlayerManager pm, ref bool __result, SoundObject ___audSnip)
        {
            bool oldResult = __result;
            if (pm.TryGetComponent(out GrabbingGameContainer grabbed) && grabbed.TryCutGrabbingGames())
                __result = true;

            if (oldResult != __result)
                CoreGameManager.Instance.audMan.PlaySingle(___audSnip);
        }

        [HarmonyPatch(typeof(ITM_Boots), "Use")]
        [HarmonyPostfix]
        private static void Postfix(PlayerManager pm)
        {
            /* I was initially going to do some weird workarounds to remove the MoveMod
             * if you had the boots on, but honestly I might as well just yoink the
             * end game functionality that I used for the scissors
             */
            if (pm.TryGetComponent(out GrabbingGameContainer grabbed))
                grabbed.TryCutGrabbingGames();
        }
    }
}
