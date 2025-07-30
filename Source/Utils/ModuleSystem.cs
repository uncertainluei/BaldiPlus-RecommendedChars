using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using System;
using System.IO;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public abstract class RecCharsModule : AbstractCaudexModule
    {
        protected static AssetManager AssetMan => RecommendedCharsPlugin.AssetMan;
        protected static Harmony Hooks => RecommendedCharsPlugin.Hooks;
        internal static RecommendedCharsPlugin BasePlugin => RecommendedCharsPlugin.Plugin;

        public virtual ModuleSaveSystem SaveSystem => null;
    }

    public abstract class ModuleSaveSystem
    {
        public abstract void CoreGameManCreated(CoreGameManager cgm, bool savedGame);
        public abstract void Load(BinaryReader reader);
        public abstract void Save(BinaryWriter writer);
        public abstract void Reset();
    }
}
