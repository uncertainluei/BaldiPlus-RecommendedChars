using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("PSODA (Editor)"), CaudexModulePriority(-120)]
    public sealed class EditorCompat_Item_Psoda : RecCharsEditorSubModule<Module_Item_Psoda>
    {
        protected override void Initialized()
        {
            // Load icon assets            
            AssetMan.Add("EditorSpr/Object_PsodaMachine", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one / 2, 1f, "Textures", "Compat", "LevelStudio", "Object", "PsodaMachine.png"));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_psoda");
            EditorInterface.AddObjectVisualWithMeshCollider("recchars_psodamachine", LevelLoaderPlugin.Instance.basicObjects["recchars_psodamachine"], convex: true);

            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => {
                EditorInterfaceModes.InsertToolInCategory(mode, "items", ObjMan.ContainsKey("Itm/PsodaMini") ? "item_recchars_smallbsoda" : "item_bsoda", new ItemTool("recchars_psoda").SetModdedFrame());
                EditorInterfaceModes.InsertToolInCategory(mode, "objects", "object_bsodamachine", new ObjectTool("recchars_psodamachine", AssetMan.Get<Sprite>("EditorSpr/Object_PsodaMachine")).SetModdedFrame());
            });

            // PSODA Mini
            if (!ObjMan.ContainsKey("Itm/PsodaMini")) return;

            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_smallpsoda");
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_recchars_psoda", new ItemTool("recchars_smallpsoda").SetModdedFrame()));
        }
    }
}
