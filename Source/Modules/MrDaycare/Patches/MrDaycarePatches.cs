using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using UnityEngine;

using BaldisBasicsPlusAdvanced.Game.Events;
using BaldisBasicsPlusAdvanced.Game.Objects.Voting.Topics;
using BaldisBasicsPlusAdvanced.Patches.Characters;

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
        private static void OnUseWhistle(PlayerManager pm)
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
            if (meta == null) return;
            if (meta.tags.Contains("food") && !meta.tags.Contains("drink") && !meta.tags.Contains("recchars_daycare_exempt"))
            {
                DaycareGuiltManager.GetInstance(itemMan.pm).BreakRule("Eating", 0.8f, 0.25f);
                return;
            }
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

    [ConditionalPatchMod(RecommendedCharsPlugin.AdvancedGuid)]
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "MrDaycare")]
    [HarmonyPatch]
    static class MrDaycareAdvancedPatches
    {
        // Prioritize getting the Principal, part of the Advanced PR
        private static Principal GetPrincipal()
        {
            Principal fallback = null;
            foreach (NPC npc in BaseGameManager.Instance.Ec.Npcs)
            {
                if (npc is not Principal pri) continue;
                fallback = pri;
                if (npc.Character == Character.Principal)
                    return pri;
            }
            return fallback;
        }

        private static readonly MethodInfo getPrincipalMethod = AccessTools.Method(typeof(MrDaycareAdvancedPatches), "GetPrincipal");

        // Temporary patch until Advanced updates with the PR
        [HarmonyPatch(typeof(VotingEvent), "Begin"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> VotingBeginTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length && !patched; i++)
            {
                if (array[i].opcode == OpCodes.Call &&
                    array[i].operand?.ToString() == "Principal FindObjectOfType[Principal]()")
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Call, getPrincipalMethod);
                    continue;
                }
                yield return array[i];
            }
            for (; i < length; i++)
            {
                yield return array[i];
            }

            if (!patched)
                RecommendedCharsPlugin.Log.LogWarning("Transpiler \"RecommendedChars.MrDaycareAdvancedPatches.VotingBeginTranspiler\" wasn't properly applied! It is extremely likely Advanced has released a fix!");

            yield break;
        }

        [HarmonyPatch(typeof(VotingEvent.PrincipalController), "SetCheckingRoomMode"), HarmonyPrefix]
        private static bool VotingEventCheck(bool value, Principal ___principal, ref NavigationState_PartyEvent ___state, RoomController ___room)
        {
            if (!value || ___principal == null || ___principal.Character != MrDaycare.charEnum) return true;

            MrDaycare daycare = (MrDaycare)___principal;

            daycare.behaviorStateMachine.ChangeState(new MrDaycare_Wandering(daycare));
            daycare.Navigator.Entity.SetBlinded(true);
            ___state = new NavigationState_PartyEvent(daycare, int.MaxValue, ___room);
            daycare.navigationStateMachine.ChangeState(___state);
            return false;
        }


        [HarmonyPatch(typeof(MrDaycare), "ObservePlayer"), HarmonyPrefix]
        private static bool MrDaycareIgnoreRules(PlayerManager player)
        {
            if (!VotingEvent.TopicIsActive<PrincipalIgnoresSomeRulesTopic>()) return true;

            return !PrincipalObservePatch.allowedRulesWhenTopicActive.Contains(DaycareGuiltManager.GetInstance(player).RuleBreak);
        }
    }
}
