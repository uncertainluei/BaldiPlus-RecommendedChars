using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Nerf Gun (Editor)"), CaudexModulePriority(-110)]
    public sealed class EditorCompat_Item_NerfGun : RecCharsEditorSubModule<Module_Item_NerfGun>
    {
        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_nerfgun");
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_scissors", new ItemTool("recchars_nerfgun").SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_nerfgun_hint").SetModdedFrame()
            );
        }
    }
}
