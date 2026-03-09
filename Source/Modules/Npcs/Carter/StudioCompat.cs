using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using MTM101BaldAPI.AssetTools;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_Carter : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load icon asset
            AssetMan.Add("EditorSpr/Npc_Carter", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "Carter.png"));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_carter", ObjMan.Get<Carter>("Npc_Carter"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_carter", AssetMan.Get<Sprite>("EditorSpr/Npc_Carter")));
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_carter"));
        }
    }
}
