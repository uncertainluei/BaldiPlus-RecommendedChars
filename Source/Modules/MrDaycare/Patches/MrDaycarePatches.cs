using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.Registers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "MrDaycare")]
    [HarmonyPatch]
    static class MrDaycarePatches
    {    
        [HarmonyPatch(typeof(PlayerManager), "RuleBreak", typeof(string), typeof(float), typeof(float)), HarmonyPostfix]
        private static void DaycareRuleBreak(PlayerManager __instance, string rule, float linger, float sensitivity)
        {
            if (daycareRules.Contains(rule))
                DaycareGuiltManager.GetInstance(__instance).BreakRule(rule, linger, sensitivity);
        }
        public static readonly string[] daycareRules =
        [
            "Running",
            "Drinking",
            "Eating",
            "DaycareEscaping",
            "Throwing",
            "LoudSound"
        ];

        [HarmonyPatch(typeof(ITM_PrincipalWhistle), "Use"), HarmonyPostfix]
        private static void WhistleScold(PlayerManager pm)
        {
            if (!pm.ec.silent && !pm.ec.CellFromPosition(IntVector2.GetGridPosition(pm.transform.position)).Silent)
                DaycareGuiltManager.GetInstance(pm).BreakRule("LoudSound", 1.5f, 0.5f);
        }

        private static int _lowestDist;
        private static PlayerManager _player;
        [HarmonyPatch(typeof(EnvironmentController), "MakeNoise", typeof(GameObject), typeof(Vector3), typeof(int)), HarmonyPostfix]
        private static void OnNoiseMade(EnvironmentController __instance, Vector3 position, int value)
        {
            if (__instance.silent || __instance.CellFromPosition(IntVector2.GetGridPosition(position)).Silent || value < 70) return;

            _lowestDist = Mathf.RoundToInt(0.045f*value);
            _player = null;

            foreach (PlayerManager player in __instance.Players)
            {
                if (player == null) continue;

                if (player.dijkstraMap.Value(IntVector2.GetGridPosition(position)) < _lowestDist)
                {
                    _lowestDist = player.dijkstraMap.Value(IntVector2.GetGridPosition(position));
                    _player = player;
                }
            }
            if (_player)
                DaycareGuiltManager.GetInstance(_player).BreakRule("LoudSound", 0.02f*value, 0.5f);
        }

        public static readonly MethodInfo itemUsedMethod = AccessTools.Method(typeof(MrDaycarePatches), "OnItemUseSuccess");
        public static void OnItemUseSuccess(ItemManager itemMan, int slot)
        {
            ItemMetaData meta = itemMan.items[slot].GetMeta();
            if (meta == null || meta.tags.Contains("recchars_daycare_exempt")) return;

            if (meta.tags.Contains("food") && !meta.tags.Contains("drink"))
            {
                DaycareGuiltManager.GetInstance(itemMan.pm).BreakRule("Eating", 0.8f, 0.25f);
                return;
            }
            if (meta.tags.Contains("recchars_daycare_throwable"))
                DaycareGuiltManager.GetInstance(itemMan.pm).BreakRule("Throwing", 0.8f, 0.25f);
        }

        [HarmonyPatch(typeof(ItemManager), "UseItem"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckForItemUse(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;
            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length;

            for (int i = 0; i < length; i++)
            {
                yield return array[i];
                if (!patched &&
                    array[i].opcode == OpCodes.Brfalse &&
                    array[i + 1].opcode == OpCodes.Ldarg_0 &&
                    array[i + 2].opcode == OpCodes.Ldarg_0)
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return array[i + 3]; // this.selectedItem
                    yield return new CodeInstruction(OpCodes.Call, itemUsedMethod); //OnItemUseSuccess()
                }
            }

            if (!patched)
                RecommendedCharsPlugin.Log.LogError("Transpiler \"MrDaycarePatches.CheckForItemUse\" wasn't properly applied!");

            yield break;
        }
    }
}
