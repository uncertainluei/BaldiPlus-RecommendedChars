using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.FragileWindows;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LegacyEditor;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [BepInAutoPlugin(ModGuid, ModName), BepInDependency(CaudexLibGuid)]

    [BepInDependency(CrispyPlusGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PineDebugGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CustomMusicsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AnimationsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CharacterRadarGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AdvancedGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LevelLoaderGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LegacyEditorGuid, BepInDependency.DependencyFlags.SoftDependency)]

    // Make sure this loads BEFORE the mod does
    [BepInDependency(ConnectorGuid, BepInDependency.DependencyFlags.SoftDependency)]

    [BepInDependency(FragileWindowsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(EcoFriendlyGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CrazyBabyGuid, BepInDependency.DependencyFlags.SoftDependency)]

    partial class RecommendedCharsPlugin : BaseUnityPlugin
    {
        internal const string ModGuid = "io.github.uncertainluei.baldiplus.recommendedchars";
        internal const string ModName = "Recommended Characters Pack";

        internal static readonly AssetManager AssetMan = new();
        internal static RecommendedCharsPlugin Plugin { get; private set; }
        internal static ManualLogSource Log { get; private set; }
        internal static Harmony Hooks { get; private set; }

        private void Awake()
        {
            Plugin = this;
            Log = Logger;

            this.SetAssetsDirectory("uncertainluei", "recommendedchars");

            // Read the config values and remove disabled modules
            RecommendedCharsConfig.BindConfig(Config);

            RecommendedCharsSaveGameIO saveGameSystem = new(Info);
            ModdedSaveGame.AddSaveHandler(saveGameSystem);
            ModdedHighscoreManager.AddModToList(Info, saveGameSystem.GenerateTags());
            QueuedActions.QueueAction(() =>
            {
                saveGameSystem.modulesInit = true;
                ModdedFileManager.Instance.RegenerateTags();
            });

            // Load localization files
            AssetLoader.LoadLocalizationFolder(Path.Combine(AssetLoader.GetModPath(this), "Lang", "English"), Language.English);

            LoadingEvents.RegisterOnAssetsLoaded(Info, GrabBaseAssets(), LoadingEventOrder.Pre);

            Hooks = new(ModGuid);
            Hooks.PatchAll(typeof(LevelGeneratorEventPatch));

            PatchCompat(typeof(PineDebugNpcIconPatch), PineDebugGuid);
            PatchCompat(typeof(CharacterRadarColorPatch), CharacterRadarGuid);
            PatchCompat(typeof(LegacyEditorCompatHelper), LegacyEditorGuid);
            PatchCompat(typeof(FragileMiscPatches), FragileWindowsGuid);
            PatchCompat(typeof(WindowletVariantPatches), FragileWindowsGuid);
        }

        private IEnumerator GrabBaseAssets()
        {
            yield return 1;
            yield return "Grabbing base assets";
            // Sprite materials
            Material[] materials = [.. Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0)];
            AssetMan.Add("BillboardMaterial", materials.First(x => x.name == "SpriteStandard_Billboard"));
            AssetMan.Add("NoBillboardMaterial", materials.First(x => x.name == "SpriteStandard_NoBillboard"));
        }
    }

    public class RecommendedCharsSaveGameIO(PluginInfo info) : ModdedSaveGameIOBinary
    {
        private readonly PluginInfo info = info;
        public override PluginInfo pluginInfo => info;
        internal bool modulesInit = false;

        private const byte SaveVersion = 1;

        public override void OnCGMCreated(CoreGameManager cgm, bool savedGame)
        {
            foreach (AbstractCaudexModule module in info.GetActiveCaudexModules())
                ((RecCharsModule)module).SaveSystem?.CoreGameManCreated(cgm, savedGame);
        }

        public override void Load(BinaryReader reader)
        {
            byte ver = reader.ReadByte();
            if (ver == 0) return;
            foreach (AbstractCaudexModule module in info.GetActiveCaudexModules())
                ((RecCharsModule)module).SaveSystem?.Load(reader);
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(SaveVersion);
            foreach (AbstractCaudexModule module in info.GetActiveCaudexModules())
                ((RecCharsModule)module).SaveSystem?.Save(writer);
        }

        public override void Reset()
        {
            foreach (AbstractCaudexModule module in info.GetActiveCaudexModules())
                ((RecCharsModule)module).SaveSystem?.Reset();
        }

        public override bool TagsReady() => modulesInit;
        public override string[] GenerateTags()
        {
            if (!modulesInit) return [];

            List<string> tags = [];
            
            foreach (string module in info.GetActiveCaudexModuleTags())
                tags.Add(module);
            if (tags.Count > 0 && RecommendedCharsConfig.guaranteeSpawnChar.Value)
                tags.Add("GuaranteedSpawn");

            return [.. tags];
        }

        public override string DisplayTags(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return "No save tags.";

            string display = "<b>Modules:</b> ";

            bool addComma = false;
            byte entryCount = 0;

            foreach (string tag in tags)
            {
                if (tag == "GuaranteedSpawn")
                {
                    display = "<b>Guaranteed Character Spawns</b>\n" + display;
                    break;
                }

                if (addComma)
                    display += "," + (entryCount % 3 == 0 ? "\n" : " ");

                addComma = true;
                entryCount++;

                display += tag;
            }
            return display;
        }
    }
}