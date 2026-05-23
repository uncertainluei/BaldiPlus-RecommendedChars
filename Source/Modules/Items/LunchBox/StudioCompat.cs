using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Lunch Box (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_Item_LunchBox : RecCharsEditorSubModule<Module_Item_LunchBox>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Item_LunchBox_Random", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Item", "LunchBox_Random.png"));
            AssetMan.Add("EditorSpr/Item_LunchBox_2", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Item", "LunchBox_2.png"));
            AssetMan.Add("EditorSpr/Item_LunchBox_3", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Item", "LunchBox_3.png"));
            AssetMan.Add("EditorSpr/Item_LunchBox_4", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Item", "LunchBox_4.png"));
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
                new ItemTool("recchars_lunchbox_random", AssetMan.Get<Sprite>("EditorSpr/Item_LunchBox_Random"), false).SetModdedFrame(),
                new ExtItemTool("recchars_lunchbox_2", AssetMan.Get<Sprite>("EditorSpr/Item_LunchBox_2"), "Ed_Tool_item_recchars_lunchbox_Desc", false).SetModdedFrame(),
                new ExtItemTool("recchars_lunchbox_3", AssetMan.Get<Sprite>("EditorSpr/Item_LunchBox_3"), "Ed_Tool_item_recchars_lunchbox_Desc", false).SetModdedFrame(),
                new ExtItemTool("recchars_lunchbox_4", AssetMan.Get<Sprite>("EditorSpr/Item_LunchBox_4"), "Ed_Tool_item_recchars_lunchbox_Desc", false).SetModdedFrame()
            ]);
        }
    }
}
