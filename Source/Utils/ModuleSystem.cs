using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public abstract class RecCharsModule : AbstractCaudexModule
    {
        protected static AssetManager ObjMan => RecommendedCharsPlugin.ObjMan;
        protected static AssetManager AssetMan => RecommendedCharsPlugin.AssetMan;
        protected static Harmony Hooks => RecommendedCharsPlugin.Hooks;
        internal static RecommendedCharsPlugin BasePlugin => RecommendedCharsPlugin.Plugin;

        public virtual ModuleSaveSystem SaveSystem => null;

        protected PosterObject CreatePoster(string path, string name)
        {
            PosterObject poster = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>(path), []);
            poster.name = name;
            return poster;
        }

        protected void AddTexturesToAssetMan(string prefix, string[] paths)
            => AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", paths), x => prefix+x.name);

        protected void AddAudioToAssetMan(string prefix, string[] paths)
        {
            string[] files = Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(BasePlugin), Path.Combine(paths)), "*.wav");
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip aud = AssetLoader.AudioClipFromFile(files[i], AudioType.WAV);
                AssetMan.Add(prefix + aud.name, aud);
                aud.name = "RecChars_" + aud.name;
            }
        }

        protected WeightedRoomAsset[] RoomAssetsFromDirectory(string dir, params int[] weights)
            => RoomAssetsFromDirectory(null, dir, weights);

        protected WeightedRoomAsset[] RoomAssetsFromDirectory(string dir)
            => RoomAssetsFromDirectory(dir, 100);

        protected WeightedRoomAsset[] RoomAssetsFromDirectory(CaudexRoomBlueprint blueprint, string dir)
            => RoomAssetsFromDirectory(blueprint, dir, 100);

        protected WeightedRoomAsset[] RoomAssetsFromDirectory(CaudexRoomBlueprint blueprint, string dir, params int[] weights)
        {
            if (weights == null || weights.Length == 0)
                weights = [100];

            int idx = 0;
            List<WeightedRoomAsset> rooms = [];

            foreach (string file in Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(BasePlugin), "Layouts", dir), "*.rbpl"))
            {
                BinaryReader reader = new(File.OpenRead(file));
                BaldiRoomAsset formatAsset = BaldiRoomAsset.Read(reader);
                reader.Close();

                ExtendedRoomAsset asset = LevelImporter.CreateRoomAsset(formatAsset);
                if (blueprint != null)
                {
                    asset.roomFunctionContainer = blueprint.functionContainer;
                    asset.lightPre = blueprint.lightObj;
                    asset.windowObject = blueprint.windowSet;
                    asset.windowChance = blueprint.windowChance;
                    asset.posters = blueprint.posters;
                    asset.posterChance = blueprint.posterChance;
                    asset.mapMaterial = blueprint.mapMaterial;
                    asset.basicSwaps = blueprint.objectSwaps;
                    asset.name = blueprint.name+"_"+Path.GetFileNameWithoutExtension(file);
                }
                else
                    asset.name = asset.category.ToStringExtended()+"_"+Path.GetFileNameWithoutExtension(file);

                ((ScriptableObject)asset).name = asset.type.ToString()+"_"+asset.name;
                rooms.Add(asset.Weighted(weights[Math.Min(idx,weights.Length-1)]));
                idx++;
            }
            return rooms.ToArray();
        }

        protected X SwapComponentSimple<T, X>(T original) where T : MonoBehaviour where X : T
        {
            X val = original.gameObject.AddComponent<X>();

            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo fieldInfo in fields)
                fieldInfo.SetValue(val, fieldInfo.GetValue(original));

            GameObject.DestroyImmediate(original);
            return val;
        }
    }

    public abstract class RecCharsEditorSubModule<T> : RecCharsModule where T : RecCharsModule 
    {
        private bool targetLoaded;

        protected override void Loaded()
        {
            targetLoaded = false;
            if (!Chainloader.PluginInfos.ContainsKey(RecommendedCharsPlugin.LevelStudioGuid))
                return;

            Type targetType = typeof(T);
            AbstractCaudexModule[] mods = Info.Plugin.GetActiveCaudexModules();
            if (mods.FirstOrDefault(x => x != null && x.GetType() == targetType) == null)
                return;

            targetLoaded = true;
        }

        public override bool Enabled => targetLoaded;
    }

    public abstract class ModuleSaveSystem
    {
        public abstract void CoreGameManCreated(CoreGameManager cgm, bool savedGame);
        public abstract void Load(BinaryReader reader);
        public abstract void Save(BinaryWriter writer);
        public abstract void Reset();
    }
}
