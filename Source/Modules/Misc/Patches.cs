using HarmonyLib;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch]
    static class SpoilerAreaPatches
    {
        private static float _physicalHeight;
        private static Entity _entity;
        private static PlayerManager _player;

        [HarmonyPatch(typeof(Entity), "EntityUpdate"), HarmonyPrefix]
        private static void EntityUpdatePrefix(Entity __instance)
        {
            _physicalHeight = Entity.physicalHeight;

            if (BaseGameManager.Instance is not PartyWinManager) return;
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

        internal static bool surpressDijkstraOOB = false;

        // Silence out of range errors by returning infinite when out of bounds
        [HarmonyPatch(typeof(DijkstraMap), "Value", typeof(int), typeof(int)), HarmonyPrefix]
        private static bool DijkstraMapValueInt(ref int __result, IntVector2 ___size, int x, int z)
        {
            if (!surpressDijkstraOOB) return true;
            if (x >= 0 && x < ___size.x && z >= 0 && z < ___size.z)
                return true;

            __result = int.MaxValue;
            return false;
        }
        [HarmonyPatch(typeof(DijkstraMap), "Value", typeof(IntVector2)), HarmonyPrefix]
        private static bool DijkstraMapValueIntPos2(ref int __result, IntVector2 ___size, IntVector2 position)
            => DijkstraMapValueInt(ref __result, ___size, position.x, position.z);

        [HarmonyPatch(typeof(ITM_Teleporter), "Use"), HarmonyPrefix]
        private static void TeleporterUse(PlayerManager pm)
        {
            if (pm.ec == BaseGameManager.Instance.Ec) return;

            pm.ec = BaseGameManager.Instance.Ec;
            pm.plm.height = pm.ec.Height+5f;
            pm.plm.Entity.environmentController = pm.ec;

            if (pm.playerNumber > 0) return;

            pm.dijkstraMap.Deactivate();
            pm.dijkstraMap.environment = pm.ec;
            pm.dijkstraMap.Activate();
            pm.dijkstraMap.QueueUpdate();
            GameCamera.dijkstraMap.Deactivate();
            GameCamera.dijkstraMap.environment = pm.ec;
            GameCamera.dijkstraMap.Activate();
            GameCamera.dijkstraMap.QueueUpdate();
        }
    }
}
