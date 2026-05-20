using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;

using UnityEngine;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Birthday Cake"), CaudexModuleSaveTag("Mdl_BaldiCake")]
    [CaudexModuleConfig("Modules.Items", "BaldiCake",
        "Ever wondered what that giant cake tastes like?", true)]
    public sealed partial class Module_Item_BaldiCake : RecCharsModule
    {
        internal override byte IconId => 18;

        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddTexturesToAssetMan("CakeItmTex/", ["Textures", "Item", "BaldiCake"]);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadCake()
        {
            ItemObject cake = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_BaldiCake", "Desc_RecChars_BaldiCake")
            .SetEnum("RecChars_BaldiCake")
            .SetMeta(ItemFlags.Persists, ["food", "recchars:daycare_exempt", "adv_good", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CakeItmTex/BaldiCake_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CakeItmTex/BaldiCake_Large"), 50f))
            .SetShopPrice(400)
            .SetGeneratorCost(45)
            .SetItemComponent<ITM_BaldiCake>()
            .Build();

            ITM_BaldiCake cakeFunction = (ITM_BaldiCake)cake.item;
            cakeFunction.audEat = AssetMan.Get<SoundObject>("Sfx/CartoonEating");
            cakeFunction.gaugeSprite = cake.itemSpriteSmall;

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_baldicake", cake);
            ObjMan.Add("Itm/BaldiCake", cake);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int num, SceneObject scene)
        {
            if (scene.shopItems?.Length > 0)
                scene.shopItems = scene.shopItems.AddToArray(ObjMan.Get<ItemObject>("Itm/BaldiCake").Weighted(RecommendedCharsPlugin.PartyMode ? 60 : 10));
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (!title.StartsWith("F") || lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/BaldiCake", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/BaldiCake", GenerationStageFlags.Addend);
            lvl.potentialItems = lvl.potentialItems.AddToArray(ObjMan.Get<ItemObject>("Itm/BaldiCake").Weighted(RecommendedCharsPlugin.PartyMode ? 65 : 15));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_baldicake");
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => {
                EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_zesty", new ItemTool("recchars_baldicake").SetModdedFrame());
            });
         }
    }
}