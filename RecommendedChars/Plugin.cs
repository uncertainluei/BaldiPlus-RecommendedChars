using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
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
using System.Reflection;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(AnimationsGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CrispyPlusGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ApiGuid)]
    class RecommendedCharsPlugin : BaseUnityPlugin
    {
        public const string ModName = "Recommended Character Pack";
        public const string ModGuid = "io.github.uncertainluei.baldiplus.recommendedchars";
        public const string ModVersion = "1.2";

        internal const string ApiGuid = "mtm101.rulerp.bbplus.baldidevapi";
        internal const string AnimationsGuid = "pixelguy.pixelmodding.baldiplus.newanimations";
        internal const string CrispyPlusGuid = "mtm101.rulerp.baldiplus.crispyplus";

        public static readonly AssetManager AssetMan = new AssetManager();
        internal static RecommendedCharsPlugin Plugin { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        internal static bool AnimationsCompat { get; private set; }

        private void Awake()
        {
            Plugin = this;
            Log = Logger;

            // Read the config values and remove disabled modules
            RecommendedCharsConfig.BindConfig(Config);
            Modules = Modules.Where(x => x.Enabled).ToArray();

            RecommendedCharsSaveGameIO saveGameSystem = new RecommendedCharsSaveGameIO(Info);
            ModdedSaveGame.AddSaveHandler(saveGameSystem);
            ModdedHighscoreManager.AddModToList(Info, saveGameSystem.GenerateTags());

            // Load localization files
            AssetLoader.LoadLocalizationFolder(Path.Combine(AssetLoader.GetModPath(this), "Lang", "English"), Language.English);

            AnimationsCompat = Chainloader.PluginInfos.ContainsKey(AnimationsGuid);

            LoadingEvents.RegisterOnAssetsLoaded(Info, LoadAssets(), false);
            LoadingEvents.RegisterOnAssetsLoaded(Info, PostLoadModules(), true);
            GeneratorManagement.Register(this, GenerationModType.Addend, GeneratorAddend);
            GeneratorManagement.RegisterFieldTripLootChange(this, FieldTripLootChange);

            Harmony harmony = new Harmony(ModGuid);
            harmony.PatchAllConditionals();
        }

        public Module[] Modules { get; private set; } = new Module[]
        {
            new Module_Circle(),
            new Module_GottaBully(),
            new Module_ArtsWithWires(),
            new Module_CaAprilFools(),
            new Module_MrDaycare(),
            new Module_Bsodaa(),
        };

        // Like AssetLoader.SpritesFromSpritesheet, but based on sprite size and sprite count
        internal static Sprite[] SplitSpriteSheet(Texture2D atlas, int spriteWidth, int spriteHeight, int totalSprites = 0, float pixelsPerUnit = 100f)
        {
            int horizontalTiles = atlas.width / spriteWidth;
            int verticalTiles = atlas.height / spriteHeight;

            if (totalSprites == 0)
                totalSprites = horizontalTiles * verticalTiles;
            Sprite[] array = new Sprite[totalSprites];

            Vector2 center = Vector2.one / 2f;

            int i = 0;
            for (int y = verticalTiles - 1; y >= 0; y--)
            {
                for (int x = 0; x < horizontalTiles && i < totalSprites; x++)
                {
                    Sprite sprite = Sprite.Create(atlas, new Rect(x * spriteWidth, y * spriteHeight, spriteWidth, spriteHeight), center, pixelsPerUnit, 0u, SpriteMeshType.FullRect);
                    sprite.name = atlas.name + "_"+i;
                    array[i++] = sprite;
                }
            }
            return array;
        }

        internal static X CloneComponent<T, X>(T original) where T : MonoBehaviour where X : T
        {
            X val = original.gameObject.AddComponent<X>();
            
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToArray();
            foreach (FieldInfo fieldInfo in fields)
                fieldInfo.SetValue(val, fieldInfo.GetValue(original));

            GameObject.DestroyImmediate(original);
            return val;
        }

        internal static void AddAudioClipsToAssetMan(string path, string prefix)
        {
            string[] files = Directory.GetFiles(path, "*.wav");
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip aud = AssetLoader.AudioClipFromFile(files[i], AudioType.WAV);
                AssetMan.Add(prefix + aud.name, aud);
                aud.name = "RecChars_" + aud.name;
            }
        }

        private IEnumerator LoadAssets()
        {
            // Sprite materials
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0).ToArray();
            AssetMan.Add("BillboardMaterial", materials.First(x => x.name == "SpriteStandard_Billboard"));
            AssetMan.Add("NoBillboardMaterial", materials.First(x => x.name == "SpriteStandard_NoBillboard"));

            if (Modules.Length == 0)
            {
                yield return 1;
                yield return "No module loads found";
                yield break;
            }
            yield return Modules.Length;

            foreach (Module module in Modules)
            { 
                yield return $"Loading module \"{module.Name}\"";
                module.LoadAction?.Invoke();
            }
        }

        private IEnumerator PostLoadModules()
        {
            Module[] postModules = Modules.Where(x => x.PostLoadAction != null).ToArray();
            if (postModules.Length == 0)
            {
                yield return 1;
                yield return "No module post-loads found";
                yield break;
            }

            yield return postModules.Length;

            foreach (Module module in postModules)
            {
                yield return $"Post-loading module \"{module.Name}\"";
                module.PostLoadAction.Invoke();
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

    public abstract class Module
    {
        // These three are here for convenience sake
        protected static AssetManager AssetMan => RecommendedCharsPlugin.AssetMan;
        internal static RecommendedCharsPlugin Plugin => RecommendedCharsPlugin.Plugin;
        internal static PluginInfo Info => Plugin.Info;

        public bool Enabled => ConfigEntry != null && ConfigEntry.Value;
        public abstract string Name { get; }
        public virtual string SaveTag => Name;
        protected abstract ConfigEntry<bool> ConfigEntry { get; }

        public virtual Action LoadAction => null;
        public virtual Action PostLoadAction => null;
        public virtual Action<string, int, SceneObject> FloorAddendAction => null;
        public virtual Action<string, int, CustomLevelObject> LevelAddendAction => null;
        public virtual Action<FieldTrips, FieldTripLoot> FieldTripLootAction => null;
    }

    public class RecommendedCharsSaveGameIO : ModdedSaveGameIOBinary
    {
        public RecommendedCharsSaveGameIO(PluginInfo info)
        {
            this.info = info;
        }

        private readonly PluginInfo info;
        public override PluginInfo pluginInfo => info;

        public override void Load(BinaryReader reader)
        {
            reader.ReadByte();
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write((byte)0);
        }

        public override void Reset()
        {
        }

        public override string[] GenerateTags()
        {
            List<string> tags = new List<string>();
            
            foreach (Module module in RecommendedCharsPlugin.Plugin.Modules)
                tags.Add(module.Name);
            if (tags.Count > 0)
            {
                if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
                    tags.Add("GuaranteedSpawn");
                if ( RecommendedCharsConfig.intendedWiresBehavior.Value)
                    tags.Add("GuaranteedSpawn");
            }

            return tags.ToArray();
        }

        public override string DisplayTags(string[] tags)
        {
            string display = "<b>Modules:</b> ";
            bool addComma = false;

            foreach (string tag in tags)
            {
                if (tag == "GuaranteedSpawn")
                {
                    display = "<b>Guaranteed Character Spawns</b>\n" + display;
                    break;
                }

                if (addComma)
                    display += ", ";
                addComma = true;

                display += tag;
            }
            return display;
        }
    }
}