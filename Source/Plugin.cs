using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;

using HarmonyLib;

using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.FragileWindows;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [BepInAutoPlugin(ModGuid, ModName), BepInDependency(CaudexLibGuid, "0.1.0.2")]
    [BepInDependency(LevelLoaderGuid)]

    [BepInDependency(CrispyPlusGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PineDebugGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CustomMusicsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AnimationsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CharacterRadarGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AdvancedGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LevelStudioGuid, BepInDependency.DependencyFlags.SoftDependency)]
    
    [BepInDependency(ConnectorGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(FragileWindowsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(EcoFriendlyGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CrazyBabyGuid, BepInDependency.DependencyFlags.SoftDependency)]

    partial class RecommendedCharsPlugin : BaseUnityPlugin
    {
        internal const string ModGuid = "io.github.uncertainluei.baldiplus.recommendedchars";
        internal const string ModName = "Recommended Characters Pack";

        internal static readonly AssetManager AssetMan = new();
        internal static readonly AssetManager ObjMan = new();

        internal static RecommendedCharsPlugin Plugin { get; private set; }
        internal static ManualLogSource Log { get; private set; }
        internal static Harmony Hooks { get; private set; }

        private void Awake()
        {
            Plugin = this;
            Log = Logger;

            this.SetAssetsDirectory("uncertainluei", "recommendedchars");
            // Gotta make it clear because MANY people are messing this up. You do not need to be a rocket scientist to properly install this mod.
            if (!Directory.Exists(AssetLoader.GetModPath(this)))
            {
                CaudexLibPlugin.CauseDelayedCrash(Info, new Exception("Subfolder \"uncertainluei/recommendedchars\" was not found in the Modded folder! Make sure you install the mod's files properly!"));
                return;
            }

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
            CaudexAssetLoader.LocalizationFromMod(Language.English, this, "Lang", "English", "SaveTags.json5");

            LoadingEvents.RegisterOnAssetsLoaded(Info, GrabBaseAssets(), LoadingEventOrder.Pre);

            // Register built-in patches
            Hooks = new(ModGuid);
            Hooks.PatchAll(typeof(LevelGeneratorPatches));
            Hooks.PatchAll(typeof(NoNpcActivityChaosPatches));
            PatchCompat(typeof(PineDebugNpcIconPatch), PineDebugGuid);
            PatchCompat(typeof(CharacterRadarColorPatch), CharacterRadarGuid);
            PatchCompat(typeof(FragileMiscPatches), FragileWindowsGuid);
            PatchCompat(typeof(WindowletVariantPatches), FragileWindowsGuid);

            //MTM101BaldiDevAPI.AddWarningScreen("You are running a <color=yellow>BETA</color> build of <color=green>Recommended Characters Pack</color>.\nAs such, the content added might not be fully implemented or polished, and you may run into <color=red>BUGS!!!</color>\nPlease report any bugs or crashes to the <color=red>Issues</color> page of the GitHub repo!", false);
        }

        internal static void PatchCompat(Type type, string guid)
        {
            if (Chainloader.PluginInfos.ContainsKey(guid))
                Hooks.PatchAll(type);
        }

        private IEnumerator GrabBaseAssets()
        {
            yield return 1;
            yield return "Grabbing base assets";
            // Sprite materials
            Material[] materials = [.. Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0)];
            AssetMan.Add("BillboardMaterial", materials.First(x => x.name == "SpriteStandard_Billboard"));
            AssetMan.Add("NoBillboardMaterial", materials.First(x => x.name == "SpriteStandard_NoBillboard"));
            // Sound objects
            AssetMan.Add("Sfx/Silence", Resources.FindObjectsOfTypeAll<SoundObject>().First(x => x.name == "Silence" && x.GetInstanceID() >= 0));
        }
    }

    public class RecommendedCharsSaveGameIO(PluginInfo info) : ModdedSaveGameIOBinary
    {
        private readonly PluginInfo info = info;
        public override PluginInfo pluginInfo => info;
        internal bool modulesInit = false;

        private const byte SaveVersion = 1;
        private const byte TagVersion = 1;

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
            
            tags.Add("Version_"+TagVersion);
            foreach (string module in info.GetActiveCaudexModuleTags())
                tags.Add(module);
            if (tags.Count > 0 && RecommendedCharsConfig.guaranteeSpawnChar.Value)
                tags.Add("GuaranteedSpawn");

            return [.. tags];
        }

        public override string DisplayTags(string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return "Txt_RecChars_InvalidTags".Localize();

            int i = 0;
            byte versionNumber = 0;

            if (tags[0].StartsWith("Version_"))
            {
                versionNumber = byte.TryParse(tags[0].Substring(8), out byte newNumber) ? newNumber : byte.MaxValue;
                i++;
            }
            string display = $"<b>{("Txt_RecChars_Version_"+versionNumber).Localize("Txt_RecChars_InvalidVersion".Localize())}</b>";
            if (TagVersion != versionNumber)
                display = "<color=red>" + display + "</color>";
            
            display += $"\n<b>{"Txt_RecChars_Modules".Localize()}</b> ";

            bool addComma = false;
            byte entryCount = 0;

            for (; i < tags.Length; i++)
            {
                if (tags[i] == "GuaranteedSpawn")
                {
                    display = $"<b>{"Conf_RecChars_GuaranteedSpawn".Localize()}</b>\n" + display;
                    continue;
                }

                if (addComma)
                    display += "," + (entryCount % 3 == 0 ? "\n" : " ");

                addComma = true;
                entryCount++;

                display += tags[i].Localize();
            }
            return display;
        }
    }
}