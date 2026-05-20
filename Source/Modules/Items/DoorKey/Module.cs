using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;

using System.Collections.Generic;

using UnityEngine;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("DoorKey"), CaudexModuleSaveTag("Mdl_DoorKey")]
    [CaudexModuleConfig("Modules.Items", "DoorKey",
        "A three-use key that can unlock anything*. (*notebook-locked Daycare doors not included)", true)]
    public sealed partial class Module_Item_DoorKey : RecCharsModule
    {
        internal override byte IconId => 13;

        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddTexturesToAssetMan("DoorKeyItm/", ["Textures", "Item", "DoorKey"]);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadDoorKey()
        {
            ItemObject keyItemObject = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_DoorKey", "Desc_RecChars_DoorKey")
            .SetEnum("RecChars_DoorKey")
            .SetMeta(ItemFlags.MultipleUse, ["key", "crmp_contraband", "presents_lessvalue"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DoorKeyItm/DoorKey_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DoorKeyItm/DoorKey_Large"), 50f))
            .SetShopPrice(600)
            .SetGeneratorCost(80)
            .SetItemComponent<ITM_DoorKey>()
            .BuildAsMulti(3);
            ((ITM_DoorKey)keyItemObject.item).layerMask = ((ITM_Acceptable)ItemMetaStorage.Instance.FindByEnum(Items.DetentionKey).value.item).layerMask;

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_doorkey", keyItemObject);
            ObjMan.Add("Itm/DoorKey", keyItemObject);
        }

        [CaudexLoadEvent(LoadingEventOrder.Post)]
        private void AddAvailableKeyTypes()
        {
            List<Items> keyItems = [Items.DetentionKey];
            foreach (ItemMetaData meta in ItemMetaStorage.Instance.FindAllWithTags(false, "shape_key"))
            {
                if (!keyItems.Contains(meta.id))
                    keyItems.Add(meta.id);
            }
            ITM_DoorKey.keyEnums = [.. keyItems];
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true || (title.StartsWith("F") && id > 0))
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(ObjMan.Get<ItemObject>("Itm/DoorKey").Weighted(25));
            }
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int id, CustomLevelObject lvl)
        {
            if (!title.StartsWith("F") || lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/DoorKey", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/DoorKey", GenerationStageFlags.Addend);

            lvl.MarkAsNeverUnload();
            lvl.potentialItems = lvl.potentialItems.AddToArray(ObjMan.Get<ItemObject>("Itm/DoorKey").Weighted(10));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_doorkey");
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_keys", new ItemTool("recchars_doorkey").SetModdedFrame()));
        }
    }
}
