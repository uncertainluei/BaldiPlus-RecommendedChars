using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusStudioLevelLoader;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.Advanced;
using BepInEx.Bootstrap;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("BSODA Mini"), CaudexModuleSaveTag("Mdl_BsodaMini")]
    [CaudexModulePriority(-10)]
    [CaudexModuleConfig("Modules.Items", "BsodaMini",
        "BSODA in small cans, given when yield is short!\n(Requires the Eveyone's Bsodaa module to be enabled.)", true)]
    public sealed class Module_Item_BsodaMini : RecCharsSubModule<Module_Bsodaa>
    {
        internal override byte IconId => 15;

        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddTexturesToAssetMan("BsodaMiniTex/", ["Textures", "Item", "BsodaMini"]);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadMiniBsoda()
        {
            // BSODA Mini
            ItemObject miniBsoda = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_SmallBsoda", "Desc_RecChars_SmallBsoda")
            .SetEnum("RecChars_SmallBsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "drink"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaMiniTex/SmallBsoda_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaMiniTex/SmallBsoda_Large"), 50f))
            .SetShopPrice(320)
            .SetGeneratorCost(55)
            .Build();

            miniBsoda.name = "RecChars SmallBsoda";

            ITM_BSODA miniBsodaSpray = GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item, MTM101BaldiDevAPI.prefabTransform);
            miniBsodaSpray.name = "Itm_SmallBsoda";
            miniBsodaSpray.spriteRenderer.transform.localScale = Vector3.one * 0.625f;
            miniBsodaSpray.time = 18f;
            miniBsodaSpray.speed = 26f;
            miniBsodaSpray.gameObject.AddComponent<VanillaBsodaComponent>();

            miniBsoda.item = miniBsodaSpray;

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_smallbsoda", miniBsoda);
            ObjMan.Add("Itm/BsodaMini", miniBsoda);


            // Diet BSODA Mini
            ItemObject miniDietBsoda = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_SmallDietBsoda", "Desc_RecChars_SmallDietBsoda")
            .SetEnum("RecChars_SmallDietBsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "drink"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaMiniTex/SmallDietBsoda_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaMiniTex/SmallDietBsoda_Large"), 50f))
            .SetShopPrice(160)
            .SetGeneratorCost(40)
            .Build();

            miniDietBsoda.name = "RecChars SmallDietBsoda";

            miniBsodaSpray = GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.DietBsoda).value.item, MTM101BaldiDevAPI.prefabTransform);
            miniBsodaSpray.name = "Itm_SmallDietBsoda";
            miniBsodaSpray.spriteRenderer.transform.localScale = Vector3.one * 0.625f;
            miniBsodaSpray.time = 1.8f;
            miniBsodaSpray.speed = 26f;

            miniDietBsoda.item = miniBsodaSpray;

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_smalldietbsoda", miniDietBsoda);
            ObjMan.Add("Itm/DietBsodaMini", miniDietBsoda);

            ObjMan.Get<BsodaaHelper>("Npc/BsodaaHelper").itmSmallBsoda = miniDietBsoda;
            ObjMan.Get<BsodaaHelper>("Npc/BsodaaHelper_Diet").itmSmallBsoda = miniDietBsoda;
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Post)]
        private void AdvancedRecipes()
        {
            ItemObject smallBsoda = ObjMan.Get<ItemObject>("Itm/BsodaMini");
            ItemObject smallDietBsoda = ObjMan.Get<ItemObject>("Itm/DietBsodaMini");

            BepInEx.PluginInfo advInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.AdvancedGuid];
            
            AdvancedCompatHelper.RemoveStoveRecipes(advInfo, (x, y) => x.Length == 1 && x[0].itemType.ToStringExtended() == "IceBoots");
            AdvancedCompatHelper.AddStoveRecipe(Plugin, [ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("IceBoots"), advInfo).value], [smallDietBsoda, smallDietBsoda]);
            AdvancedCompatHelper.AddStoveRecipe(Plugin, [smallBsoda], [smallDietBsoda, smallDietBsoda]);
            AdvancedCompatHelper.AddStoveRecipe(Plugin, [smallDietBsoda, smallDietBsoda], [smallBsoda]);
        }
    }
}