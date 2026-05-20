using BepInEx.Bootstrap;
using MonoMod.Utils;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PineDebug;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    internal static class PineDebugNpcIcons
    {
        private static readonly Dictionary<NPC, Texture2D> icons = [];

        private static bool initialized, pinedebugPresent;
        //private static string pathTemplate;

        private static void InitializeIfNecessary()
        {
            if (!initialized)
            {
                pinedebugPresent = Chainloader.PluginInfos.ContainsKey(RecommendedCharsPlugin.PineDebugGuid);
                //pathTemplate = Path.Combine(AssetLoader.GetModPath(RecommendedCharsPlugin.Plugin), "Textures", "Compat", "PineDebug");
            }
            initialized = true;
        }

        internal static void AddIcon(NPC[] instances, string icon)
        {
            InitializeIfNecessary();
            if (!pinedebugPresent)
                return; // DO NOT RUN IF PINEDEBUG IS NOT PRESENT!!!!

            Texture2D tex = ObjectCreation.AddTextureToAssetManWLegacy("PineDebugTex/"+icon, ["Textures", "Compat", "PineDebug", icon]);
            icons.AddRange(instances.ToDictionary(x => x, x => tex));
        }

        internal static void SetIcons()
            => PineDebugManager.SetIconsForNPCs(icons);
    }
}