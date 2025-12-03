using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_Gifter : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Npc_Gifter", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "Gifter.png"));
            AssetMan.Add("EditorSpr/Npc_Gifttanynt", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "Gifter_Giftanny.png"));
            AssetMan.Add("EditorSpr/Object_Gift", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "Gift.png"));
            AssetMan.Add("EditorSpr/Object_GiftBomb", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "Gift_Bomb.png"));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "Loldi.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_gifter", ObjMan.Get<Gifter>("Npc_Gifter"));
            LevelStudioPlugin.Instance.npcDisplays.Add("recchars_gifttanynt", LevelStudioPlugin.Instance.npcDisplays["recchars_gifter"]);

            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolsToCategory(mode, "npcs", [
                //new NPCTool("recchars_gifter", AssetMan.Get<Sprite>("EditorSpr/Npc_Gifter")),
                //new ExtNpcTool("recchars_gifttanynt", AssetMan.Get<Sprite>("EditorSpr/Npc_Gifttanynt"),
                    //"Ed_Tool_npc_recchars_gifttanynt_Title", "Ed_Tool_npc_recchars_gifttanynt_Desc")

                new NPCTool("recchars_gifttanynt", AssetMan.Get<Sprite>("EditorSpr/Npc_Gifter"))
            ]);
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_gifter"));
        }
    }
}
