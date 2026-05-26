using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public abstract class RecCharsModule : AbstractCaudexModule
    {
        internal virtual byte IconId => 0;

        internal static HashSet<RecCharsModule> allModulesInConfig = [];
        protected static AssetManager ObjMan => RecommendedCharsPlugin.ObjMan;
        protected static AssetManager AssetMan => RecommendedCharsPlugin.AssetMan;
        protected static Harmony Hooks => RecommendedCharsPlugin.Hooks;
        internal static RecommendedCharsPlugin BasePlugin => RecommendedCharsPlugin.Plugin;

        public virtual ModuleSaveSystem SaveSystem => null;

        protected override void Loaded()
        {
            if (Info.ConfigEntry != null)
                allModulesInConfig.Add(this);
        }
    }

    public abstract class RecCharsSubModule<T> : RecCharsModule where T : RecCharsModule 
    {
        protected bool targetLoaded;

        protected override void Loaded()
        {
            base.Loaded();
            targetLoaded = false;
            Type targetType = typeof(T);
            AbstractCaudexModule[] mods = Info.Plugin.GetActiveCaudexModules();
            if (mods.FirstOrDefault(x => x != null && x.GetType() == targetType) == null)
                return;

            targetLoaded = true;
        }

        public bool DependencyExists => targetLoaded;
        public override bool Enabled => base.Enabled && targetLoaded;
    }

    /* Thank God someone figured out the one and only fucking way this actually works.
     * .NET throws a fit if any class with ANYTHING referencing an unavailable assembly gets referenced.
     * .NET developers would rarely write anything actually useful.
     * .NET designers are assholes.
     * If you haven't figured it out yet, it's a copy-pasta.
     */
    public abstract class RecCharsEditorSubModule<T> : RecCharsSubModule<T> where T : RecCharsModule
    {
        protected override void Loaded()
        {
            base.Loaded();
            if (!Chainloader.PluginInfos.ContainsKey(RecommendedCharsPlugin.LevelStudioGuid))
                targetLoaded = false;
        }
    }

    public abstract class ModuleSaveSystem
    {
        public abstract void CoreGameManCreated(CoreGameManager cgm, bool savedGame);
        public abstract void Load(BinaryReader reader);
        public abstract void Save(BinaryWriter writer);
        public abstract void Reset();
    }
}
