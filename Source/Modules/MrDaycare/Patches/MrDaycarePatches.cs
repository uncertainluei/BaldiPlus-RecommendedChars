using HarmonyLib;

using System;
using System.Linq;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
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
        [HarmonyPatch(typeof(BaseGameManager), "NoiseMade", typeof(EnvironmentController), typeof(Vector3), typeof(int)), HarmonyPostfix]
        private static void OnNoiseMade(EnvironmentController ec, Vector3 position, int value)
        {
            if (ec.silent || ec.CellFromPosition(IntVector2.GetGridPosition(position)).Silent || value < 70) return;

            _lowestDist = Mathf.RoundToInt(0.045f*value);
            _player = null;

            foreach (PlayerManager player in ec.Players)
            {
                if (player == null || player.ec != ec || player.dijkstraMap == null) continue;

                IntVector2 gridPos = IntVector2.GetGridPosition(position);
                if (gridPos.x < 0 || gridPos.x >= player.dijkstraMap.size.x ||
                    gridPos.z < 0 || gridPos.z >= player.dijkstraMap.size.z)
                    continue;

                if (player.dijkstraMap.Value(gridPos) < _lowestDist)
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
