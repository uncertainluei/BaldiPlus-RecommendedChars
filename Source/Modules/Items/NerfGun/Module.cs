using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusStudioLevelLoader;

using System.Linq;

using UncertainLuei.CaudexLib.Registers;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Nerf Gun"), CaudexModuleSaveTag("Mdl_NerfGun")]
    [CaudexModulePriority(-10)]
    [CaudexModuleConfig("Modules.Items", "NerfGun",
        "A 'toy' water gun that prematurely ends Circle's game.\n(Requires the Circle module to be enabled.)", true)]
    public sealed class Module_Item_NerfGun : RecCharsSubModule<Module_Circle>
    {
        internal override byte IconId => 12;

        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddTexturesToAssetMan("NerfGun/", ["Textures", "Item", "NerfGun"]);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadNerfGun()
        {
            ItemBuilder nerfGunBuilder = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_NerfGun", "Desc_RecChars_NerfGun")
            .SetEnum("RecChars_NerfGun")
            .SetMeta(ItemFlags.MultipleUse, ["adv_normal", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NerfGun/NerfGun_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NerfGun/NerfGun_Large"), 50f))
            .SetShopPrice(300)
            .SetGeneratorCost(30)
            .SetItemComponent<ITM_NerfGun>();

            if (!RecommendedCharsConfig.nerfCircle.Value)
                nerfGunBuilder.SetShopPrice(500).SetGeneratorCost(60);

            ItemObject nerfGun = nerfGunBuilder.BuildAsMulti(2);
            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_nerfgun", nerfGun);
            ObjMan.Add("Itm/NerfGun", nerfGun);

            ObjectCreation.CreatePoster(ObjectCreation.AddTextureToAssetManWLegacy("PstTex/hnt_nerfgun", ["Textures", "Environment", "Poster", "hnt_nerfgun.png"]), "NerfGunHint", "nerfgun_hint");
            CaudexGeneratorEvents.AddAction(CaudexGeneratorEventType.NpcPrep, AddItemsToLevel);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(new WeightedItemObject() { selection = ObjMan.Get<ItemObject>("Itm/NerfGun"), weight = 50 });
            }
        }

        private void AddItemsToLevel(LevelGenerator gen)
        {
            if (!gen.scene) return;
            if (!gen.Ec.npcsToSpawn.FirstOrDefault(x => x != null && x.Character == CircleNpc.charEnum)) return;

            if (gen.scene.GetMeta()?.tags.Contains("endless") == true)
                gen.ld.shopItems = gen.ld.shopItems.AddToArray(new WeightedItemObject() { selection = ObjMan.Get<ItemObject>("Itm/NerfGun"), weight = 50 });

            gen.ld.posters = gen.ld.posters.AddToArray(ObjMan.Get<PosterObject>("Pst/NerfGunHint").Weighted(100));
            gen.ld.potentialItems = gen.ld.potentialItems.AddToArray(ObjMan.Get<ItemObject>("Itm/NerfGun").Weighted(50));
        }
    }
}
