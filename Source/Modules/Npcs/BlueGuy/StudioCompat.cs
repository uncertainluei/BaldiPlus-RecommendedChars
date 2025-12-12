using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_BlueGuy : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "Loldi.json5");
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_blueguy", ObjMan.Get<BlueGuy>("Npc_BlueGuy"));
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_blueguy", AssetMan.Get<Sprite>("StatusSpr/BlueGuyFog")));
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_blueguy"));
        }
    }
}
