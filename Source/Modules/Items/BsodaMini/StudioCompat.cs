using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("BSODA Mini (Editor)"), CaudexModulePriority(-110)]
    public sealed class EditorCompat_Item_BsodaMini : RecCharsEditorSubModule<Module_Item_BsodaMini>
    {
        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.AddRange(["recchars_smallbsoda", "recchars_smalldietbsoda"]);
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => {
                EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_dietbsoda", new ItemTool("recchars_smalldietbsoda").SetModdedFrame());
                EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_bsoda", new ItemTool("recchars_smallbsoda").SetModdedFrame());
            });
        }
    }
}
