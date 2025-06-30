using HarmonyLib;

using MTM101BaldAPI;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

using UnityEngine;
using System;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [ConditionalPatchMod("alexbw145.baldiplus.pinedebug")]
    [HarmonyPatch(typeof(PineDebug.PineDebugManager), "InitAssets")]
    static class PineDebugNpcIconPatch
    {
        internal static readonly Dictionary<Character, Texture2D> icons = new Dictionary<Character, Texture2D>();
        private static bool initialized;

        private static void Postfix()
        {
            if (initialized) return;
            initialized = true;

            foreach (Character character in icons.Keys)
                PineDebug.PineDebugManager.pinedebugAssets.Add($"Border{character.ToStringExtended()}", icons[character]);
        }
    }

    [HarmonyPatch(typeof(LevelGenerator), "Generate", MethodType.Enumerator)]
    static class LevelGeneratorEventPatch
    {
        public static event Action<LevelGenerator> OnNpcAdd;
        public static event Action<LevelGenerator> OnGeneratorCompletion;

        private static readonly MethodInfo npcAddMethod = AccessTools.Method(typeof(LevelGeneratorEventPatch), "NpcAddInvoke");
        private static readonly MethodInfo genCompleteMethod = AccessTools.Method(typeof(LevelGeneratorEventPatch), "GeneratorCompletionInvoke");

        private static void NpcAddInvoke(LevelGenerator gen)
        {
            try
            {
                OnNpcAdd?.Invoke(gen);
            }
            catch (Exception e)
            {
                RecommendedCharsPlugin.Log.LogError(e);
            }
        }

        private static void GeneratorCompletionInvoke(LevelGenerator gen)
        {
            try
            {
                OnGeneratorCompletion?.Invoke(gen);
            }
            catch (Exception e)
            {
                RecommendedCharsPlugin.Log.LogError(e);
            }
        }

        [HarmonyBefore(RecommendedCharsPlugin.ApiGuid)]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            byte patchesLeft = 2;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length && patchesLeft == 2; i++)
            {
                // if (levelGenerator.ld.potentialBaldis.Length != 0)
                if (array[i].opcode   == OpCodes.Ldloc_2 &&
                    array[i+1].opcode == OpCodes.Ldfld &&
                    array[i+2].opcode == OpCodes.Ldfld &&
                    array[i+3].opcode == OpCodes.Ldlen &&
                    array[i+4].opcode == OpCodes.Brfalse)
                {
                    patchesLeft--;
                    // NpcAddInvoke(this);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, npcAddMethod);
                }
                yield return array[i];
            }
            for (; i < length && patchesLeft == 1; i++)
            {
                // levelInProgress = false;
                // levelCreated = true;
                // if (CoreGameManager.Instance.GetCamera(0) != null)
                //     CoreGameManager.Instance.GetCamera(0).StopRendering(false);
                if (array[i].opcode    == OpCodes.Ldloc_2 &&
                    array[i+1].opcode  == OpCodes.Ldc_I4_0 &&
                    array[i+2].opcode  == OpCodes.Stfld &&
                    array[i+3].opcode  == OpCodes.Ldloc_2 &&
                    array[i+4].opcode  == OpCodes.Ldc_I4_1 &&
                    array[i+5].opcode  == OpCodes.Stfld &&
                    array[i+6].opcode  == OpCodes.Call &&
                    array[i+7].opcode  == OpCodes.Ldc_I4_0 &&
                    array[i+8].opcode  == OpCodes.Callvirt &&
                    array[i+9].opcode  == OpCodes.Ldnull &&
                    array[i+10].opcode == OpCodes.Call &&
                    array[i+11].opcode == OpCodes.Brfalse &&
                    array[i+12].opcode == OpCodes.Call  &&
                    array[i+13].opcode == OpCodes.Ldc_I4_0 &&
                    array[i+14].opcode == OpCodes.Callvirt &&
                    array[i+13].opcode == OpCodes.Ldc_I4_0 &&
                    array[i+14].opcode == OpCodes.Callvirt)
                {
                    patchesLeft--;
                    // GeneratorCompletionInvoke(this);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, genCompleteMethod);
                }
                yield return array[i];
            }
            for (; i < length; i++)
            {
                yield return array[i];
            }

            if (patchesLeft > 0)
                RecommendedCharsPlugin.Log.LogError("Transpiler \"RecommendedChars.LevelGeneratorEventPatch.Transpiler\" did not go through!");

            yield break;
        }
    }
}
