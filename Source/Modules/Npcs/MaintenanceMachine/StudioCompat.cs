using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Evil Maintenance Machine (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_MaintenanceMachine : RecCharsEditorSubModule<Module_MaintenanceMachine>
    {
        protected override void Initialized()
        {
            // Load icon asset
            AssetMan.Add("EditorSpr/Npc_MaintenanceMachine", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "MaintenanceMachine.png"));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_maintmachine", ObjMan.Get<MaintenanceMachine>("Npc/MaintMachine"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_maintmachine", AssetMan.Get<Sprite>("EditorSpr/Npc_MaintenanceMachine")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_maintmachine").SetModdedFrame());
        }
    }
}
