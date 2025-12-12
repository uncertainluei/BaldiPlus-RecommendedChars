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
    [CaudexModule("Pie"), CaudexModuleSaveTag("Mdl_Pie")]
    [CaudexModuleConfig("Modules.Items", "Pie",
        "As seen in Dave's House! Dave couldn't appear here, so you might find others to fill in that gap!", true)]
    public sealed partial class Module_Item_Pie : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture assets
            AddTexturesToAssetMan("PieItm/", ["Textures", "Item", "Pie"]);

            // Load throw sound
            AssetMan.Add("Sfx/PieThrow", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "PieThrow.wav"), "", SoundType.Effect, Color.white, 0f));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadPie()
        {
            ItemObject pie = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_Pie", "Desc_RecChars_Pie")
            .SetEnum("RecChars_Pie")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "recchars:daycare_exempt", "adv_good", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PieItm/Pie_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PieItm/Pie_Large"), 50f))
            .SetShopPrice(450)
            .SetGeneratorCost(55)
            .Build();

            pie.name = "RecChars Pie";

            Gum gumClone = GameObject.Instantiate(((Beans)NPCMetaStorage.Instance.Get(Character.Beans).value).gumPre, MTM101BaldiDevAPI.prefabTransform);

            ITM_Pie pieUse = gumClone.gameObject.AddComponent<ITM_Pie>();
            pie.item = pieUse;
            pie.item.name = "Itm_Pie";

            pieUse.entity = gumClone.entity;
            pieUse.audMan = gumClone.audMan;

            pieUse.audThrow = AssetMan.Get<SoundObject>("Sfx/PieThrow");
            pieUse.audSplat = AssetMan.Get<SoundObject>("Sfx/FoodSplat");

            pieUse.flyingSprite = gumClone.flyingSprite;
            Sprite thrownPieSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PieItm/Pie_Large"), 25f);
            thrownPieSprite.name = "Pie_Thrown";
            pieUse.flyingSprite.GetComponent<SpriteRenderer>().sprite = thrownPieSprite;

            pieUse.splatSprite = gumClone.groundedSprite;
            pieUse.splatSprite.transform.localPosition = Vector3.back * -0.1f;
            pieUse.splatSprite.GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PieItm/PieSplat"), 10f);

            pieUse.noBillboardMat = AssetMan.Get<Material>("NoBillboardMaterial");

            GameObject.DestroyImmediate(gumClone);

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_pie", pie);
            ObjMan.Add("Itm_Pie", pie);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END" || title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(ObjMan.Get<ItemObject>("Itm_Pie").Weighted(50));
            }
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int id, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Pie", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Pie", GenerationStageFlags.Addend);

            if (title == "END" || title.StartsWith("F"))
            {
                lvl.MarkAsNeverUnload();
                lvl.potentialItems = lvl.potentialItems.AddToArray(ObjMan.Get<ItemObject>("Itm_Pie").Weighted(25));
            }
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.Add("recchars_pie");
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("recchars_pie")));
        }
    }
}
