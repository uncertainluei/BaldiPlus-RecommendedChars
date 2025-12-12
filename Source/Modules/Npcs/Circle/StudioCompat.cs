using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_Circle : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Npc_Circle", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "Circle.png"));
            AssetMan.Add("EditorSpr/Npc_Circle_Og", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "Circle_Unnerfed.png"));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Compat", "LevelStudio", "Circle.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_circle", ObjMan.Get<CircleNpc>("Npc_Circle_Nerfed"));
            LevelStudioPlugin.Instance.npcDisplays.Add("recchars_circle_og", LevelStudioPlugin.Instance.npcDisplays["recchars_circle"]);
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolsToCategory(mode, "npcs", [
                new ExtNpcTool("recchars_circle", AssetMan.Get<Sprite>("EditorSpr/Npc_Circle"),
                    "Ed_Tool_npc_recchars_circle_Desc"),
                new ExtNpcTool("recchars_circle_og", AssetMan.Get<Sprite>("EditorSpr/Npc_Circle_Og"),
                    "Ed_Tool_npc_recchars_circle_og_Title", "Ed_Tool_npc_recchars_circle_og_Desc")
            ]);
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_circle"));
        }
    }
}
