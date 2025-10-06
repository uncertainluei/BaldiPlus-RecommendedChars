using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch(typeof(LevelGenerator))]
    static class LevelGeneratorPatches
    {
        // If a larger room won't work, then keep trying until there are none left
        private static bool BruteForceRoomPlacements(LevelGenerator gen, WeightedSelection<RoomAsset>[] rooms, Random rng, bool addDoor, out RoomController result)
        {
            List<WeightedSelection<RoomAsset>> roomsAsList = new(rooms);
            int i;
            while (roomsAsList.Count > 0)
            {
                i = WeightedSelection<RoomAsset>.ControlledRandomIndexList(roomsAsList, rng);
                if (gen.RandomlyPlaceRoom(roomsAsList[i].selection, addDoor, out result))
                    return true;
                    
                roomsAsList.RemoveAt(i);
            }
            RecommendedCharsPlugin.Log.LogWarning("Failed to spawn any room assets!");

            result = null;
            return false;
        }

        private static readonly MethodInfo bruteForcePlacements = AccessTools.Method(typeof(LevelGeneratorPatches), "BruteForceRoomPlacements");
        private static readonly object controlledRng = AccessTools.Field(typeof(LevelBuilder), "controlledRNG");
        private static readonly object randomSelection = AccessTools.Method(typeof(WeightedSelection<RoomAsset>), "ControlledRandomSelection");

        // If there are any BepInEx/Harmony professionals who know how to check for out parameters any other way, then tell me ASAP.
        private static readonly object randomlyPlace = AccessTools.FirstMethod(typeof(LevelGenerator), (method) =>
        {
            if (method.Name != "RandomlyPlaceRoom")
                return false;

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 3 && parameters[2].IsOut;
        });

        [HarmonyPatch("Generate", MethodType.Enumerator), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RoomPlacementTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            byte patchesDone = 0;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length; i++)
            {
                // if (levelGenerator.RandomlyPlaceRoom(WeightedSelection<RoomAsset>.ControlledRandomSelection(potentialRooms, levelGenerator.controlledRNG), addDoor: true, out newRoom))
                if (i+6 < length &&
                    array[i].opcode   == OpCodes.Ldloc_S    &&
                    array[i+1].opcode == OpCodes.Ldloc_2    &&
                    array[i+2].opcode == OpCodes.Ldfld      &&
                    array[i+2].operand == controlledRng     &&
                    array[i+3].opcode == OpCodes.Call       &&
                    array[i+3].operand == randomSelection   &&
                    array[i+4].opcode == OpCodes.Ldc_I4_1   &&
                    array[i+5].opcode == OpCodes.Ldloca_S   &&
                    array[i+6].opcode == OpCodes.Call       &&
                    array[i+6].operand == randomlyPlace     &&
                    array[i+7].opcode == OpCodes.Brfalse)
                {
                    patchesDone++;
                    yield return array[i];
                    yield return array[i+1];
                    yield return array[i+2];
                    yield return array[i+4];
                    yield return array[i+5];
                    yield return new CodeInstruction(OpCodes.Call, bruteForcePlacements);
                    i += 7;
                }
                yield return array[i];
            }

            if (patchesDone == 0)
                RecommendedCharsPlugin.Log.LogError("No patches have been done for \"RecommendedChars.LevelGeneratorPatches.RoomPlacementTranspiler\"!");

            yield break;
        }
    }
}
