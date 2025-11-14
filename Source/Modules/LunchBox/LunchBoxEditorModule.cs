using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Util;
using PlusLevelStudio.Editor.Tools;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat
{
    [CaudexModule("Lunch Box (Editor)")]
    public sealed class EditorCompat_LunchBox : RecCharsEditorSubModule<Module_LunchBox>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AddTexturesToAssetMan("EditorTex/LunchBox/", ["Textures", "Editor", "LunchBox"]);
            
            AssetMan.Add("EditorSpr/Item_LunchBox_Random", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/LunchBox/item_lunchbox_random"), 1f));
            AssetMan.Add("EditorSpr/Item_LunchBox_2", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/LunchBox/item_lunchbox_2"), 1f));
            AssetMan.Add("EditorSpr/Item_LunchBox_3", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/LunchBox/item_lunchbox_3"), 1f));
            AssetMan.Add("EditorSpr/Item_LunchBox_4", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/LunchBox/item_lunchbox_4"), 1f));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "LunchBox.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
            => EditorInterfaceModes.AddModeCallback(AddContentToMode);

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
