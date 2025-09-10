using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public abstract class RecCharsModule : AbstractCaudexModule
    {
        protected static AssetManager ObjMan => RecommendedCharsPlugin.ObjMan;
        protected static AssetManager AssetMan => RecommendedCharsPlugin.AssetMan;
        protected static Harmony Hooks => RecommendedCharsPlugin.Hooks;
        internal static RecommendedCharsPlugin BasePlugin => RecommendedCharsPlugin.Plugin;

        public virtual ModuleSaveSystem SaveSystem => null;
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
