using UnityEngine;
using HarmonyLib;
using UnityEngine.AI;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class EntityHeightFixer : MonoBehaviour
    {
        private static Entity _player;
        private static EntityHeightFixer _instance;
        public static EntityHeightFixer GetInstance(Entity entity)
        {
            if (_player == entity && _instance)
                return _instance;

            _player = entity;
            if (!_player.TryGetComponent(out _instance))
                _instance = _player.gameObject.AddComponent<EntityHeightFixer>();

            return _instance;
        }

        public float heightDifference = 0;
    }

    [HarmonyPatch]
    internal static class CrawlspacePatches
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

        [HarmonyPatch(typeof(Entity), "Initialize"), HarmonyPostfix]
        private static void EntityInitialize(Entity __instance)
        {
            if (CrawlspaceEvent.Instance)
                __instance.gameObject.AddComponent<CrawlspaceEntity>();
        }

        private static float _physicalHeight, _playerHeight;

        [HarmonyPatch(typeof(PlayerMovement), "PlayerMove"), HarmonyPrefix]
        private static void PlayerMovePrefix(PlayerMovement __instance)
        {
            _playerHeight = __instance.height;
            __instance.height += EntityHeightFixer.GetInstance(__instance.Entity).heightDifference;
        }
        [HarmonyPatch(typeof(PlayerMovement), "PlayerMove"), HarmonyPostfix]
        private static void PlayerMovePostfix(PlayerMovement __instance)
            => __instance.height = _playerHeight;

        [HarmonyPatch(typeof(Entity), "EntityUpdate"), HarmonyPrefix]
        private static void EntityUpdatePrefix(Entity __instance)
        {
            _physicalHeight = Entity.physicalHeight;
            Entity.physicalHeight += EntityHeightFixer.GetInstance(__instance).heightDifference;
        }
        [HarmonyPatch(typeof(Entity), "EntityUpdate"), HarmonyPostfix]
        private static void EntityUpdatePostfix()
            => Entity.physicalHeight = _physicalHeight;
    }
}
