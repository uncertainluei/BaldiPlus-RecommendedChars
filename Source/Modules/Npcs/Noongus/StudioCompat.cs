using MTM101BaldAPI.AssetTools;

using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Noongus (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_Noongus : RecCharsEditorSubModule<Module_Noongus>
    {
        protected override void Initialized()
        {
            // Load icon asset
            AssetMan.Add("EditorSpr/Npc_Noongus", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "Noongus.png"));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_noongus", ObjMan.Get<Noongus>("Npc/Noongus"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_noongus", AssetMan.Get<Sprite>("EditorSpr/Npc_Noongus")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_noongus").SetModdedFrame());
        }
    }
}
