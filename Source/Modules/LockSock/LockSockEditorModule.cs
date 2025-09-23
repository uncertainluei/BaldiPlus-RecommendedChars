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
    [CaudexModule("Door Lock Sock (Editor)")]
    public sealed class EditorCompat_LockSock : RecCharsEditorSubModule<Module_LockSock>
    {
        protected override void Initialized()
        {
            // Load texture asset
            AssetMan.Add("EditorSpr/Npc_LockSock", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Editor", "npc_locksock.png"));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "LockSock.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_locksock", ObjMan.Get<LockSock>("Npc_LockSock"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_locksock", AssetMan.Get<Sprite>("EditorSpr/Npc_LockSock")));
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_locksock"));
        }
    }
}
