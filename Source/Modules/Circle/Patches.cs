using HarmonyLib;

using MTM101BaldAPI;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "Circle")]
    [HarmonyPatch]
    static class CirclePatches
    {
        [HarmonyAfter(RecommendedCharsPlugin.AnimationsGuid)]
        [HarmonyPatch(typeof(Playtime), "EndJumprope"), HarmonyPostfix]
        private static void EndJumprope(Playtime __instance, bool won)
        {
            if (__instance.Character != CircleNpc.charEnum) return;

            CircleNpc circle = (CircleNpc)__instance;

            if (!won)
            {
                // Re-disable the animator for good measure
                __instance.animator.enabled = false;
                circle.Navigator.maxSpeed = circle.sadSpeed;
                circle.Navigator.SetSpeed(circle.sadSpeed);
                circle.sprite.sprite = circle.sprSad;
                return;
            }
            // If you win the jump rope game, then his cooldown is instead 50 seconds
            if (circle.behaviorStateMachine.currentState is Playtime_Cooldown cooldown)
                cooldown.time = circle.successCooldown;
        }

        [HarmonyAfter(RecommendedCharsPlugin.AnimationsGuid)]
        [HarmonyPatch(typeof(Playtime), "EndCooldown"), HarmonyPostfix]
        private static void EndCooldown(Playtime __instance)
        {
            if (__instance.Character == CircleNpc.charEnum)
            {
                // Re-disable the animator for good measure
                __instance.animator.enabled = false;

                CircleNpc circle = (CircleNpc)__instance;
                circle.sprite.sprite = circle.sprNormal;
            }
        }

        private static readonly MethodInfo jumpropeCheckMethod = AccessTools.Method(typeof(CirclePatches), "JumpropeCheck");
        private static bool JumpropeCheck(PlayerManager pm)
        {
            bool success = false;
            for (int i = pm.jumpropes.Count - 1; i >= 0; i--)
            {
                if (pm.jumpropes[i] is not CircleJumprope)
                {
                    success = true;
                    pm.jumpropes[i].End(false);
                }
            }
            return success;
        }

        [HarmonyPatch(typeof(ITM_Scissors), "Use"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ScissorsUseTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length; i++)
            {
                yield return array[i];

                if (i >= 4 &&
                    array[i].opcode   == OpCodes.Ble &&
                    array[i-1].opcode == OpCodes.Ldc_I4_0 &&
                    array[i-2].opcode == OpCodes.Callvirt &&
                    array[i-3].opcode == OpCodes.Ldfld &&
                    array[i-4].opcode == OpCodes.Ldarg_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, jumpropeCheckMethod);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, array[i].operand);
                    break;
                }
            }
            for (i++; i < length; i++)
            {
                if (array[i].opcode != OpCodes.Bgt) continue;

                patched = true;
                break;
            }
            for (i++; i < length; i++)
            {
                yield return array[i];
            }

            if (!patched)
                RecommendedCharsPlugin.Log.LogError("Transpiler \"RecommendedChars.CirclePatches.ScissorsUseTranspiler\" wasn't properly applied!");

            yield break;
        }
    }

    [ConditionalPatchMod(RecommendedCharsPlugin.CustomMusicsGuid)]
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "Circle")]
    [HarmonyPatch(typeof(BBPlusCustomMusics.Patches.PlaytimeDingOverridePatch), "PlaytimeDingOverride")]
    static class CircleMusicCompatPatch
    {
        private static bool Prefix(Playtime __0) => __0.Character != CircleNpc.charEnum;
    }
}
