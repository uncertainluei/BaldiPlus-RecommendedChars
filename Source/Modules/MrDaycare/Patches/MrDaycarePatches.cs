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
    }
}
