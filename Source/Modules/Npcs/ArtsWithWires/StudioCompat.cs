using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_ArtsWithWires : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load icon asset
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_ArtsWithWires", ["Textures", "Compat", "LevelStudio", "Npc", "ArtsWithWires.png"]);
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_artswithwires", ObjMan.Get<ArtsWithWires>("Npc/ArtsWithWires"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_artswithwires", AssetMan.Get<Sprite>("EditorSpr/Npc_ArtsWithWires")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_wires").SetModdedFrame());
        }
    }
}
