using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;


using UnityEngine;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

using PlusStudioLevelLoader;
using System.Linq;
using System.Collections.Generic;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Lunch Box"), CaudexModuleSaveTag("Mdl_LunchBox")]
    [CaudexModuleConfig("Modules", "LunchBox",
        "A rare box that can give you random food items.", true)]
    public sealed class Module_LunchBox : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Item", "LunchBox"), x => "LunchTex/" + x.name);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "LunchBox.json5");

            // Load patches
            Hooks.PatchAll(typeof(LunchBoxPatches));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadLunchBoxItem()
        {
            CaudexMultiItemObject lunchBox = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_LunchBox", "Desc_RecChars_LunchBox")
            .SetEnum("RecChars_LunchBox")
            .SetMeta(ItemFlags.MultipleUse, ["recchars:gifter_blacklist", "adv_forbidden_present"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("LunchTex/LunchBox_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("LunchTex/LunchBox_Large"), 50f))
            .SetShopPrice(550)
            .SetGeneratorCost(20)
            .SetAsNotOverridable()
            .SetItemComponent<ITM_LunchBox>()
            .BuildAsMulti(4);

            ITM_LunchBox.itemEnum = lunchBox.itemType;
            ITM_LunchBox.randomDummyEnum = EnumExtensions.ExtendEnum<Items>("RecChars_LunchBoxRandomDummy");

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_lunchbox_4", lunchBox);
            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_lunchbox_3", lunchBox.nextStage);
            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_lunchbox_2", ((CaudexMultiItemObject)lunchBox.nextStage).nextStage); // WHY ME
            ObjMan.Add("Itm_LunchBox_4", lunchBox);
            ObjMan.Add("Itm_LunchBox_3", lunchBox.nextStage);
            ObjMan.Add("Itm_LunchBox_2", ((CaudexMultiItemObject)lunchBox.nextStage).nextStage);

            ITM_LunchBox.weightedItems =
            [
                lunchBox.Weighted(2),
                lunchBox.nextStage.Weighted(3),
                ((CaudexMultiItemObject)lunchBox.nextStage).nextStage.Weighted(4)
            ];

            ItemObject lunchBoxRandomDummy = ScriptableObject.CreateInstance<ItemObject>();
            lunchBoxRandomDummy.name = "LunchBoxRandomDummy";
            lunchBoxRandomDummy.itemType = ITM_LunchBox.randomDummyEnum;
            lunchBoxRandomDummy.nameKey = lunchBoxRandomDummy.descKey = "Desc_Nothing";
            lunchBoxRandomDummy.itemSpriteLarge = lunchBox.itemSpriteLarge;
            lunchBoxRandomDummy.itemSpriteSmall = lunchBox.itemSpriteSmall;
            lunchBoxRandomDummy.item = ItemMetaStorage.Instance.FindByEnum(Items.Apple).value.item;
            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_lunchbox_random", lunchBoxRandomDummy);
        }

        [CaudexLoadEvent(LoadingEventOrder.Post)]
        private void GetLunchBoxFoodItems()
        {
            ITM_LunchBox lunchBox = (ITM_LunchBox)ObjMan.Get<ItemObject>("Itm_LunchBox_4").item;
            
            ItemMetaData[] itemMetas = ItemMetaStorage.Instance.FindAllWithTags(false, "food")
                .Where(x => !x.flags.HasFlag(ItemFlags.InstantUse) && !x.tags.Contains("recchars:lunchbox_blacklist")).ToArray();
            List<WeightedItemObject> foods = [];

            int maxValue = 0;
            foreach (ItemMetaData itemMeta in itemMetas)
            {
                if (itemMeta.value.value > maxValue)
                    maxValue = itemMeta.value.value;
            }
            foreach (ItemMetaData itemMeta in itemMetas)
                foods.Add(itemMeta.value.Weighted(maxValue*4/itemMeta.value.value)); // For better precision

            lunchBox.possibleFoodItems = foods.ToArray();
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject lvl)
        {
            if (title == "END" || title.StartsWith("F"))
            {
                lvl.shopItems = lvl.shopItems.AddRangeToArray([
                    ObjMan.Get<ItemObject>("Itm_LunchBox_2").Weighted(20),
                    ObjMan.Get<ItemObject>("Itm_LunchBox_3").Weighted(15),
                    ObjMan.Get<ItemObject>("Itm_LunchBox_4").Weighted(10)
                ]);
            }
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/LunchBox", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/LunchBox", GenerationStageFlags.Addend);

            if (title != "END" && !title.StartsWith("F"))
                return;
            
            lvl.potentialItems = lvl.potentialItems.AddRangeToArray([
                ObjMan.Get<ItemObject>("Itm_LunchBox_2").Weighted(4),
                ObjMan.Get<ItemObject>("Itm_LunchBox_3").Weighted(2),
                ObjMan.Get<ItemObject>("Itm_LunchBox_4").Weighted(2),
            ]);            
        }

        // This is done in Finalizer to reduce cafeteria room weights to make space for potential new ones
        [CaudexGenModEvent(GenerationModType.Finalizer)]
        private void FloorFinalizerLvl(string title, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/LunchBox", GenerationStageFlags.Finalizer))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/LunchBox", GenerationStageFlags.Finalizer);

            if (title != "END" && !title.StartsWith("F"))
                return;

            List<WeightedRoomAsset> specialRooms = lvl.potentialSpecialRooms.ToList(),
                cafeterias = [];
            for (int i = 0; i < specialRooms.Count; i++)
            {
                if (specialRooms[i].selection.roomFunctionContainer != null &&
                    specialRooms[i].selection.roomFunctionContainer.name.Contains("Cafeteria"))
                {
                    cafeterias.Add(specialRooms[i]);
                    specialRooms.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
