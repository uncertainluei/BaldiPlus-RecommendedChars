using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public abstract class Module
    {
        // For quick access
        protected static AssetManager AssetMan => RecommendedCharsPlugin.AssetMan;
        internal static RecommendedCharsPlugin Plugin => RecommendedCharsPlugin.Plugin;
        internal static PluginInfo Info => Plugin.Info;

        public bool Enabled => ConfigEntry != null && ConfigEntry.Value;
        public abstract string Name { get; }
        public virtual string SaveTag => Name;
        protected abstract ConfigEntry<bool> ConfigEntry { get; }

        private MethodInfo[] _methods;
        private static readonly Type _moduleLoadEvent = typeof(ModuleLoadEvent);
        public void RunLoadEvents(LoadingEventOrder order)
        {
            if (_methods == null || _methods.Length == 0)
                _methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (MethodInfo method in _methods)
            {
                if (method.GetParameters().Length > 0)
                    continue;

                foreach (CustomAttributeData data in method.CustomAttributes)
                {
                    if (!_moduleLoadEvent.IsAssignableFrom(data.AttributeType)) continue;
                    List<object> args = [];
                    data.ConstructorArguments.Do(arg =>
                    {
                        args.Add(arg.Value);
                    });

                    ModuleLoadEvent loadEvent = (ModuleLoadEvent)Activator.CreateInstance(data.AttributeType, [.. args]);
                    if (!loadEvent.ShouldRun(order)) continue;

                    try
                    {
                        method.Invoke(this, []);
                    }
                    catch (Exception e)
                    {
                        MTM101BaldiDevAPI.CauseCrash(Info, e.InnerException);
                    }
                    break;
                }
            }
        }

        public virtual Action<string, int, SceneObject> FloorAddendAction => null;
        public virtual Action<string, int, CustomLevelObject> LevelAddendAction => null;
        public virtual Action<FieldTrips, FieldTripLoot> FieldTripLootAction => null;

        public virtual ModuleSaveSystem SaveSystem => null;
    }

    public abstract class ModuleSaveSystem
    {
        public abstract void CoreGameManCreated(CoreGameManager cgm, bool savedGame);
        public abstract void Load(BinaryReader reader);
        public abstract void Save(BinaryWriter writer);
        public abstract void Reset();
    }

    public class ModuleLoadEvent(LoadingEventOrder order) : Attribute
    {
        private readonly LoadingEventOrder orderToExecute = order;
        public virtual bool ShouldRun(LoadingEventOrder order) => orderToExecute == order;
    }

    public class ModuleCompatLoadEvent(string modGuid, LoadingEventOrder order) : ModuleLoadEvent(order)
    {
        private readonly string modGuid = modGuid;
        public override bool ShouldRun(LoadingEventOrder order) => Chainloader.PluginInfos.ContainsKey(modGuid) && base.ShouldRun(order);
    }
}
