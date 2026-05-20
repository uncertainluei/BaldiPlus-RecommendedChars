using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Util;
using System.Collections.Generic;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_Npc_Circle : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_Circle", ["Textures", "Compat", "LevelStudio", "Npc", "Circle.png"]);
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_Circle_Og", ["Textures", "Compat", "LevelStudio", "Npc", "Circle_Unnerfed.png"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Compat", "LevelStudio", "Circle.json5");
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_circle", ObjMan.Get<CircleNpc>("Npc/Circle_Nerfed"));
            LevelStudioPlugin.Instance.npcDisplays.Add("recchars_circle_og", LevelStudioPlugin.Instance.npcDisplays["recchars_circle"]);
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            List<EditorTool> npcTools = [new ExtNpcTool("recchars_circle", AssetMan.Get<Sprite>("EditorSpr/Npc_Circle"),
                    "Ed_Tool_npc_recchars_circle_Desc").SetModdedFrame()];
            if (vanillaCompliant)
                npcTools.Add(new ExtNpcTool("recchars_circle_og", AssetMan.Get<Sprite>("EditorSpr/Npc_Circle_Og"),
                    "Ed_Tool_npc_recchars_circle_og_Title", "Ed_Tool_npc_recchars_circle_og_Desc").SetModdedFrame());

            EditorInterfaceModes.InsertToolsInCategory(mode, "npcs", "npc_playtime", npcTools);
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_circle").SetModdedFrame());
        }
    }
}
