using HarmonyLib;
using UnityEngine.UIElements;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    static class SpoilerAreaPatches
    {
        internal static bool LookerRaycast(PlayerManager player, NPC ___npc, ref bool targetSighted, ref bool ____castFailed)
        {
            targetSighted = false;
            ____castFailed = false;
            if (player && player.ec != ___npc.ec)
            {
                ____castFailed = true;
                return false;
            }
            return true;
        }

        private static float _physicalHeight;
        private static Entity _entity;
        private static PlayerManager _player;

        [HarmonyPatch(typeof(Entity), "EntityUpdate"), HarmonyPrefix]
        private static void EntityUpdatePrefix(Entity __instance)
        {
            _physicalHeight = Entity.physicalHeight;

            if (!__instance.enabled || !__instance.Ec) return;
            if (!__instance.CompareTag("Player"))
            {
                Entity.physicalHeight += __instance.Ec.Height;
                return;
            }
            if (!_entity || __instance != _entity)
                _player = __instance.GetComponent<PlayerManager>();
            _entity = __instance;
            Entity.physicalHeight = _player.plm.height;
        }
        [HarmonyPatch(typeof(Entity), "EntityUpdate"), HarmonyPostfix]
        private static void EntityUpdatePostfix()
            => Entity.physicalHeight = _physicalHeight;

        // Mostly in case of something in a dijkstra map being out of bounds
        [HarmonyPatch(typeof(DijkstraMap), "Value", typeof(int), typeof(int)), HarmonyPrefix]
        private static bool DijkstraMapValueInt(ref int __result, IntVector2 ___size, int x, int z)
        {
            if (x > 0 && x < ___size.x && z > 0 && z < ___size.z)
                return true;

            __result = int.MaxValue;
            return false;
        }
        [HarmonyPatch(typeof(DijkstraMap), "Value", typeof(IntVector2)), HarmonyPrefix]
        private static bool DijkstraMapValueIntPos2(ref int __result, IntVector2 ___size, IntVector2 position)
            => DijkstraMapValueInt(ref __result, ___size, position.x, position.z);
    }
}
