using HarmonyLib;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

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
    internal static class CrawlspaceEntityPatches
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

        // Principal sends player to detention in Crawlspace
        [HarmonyPatch(typeof(Principal), "SendToDetention"), HarmonyPrefix]
        [HarmonyPatch(typeof(MrDaycare), "SendToTimeout", typeof(bool))]
        private static void PrincipalSendPlayerToDetention(Principal __instance, PlayerManager ___targetedPlayer, bool validCollision)
        {
            if (!CrawlspaceEvent.Instance || __instance.ec != CrawlspaceEvent.Instance.CrawlspaceEc)
                return;

            __instance.GetComponent<CrawlspaceEntity>().SetEnvironmentController(CrawlspaceEvent.Instance.ec);

            if (!validCollision) return;

            ___targetedPlayer.GetComponent<CrawlspaceEntity>().SetEnvironmentController(CrawlspaceEvent.Instance.ec);
        }
        

        [HarmonyPatch(typeof(Entity), "CopyStatusEffects"), HarmonyPostfix]
        private static void CopyHeightDifference(Entity __instance, Entity entityToCopy)
            => EntityHeightFixer.GetInstance(__instance).heightDifference = EntityHeightFixer.GetInstance(entityToCopy).heightDifference;

        [HarmonyPatch(typeof(Entity), "Initialize"), HarmonyPostfix]
        private static void EntityInitialize(Entity __instance)
        {
            if (__instance.CompareTag("NPC") && __instance.GetComponent<Bully>()) return;

            if (CrawlspaceEvent.Instance && !__instance.GetComponent<CrawlspaceEntity>())
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


        [HarmonyPatch(typeof(Navigator), "BuildNavPath"), HarmonyPrefix]
        private static bool BuildNavPathPrefix(Navigator __instance, Cell firstOpenTile, Cell lastOpenTile)
        {
            if (firstOpenTile == lastOpenTile)
            {
                __instance.destinationPoints.Add(firstOpenTile.CenterWorldPosition + Vector3.up * __instance.ec.Height);
                return false;
            }
            return true;
        }



        private static readonly object zeroOutY = AccessTools.Method(typeof(Vector3Extensions), "ZeroOutY");
        private static readonly object floorWorldPosGetter = AccessTools.PropertyGetter(typeof(Cell), "FloorWorldPosition");


        // For nav meshes, duh!
        private static readonly MethodInfo correctYMethod = AccessTools.Method(typeof(CrawlspaceEntityPatches), "CorrectY");
        private static Vector3 CorrectY(this Vector3 v, Navigator nav)
            => new(v.x, nav.npc.ec.Height, v.z);

        // For the singular cell
        private static readonly MethodInfo accurateCellPos = AccessTools.Method(typeof(CrawlspaceEntityPatches), "GetAccurateCellPos");
        private static Vector3 GetAccurateCellPos(this Cell cell)
        {
            Vector3 pos = cell.FloorWorldPosition;
            pos.y = cell.room.ec.Height;
            return pos;
        }

        [HarmonyPatch(typeof(Navigator), "BuildNavPath"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BuildNavPathTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            byte patchesDone = 0;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length; i++)
            {
                // vector.ZeroOutY()
                if (array[i].opcode   == OpCodes.Call    &&
                    array[i].operand  == zeroOutY)
                {
                    patchesDone++;

                    // vector.CorrectY(this)
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    array[i].operand = correctYMethod;
                    yield return array[i];
                    continue;
                }
                // cell.FloorWorldPosition
                if (array[i].opcode   == OpCodes.Callvirt    &&
                    array[i].operand  == floorWorldPosGetter)
                {
                    patchesDone++;

                    // cell.GetAccurateCellPos()
                    yield return new CodeInstruction(OpCodes.Call, accurateCellPos);
                    continue;
                }
                yield return array[i];
            }

            if (patchesDone == 0)
                RecommendedCharsPlugin.Log.LogError("No patches have been done for \"RecommendedChars.CrawlspaceEntityPatches.BuildNavPathTranspiler\"!");

            yield break;
        }
    }
}
