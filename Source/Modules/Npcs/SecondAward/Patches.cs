using HarmonyLib;

using MTM101BaldAPI;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    static class SecondAwardPatches
    {
        [HarmonyPatch(typeof(FirstPrize), "CutWires"), HarmonyPostfix]
        private static void CutWires(FirstPrize __instance)
        {
            if (__instance.Character != SecondAward.charEnum) return;

            SecondAward award = (SecondAward)__instance;
            award.Stunned();

            // Get new stun time for good measure
            if (award.behaviorStateMachine.CurrentState is FirstPrize_Stunned stunState)
                __instance.behaviorStateMachine.ChangeState(new SecondAward_Stunned(award, stunState.time));
        }
    }
}
