using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("2nd Award (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_SecondAward : RecCharsEditorSubModule<Module_SecondAward>
    {
        protected override void Initialized()
        {
            // Load icon asset
            AssetMan.Add("EditorSpr/Npc_SecondAward", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "SecondAward.png"));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_secondaward", ObjMan.Get<SecondAward>("Npc/SecondAward"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_secondaward", AssetMan.Get<Sprite>("EditorSpr/Npc_SecondAward")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_secaward").SetModdedFrame());
        }
    }
}
