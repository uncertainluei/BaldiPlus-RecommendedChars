using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;
using PlusStudioLevelLoader;

using System.Linq;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Nerf Gun"), CaudexModuleSaveTag("Mdl_NerfGun")]
    [CaudexModulePriority(-1)]
    [CaudexModuleConfig("Modules.Items", "NerfGun",
        "A 'toy' water gun that prematurely ends Circle's game.", true)]
    public sealed partial class Module_Item_NerfGun : RecCharsSubModule<Module_Npc_Circle>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AddTexturesToAssetMan("NerfGun/", ["Textures", "Item", "NerfGun"]);
            AssetMan.Add("NerfGunPoster/hnt_nerfgun", AssetLoader.TextureFromMod(BasePlugin, "Textures", "Environment", "Poster", "hnt_nerfgun.png"));
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
            ObjMan.Add("Itm_NerfGun", nerfGun);

            PosterObject nerfGunHint = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("NerfGunPoster/hnt_nerfgun"), []);
            nerfGunHint.name = "NerfGunPoster";
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_nerfgun_hint", nerfGunHint);
            ObjMan.Add("Pst_NerfGunHint", nerfGunHint);

            CaudexGeneratorEvents.AddAction(CaudexGeneratorEventType.NpcPrep, AddItemsToLevel);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(new WeightedItemObject() { selection = ObjMan.Get<ItemObject>("Itm_NerfGun"), weight = 50 });
            }
        }

        private void AddItemsToLevel(LevelGenerator gen)
        {
            if (!gen.scene) return;
            if (!gen.Ec.npcsToSpawn.FirstOrDefault(x => x != null && x.Character == CircleNpc.charEnum)) return;

            SceneObjectMetadata meta = SceneObjectMetaStorage.Instance.Get(gen.scene);
            if (meta == null || !meta.tags.Contains("endless") || gen.scene.levelTitle != "END")
            {
                gen.ld.posters = gen.ld.posters.AddToArray(ObjMan.Get<PosterObject>("Pst_NerfGunHint").Weighted(75));
                gen.ld.potentialItems = gen.ld.potentialItems.AddToArray(ObjMan.Get<ItemObject>("Itm_NerfGun").Weighted(25));
                return;
            }

            gen.ld.posters = gen.ld.posters.AddToArray(ObjMan.Get<PosterObject>("Pst_NerfGunHint").Weighted(100));
            gen.ld.potentialItems = gen.ld.potentialItems.AddToArray(ObjMan.Get<ItemObject>("Itm_NerfGun").Weighted(50));
            gen.ld.shopItems = gen.ld.shopItems.AddToArray(new WeightedItemObject() { selection = ObjMan.Get<ItemObject>("Itm_NerfGun"), weight = 50 });
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_nerfgun");
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("recchars_nerfgun"));
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_nerfgun_hint")
            );
        }
    }
}
