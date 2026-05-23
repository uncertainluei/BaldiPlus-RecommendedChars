using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Door Key (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_Item_DoorKey : RecCharsEditorSubModule<Module_Item_DoorKey>
    {
        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_doorkey");
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_keys", new ItemTool("recchars_doorkey").SetModdedFrame()));
        }
    }
}
