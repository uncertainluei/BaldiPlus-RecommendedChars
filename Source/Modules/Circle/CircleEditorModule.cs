using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat
{
    [CaudexModule("TCMGBiMaE Circle (Editor)")]
    public sealed class EditorCompat_Circle : RecCharsEditorSubModule<Module_Circle>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Editor", "Circle"), x => "EditorTex/Circle/" + x.name);
            
            AssetMan.Add("EditorSpr/Npc_Circle", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Circle/npc_circle"), 1f));
            AssetMan.Add("EditorSpr/Npc_Circle_Og", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Circle/npc_circle_og"), 1f));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "Circle.json5");
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
            EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("recchars_nerfgun"));
            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_circle"),
                new PosterTool("recchars_nerfgun_hint")
            ]);
        }
    }
}
