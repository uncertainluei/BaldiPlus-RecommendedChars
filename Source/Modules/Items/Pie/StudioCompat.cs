using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Pie (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_Item_Pie : RecCharsEditorSubModule<Module_Item_Pie>
    {
        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_pie");
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("recchars_pie").SetModdedFrame()));
        }
    }
}
