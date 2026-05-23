using HarmonyLib;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    static class MaintenanceMachinePatches
    {
        [HarmonyPatch(typeof(GravityEvent), "FlipPlayer"), HarmonyPrefix]
        private static void SyncPlayerFlipState(GravityEvent __instance)
            => __instance.playerFlipped = CoreGameManager.Instance.GetPlayer(0).plm.Entity.Flipped;

        [HarmonyPatch(typeof(GravityEvent), "FlipNPC"), HarmonyPrefix]
        private static void SyncNpcFlipState(GravityEvent __instance, NPC npc)
        {
            if (!__instance.Active || !npc.Entity) return;

            for (int i = 0, c = __instance.npcs.Count; i < c; i++)
            {
                if (__instance.npcs[i] == npc)
                {
                    __instance.npcFlipped[i] = npc.Entity.Flipped;
                    return;
                }
            }
        }
    }
}