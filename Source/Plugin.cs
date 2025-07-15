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

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [BepInAutoPlugin(ModGuid, ModName), BepInDependency(ApiGuid)]

    [BepInDependency(CrispyPlusGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PineDebugGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CustomMusicsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AnimationsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CharacterRadarGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AdvancedGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LevelLoaderGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(LegacyEditorGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(FragileWindowsGuid, BepInDependency.DependencyFlags.SoftDependency)]

    partial class RecommendedCharsPlugin : BaseUnityPlugin
    {
        internal const string ModGuid = "io.github.uncertainluei.baldiplus.recommendedchars";
        internal const string ModName = "Recommended Characters Pack";

        internal static readonly AssetManager AssetMan = new();
        internal static RecommendedCharsPlugin Plugin { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Plugin = this;
            Log = Logger;

            // Read the config values and remove disabled modules
            RecommendedCharsConfig.BindConfig(Config);
            Modules = Modules.Where(x => x.Enabled).ToArray();

            RecommendedCharsSaveGameIO saveGameSystem = new(Info);
            ModdedSaveGame.AddSaveHandler(saveGameSystem);
            ModdedHighscoreManager.AddModToList(Info, saveGameSystem.GenerateTags());

            // Load localization files
            AssetLoader.LoadLocalizationFolder(Path.Combine(AssetLoader.GetModPath(this), "Lang", "English"), Language.English);

            LoadingEvents.RegisterOnAssetsLoaded(Info, GrabBaseAssets(), LoadingEventOrder.Pre);
            foreach (LoadingEventOrder order in Enum.GetValues(typeof(LoadingEventOrder)))
                LoadingEvents.RegisterOnAssetsLoaded(Info, LoadModules(order), order);

            GeneratorManagement.Register(this, GenerationModType.Addend, GeneratorAddend);
            GeneratorManagement.RegisterFieldTripLootChange(this, FieldTripLootChange);

            Harmony harmony = new(ModGuid);
            harmony.PatchAllConditionals();
        }

        internal Module[] Modules { get; private set; } =
        [
            new Module_Circle(),
            new Module_GottaBully(),
            new Module_ArtsWithWires(),
            new Module_CaAprilFools(),
            new Module_MrDaycare(),
            new Module_Bsodaa(),
        ];

        private IEnumerator GrabBaseAssets()
        {
            yield return 1;
            yield return "Grabbing base assets";
            // Sprite materials
            Material[] materials = [.. Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0)];
            AssetMan.Add("BillboardMaterial", materials.First(x => x.name == "SpriteStandard_Billboard"));
            AssetMan.Add("NoBillboardMaterial", materials.First(x => x.name == "SpriteStandard_NoBillboard"));
        }

        private IEnumerator LoadModules(LoadingEventOrder order)
        {
            if (Modules.Length == 0)
            {
                yield return 1;
                yield return "No modules to load";
                yield break;
            }

            yield return Modules.Length;
            foreach (Module module in Modules)
            {
                yield return $"Loading module \"{module.Name}\"";
                module.RunLoadEvents(order);
            }
        }

        private void GeneratorAddend(string title, int id, SceneObject scene)
        {
            CustomLevelObject[] lvls = scene.GetCustomLevelObjects();

            foreach (Module module in Modules)
            {
                module.FloorAddendAction?.Invoke(title, id, scene);
                foreach (CustomLevelObject lvl in lvls)
                    module.LevelAddendAction?.Invoke(title, id, lvl);
            }
        }

        private void FieldTripLootChange(FieldTrips fieldTrip, FieldTripLoot table)
        {
            foreach (Module module in Modules)
                module.FieldTripLootAction?.Invoke(fieldTrip, table);
        }
    }

    public class RecommendedCharsSaveGameIO(PluginInfo info) : ModdedSaveGameIOBinary
    {
        private readonly PluginInfo info = info;
        public override PluginInfo pluginInfo => info;

        private const byte SaveVersion = 1;
        internal Dictionary<string, object> savedValues;

        public override void OnCGMCreated(CoreGameManager cgm, bool savedGame)
        {
            foreach (Module module in RecommendedCharsPlugin.Plugin.Modules)
                module.SaveSystem?.CoreGameManCreated(cgm, savedGame);
        }

        public override void Load(BinaryReader reader)
        {
            byte ver = reader.ReadByte();
            if (ver == 0) return;
            foreach (Module module in RecommendedCharsPlugin.Plugin.Modules)
                module.SaveSystem?.Load(reader);
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(SaveVersion);
            foreach (Module module in RecommendedCharsPlugin.Plugin.Modules)
                module.SaveSystem?.Save(writer);
        }

        public override void Reset()
        {
            foreach (Module module in RecommendedCharsPlugin.Plugin.Modules)
                module.SaveSystem?.Reset();
        }

        public override string[] GenerateTags()
        {
            List<string> tags = [];
            
            foreach (Module module in RecommendedCharsPlugin.Plugin.Modules)
                tags.Add(module.Name);
            if (tags.Count > 0 && RecommendedCharsConfig.guaranteeSpawnChar.Value)
                tags.Add("GuaranteedSpawn");

            return [.. tags];
        }

        public override string DisplayTags(string[] tags)
        {
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