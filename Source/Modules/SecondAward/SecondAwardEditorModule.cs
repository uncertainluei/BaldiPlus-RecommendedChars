using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat
{
    [CaudexModule("2nd Award (Editor)")]
    public sealed class EditorCompat_SecondAward : RecCharsEditorSubModule<Module_SecondAward>
    {
        protected override void Initialized()
        {
            // Load texture asset
            AssetMan.Add("EditorSpr/Npc_SecondAward", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Editor", "npc_secondaward.png"));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "SecondAward.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_secondaward", ObjMan.Get<SecondAward>("Npc_SecondAward"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_secondaward", AssetMan.Get<Sprite>("EditorSpr/Npc_SecondAward")));
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_secaward"));
        }
    }
}
