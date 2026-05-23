using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Birthday Cake"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_Item_BaldiCake : RecCharsEditorSubModule<Module_Item_BaldiCake>
    {
        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_baldicake");
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => {
                EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_zesty", new ItemTool("recchars_baldicake").SetModdedFrame());
            });
        }
    }
}
