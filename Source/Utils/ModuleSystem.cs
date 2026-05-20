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
        internal abstract byte IconId { get; }

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

        internal void LegacyTexturesToggle(bool editorInstalled)
        {
            OnLegacyTexturesToggle();
            if (editorInstalled) OnLegacyTexturesToggleEditor();
        }

        protected virtual void OnLegacyTexturesToggle() {}
        protected virtual void OnLegacyTexturesToggleEditor() {}
    }

    public abstract class RecCharsSubModule<T> : RecCharsModule where T : RecCharsModule 
    {
        private bool targetLoaded;

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

    public abstract class ModuleSaveSystem
    {
        public abstract void CoreGameManCreated(CoreGameManager cgm, bool savedGame);
        public abstract void Load(BinaryReader reader);
        public abstract void Save(BinaryWriter writer);
        public abstract void Reset();
    }
}
