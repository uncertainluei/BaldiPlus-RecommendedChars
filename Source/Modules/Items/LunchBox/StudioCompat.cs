using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using PlusLevelStudio.Editor.Tools;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_Item_LunchBox : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Item_LunchBox_Random", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Item", "LunchBox_Random.png"));
            AssetMan.Add("EditorSpr/Item_LunchBox_2", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Item", "LunchBox_2.png"));
            AssetMan.Add("EditorSpr/Item_LunchBox_3", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Item", "LunchBox_3.png"));
            AssetMan.Add("EditorSpr/Item_LunchBox_4", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Item", "LunchBox_4.png"));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "LunchBox.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_lunchbox_random");
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolsToCategory(mode, "items", [
                new ItemTool("recchars_lunchbox_random", AssetMan.Get<Sprite>("EditorSpr/Item_LunchBox_Random"), false),
                new ItemTool("recchars_lunchbox_2", AssetMan.Get<Sprite>("EditorSpr/Item_LunchBox_2"), false),
                new ItemTool("recchars_lunchbox_3", AssetMan.Get<Sprite>("EditorSpr/Item_LunchBox_3"), false),
                new ItemTool("recchars_lunchbox_4", AssetMan.Get<Sprite>("EditorSpr/Item_LunchBox_4"), false)
            ]);
        }
    }
}
