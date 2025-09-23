using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat
{
    [CaudexModule("LOLdi Exchanges (Editor)")]
    public sealed class EditorCompat_Loldi : RecCharsEditorSubModule<Module_Loldi>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Editor", "Loldi"), x => "EditorTex/Loldi/" + x.name);
            
            AssetMan.Add("EditorSpr/Npc_Gifter", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Loldi/npc_gifter"), 1f));
            AssetMan.Add("EditorSpr/Npc_Gifttanynt", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Loldi/npc_gifttanynt"), 1f));

            AssetMan.Add("EditorSpr/Object_Gift", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Loldi/object_gift"), 1f));
            AssetMan.Add("EditorSpr/Object_GiftBomb", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Loldi/object_giftbomb"), 1f));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "Loldi.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_blueguy", ObjMan.Get<BlueGuy>("Npc_BlueGuy"));
            EditorInterface.AddNPCVisual("recchars_gifter", ObjMan.Get<Gifter>("Npc_Gifter"));
            LevelStudioPlugin.Instance.npcDisplays.Add("recchars_gifttanynt", LevelStudioPlugin.Instance.npcDisplays["recchars_gifter"]);

            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolsToCategory(mode, "npcs", [
                new NPCTool("recchars_blueguy", AssetMan.Get<Sprite>("StatusSpr/BlueGuyFog")),
                new NPCTool("recchars_gifter", AssetMan.Get<Sprite>("EditorSpr/Npc_Gifter")),
                new ExtNpcTool("recchars_gifttanynt", AssetMan.Get<Sprite>("EditorSpr/Npc_Gifttanynt"),
                    "Ed_Tool_npc_recchars_gifttanynt_Title", "Ed_Tool_npc_recchars_gifttanynt_Desc")
            ]);
            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_blueguy"),
                new PosterTool("recchars_pri_gifter")
            ]);
        }
    }
}
