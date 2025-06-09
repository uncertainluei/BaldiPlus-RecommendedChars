using HarmonyLib;

using MTM101BaldAPI;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

using UnityEngine;
using MTM101BaldAPI.Registers;
using System;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Patches
{
    [HarmonyPatch(typeof(ITM_Scissors), "Use")]
    class ScissorsItemPatch
    {
        private static readonly MethodInfo jumpropeCheckMethod = AccessTools.Method(typeof(ScissorsItemPatch), "HasCutAnyJumpropes");

        private static bool HasCutAnyJumpropes(PlayerManager pm)
        {
            bool success = false;
            for (int i = pm.jumpropes.Count - 1; i >= 0; i--)
            {
                if (!(pm.jumpropes[i] is CircleJumprope))
                {
                    success = true;
                    pm.jumpropes[i].End(false);
                }
            }
            return success;
        }

        [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "ArtsWWires")]
        private static void Postfix(PlayerManager pm, ref bool __result, SoundObject ___audSnip)
        {
            bool oldResult = __result;
            if (pm.TryGetComponent(out GrabbingGameContainer grabbed) && grabbed.TryCutGrabbingGames())
                __result = true;

            if (oldResult != __result)
                CoreGameManager.Instance.audMan.PlaySingle(___audSnip);
        }

        [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "Circle")]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length; i++)
            {
                yield return array[i];

                if (array[i].opcode == OpCodes.Ble &&
                    array[i - 1].opcode == OpCodes.Ldc_I4_0 &&
                    array[i - 2].opcode == OpCodes.Callvirt &&
                    array[i - 3].opcode == OpCodes.Ldfld &&
                    array[i - 4].opcode == OpCodes.Ldarg_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, jumpropeCheckMethod);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, array[i].operand);
                    break;
                }
            }
            for (i++; i < length; i++)
            {
                if (array[i].opcode == OpCodes.Bgt)
                {
                    patched = true;
                    break;
                }
            }
            for (i++; i < length; i++)
            {
                yield return array[i];
            }

            if (!patched)
                Debug.LogError("Transpiler \"ShapeWorldCircle.ScissorsItemPatch.Transpiler\" did not go through!");

            yield break;
        }
    }

    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "CaAprilFools")]
    [HarmonyPatch(typeof(LevelGenerator), "StartGenerate")]
    class ManMemeCoinMainSpawn
    {
        // This function in itself is pretty much boilerplate, transpiling the generator would be more ideal
        private static IEnumerator AddManMemeCoin(SceneObject scene, ManMemeCoin coin)
        {
            NPC[] npcs = scene.forcedNpcs;
            scene.forcedNpcs = scene.forcedNpcs.AddToArray(coin);
            // Revert once the cells are initialised, as that's when the NPCs have been added to the list
            yield return new WaitUntil(() => BaseGameManager.Instance.Ec.cells?.Length > 0);
            scene.forcedNpcs = npcs;
        }

        private static void Prefix(SceneObject ___scene)
        {
            if (___scene == null) return;

            SceneObjectMetadata meta = SceneObjectMetaStorage.Instance.Get(___scene);
            if (meta == null)
            {
                if (!___scene.levelTitle.StartsWith("F")) return;
            }
            else if (!meta.tags.Contains("main")) return;

            int chance = ___scene.levelNo;
            SceneObject next = ___scene;
            while (next = next.nextLevel)
                chance++;

            chance = new System.Random(CoreGameManager.Instance.Seed() + 41223).Next(chance);
            if (chance == ___scene.levelNo)
                BaseGameManager.Instance.StartCoroutine(AddManMemeCoin(___scene, RecommendedCharsPlugin.AssetMan.Get<ManMemeCoin>("ManMemeCoinNpc")));
        }
    }

    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "ArtsWWires")]
    [HarmonyPatch(typeof(ITM_Boots), "Use")]
    class BootsItemPatch
    {
        private static void Postfix(PlayerManager pm)
        {
            /* I was initially going to do some weird workarounds to remove the MoveMod
             * if you had the boots on, but honestly I might as well just yoink the
             * end game functionality that I used for the scissors
             */
            if (pm.TryGetComponent(out GrabbingGameContainer grabbed))
                grabbed.TryCutGrabbingGames();
        }
    }

    [ConditionalPatchMod("pixelguy.pixelmodding.baldiplus.custommusics")]
    [ConditionalPatchConfig(RecommendedCharsPlugin.ModGuid, "Modules", "Circle")]
    [HarmonyPatch(typeof(BBPlusCustomMusics.MusicalInjection), "PlaytimeDingOverride")]
    class CustomPlaytimeMusicPatch
    {
        private bool Prefix(object[] __args) => ((Playtime)__args[0]).Character != CircleNpc.charEnum;
    }

    [ConditionalPatchMod("alexbw145.baldiplus.pinedebug")]
    [HarmonyPatch(typeof(PineDebug.PineDebugManager), "InitAssets")]
    class PineDebugNpcIconPatch
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
}
