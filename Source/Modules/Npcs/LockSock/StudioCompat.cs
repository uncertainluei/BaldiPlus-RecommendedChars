using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Locks and Bolts (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_LockSock : RecCharsEditorSubModule<Module_LockSock>
    {
        protected override void Initialized()
        {
            // Load icon asset
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_LockSock", ["Textures", "Compat", "LevelStudio", "Npc", "LockSock.png"]);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_locksock", ObjMan.Get<LockSock>("Npc/LockSock"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_locksock", AssetMan.Get<Sprite>("EditorSpr/Npc_LockSock")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_locksock").SetModdedFrame());
        }
    }
}
