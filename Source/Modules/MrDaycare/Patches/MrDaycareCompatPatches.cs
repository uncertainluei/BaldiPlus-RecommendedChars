using BaldisBasicsPlusAdvanced.Game.Events;
using BaldisBasicsPlusAdvanced.Game.Objects.Food;
using BaldisBasicsPlusAdvanced.Game.Objects.Voting.Topics;
using BaldisBasicsPlusAdvanced.Patches.Characters;

using brobowindowsmod.ItemScripts;
using brobowindowsmod;

using ecopack;

using HarmonyLib;
using MTM101BaldAPI;

using System.Linq;
using CRAZYBABYJUMPSCARE;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [ConditionalPatchMod(RecommendedCharsPlugin.AdvancedGuid)]
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "MrDaycare")]
    [HarmonyPatch]
    static class MrDaycareAdvancedPatches
    {
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

        [HarmonyPatch(typeof(PlateFoodTrap), "Eat"), HarmonyPostfix]
        private static void PlateFoodScold(Entity entity)
        {
            if (entity.CompareTag("Player") && entity.TryGetComponent(out PlayerManager pm))
                DaycareGuiltManager.GetInstance(pm).BreakRule("Eating", 0.8f, 0.25f);
        }
    }

    [ConditionalPatchMod(RecommendedCharsPlugin.FragileWindowsGuid)]
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "MrDaycare")]
    [HarmonyPatch]
    static class MrDaycareFragilePatches
    {
        [HarmonyPatch(typeof(CannonWindowHotspot), "Clicked"), HarmonyPrefix]
        private static void GlassCannonScold(int playerNumber)
            => DaycareGuiltManager.GetInstance(CoreGameManager.Instance.GetPlayer(playerNumber)).BreakRule("Throwing", 0.8f, 0.25f);
    }

    [ConditionalPatchMod(RecommendedCharsPlugin.EcoFriendlyGuid)]
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "MrDaycare")]
    [HarmonyPatch]
    static class MrDaycareEcoFriendlyPatches
    {
        [HarmonyPatch(typeof(CheeseStand), "EatCheese"), HarmonyPostfix]
        private static void CheeseStandScold(PlayerManager pm)
            => DaycareGuiltManager.GetInstance(pm).BreakRule("Eating", 1.2f, 0.25f);
    }
}
