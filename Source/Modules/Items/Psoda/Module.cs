using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("PSODA"), CaudexModuleSaveTag("Mdl_Psoda")]
    [CaudexModulePriority(-20)]
    [CaudexModuleConfig("Modules.Items", "Psoda",
        "PSODA! It's a BSODA, but it bounces. Yes, I know Extra Content has it.", true)]
    public sealed partial class Module_Item_Psoda : RecCharsSubModule<Module_Item_BsodaMini>
    {
        public override bool Enabled => true; // Use base.Enabled for adding the PSODA Mini variants

        protected override void Initialized()
        {
            // Load texture assets
            AddTexturesToAssetMan("PsodaTex/", ["Textures", "Item", "Psoda"]);
            AssetMan.Add("VendingTex/PsodaMachine", AssetLoader.TextureFromMod(BasePlugin, "Textures", "Environment", "Structure", "PsodaMachine.png"));
            AssetMan.Add("VendingTex/PsodaMachine_Out", AssetLoader.TextureFromMod(BasePlugin, "Textures", "Environment", "Structure", "PsodaMachine_Out.png"));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadPsoda()
        {
            // PSODA
            ItemObject psoda = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_Psoda", "Desc_RecChars_Psoda")
            .SetEnum("RecChars_Psoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "drink", "adv_perfect", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PsodaTex/Psoda_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PsodaTex/Psoda_Large"), 50f))
            .SetShopPrice(600)
            .SetGeneratorCost(75)
            .Build();

            ITM_Psoda psodaSpray = SwapComponentSimple<ITM_BSODA, ITM_Psoda>(GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item, MTM101BaldiDevAPI.prefabTransform));
            psodaSpray.spriteRenderer.sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PsodaTex/Psoda_Spray"), 12f);
            psodaSpray.speed = 40f;
            psodaSpray.time = 60f;
            psodaSpray.moveMod.movementMultiplier = 0.33f;
            psodaSpray.name = "Itm_Psoda";
            psodaSpray.boing = AssetMan.Get<SoundObject>("Sfx/Boing");

            PropagatedAudioManager audMan = psodaSpray.gameObject.AddComponent<PropagatedAudioManager>();
            audMan.audioDevice = psodaSpray.gameObject.AddComponent<AudioSource>();
            audMan.audioDevice.maxDistance = 100f;
            audMan.audioDevice.dopplerLevel = 0f;
            psodaSpray.audMan = audMan;
            psoda.item = psodaSpray;

            ITM_GrapplingHook hook = (ITM_GrapplingHook)ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value.item;
            psodaSpray.entity.collisionLayerMask = hook.entity.collisionLayerMask;
            psodaSpray.layerMask = hook.layerMask;

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_psoda", psoda);
            ObjMan.Add("Itm/Psoda", psoda);

            // PSODA Machine
            SodaMachine sodaMachine = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<SodaMachine>().First(x => x.name == "SodaMachine" && x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform);
            sodaMachine.name = "RecChars PsodaMachine";
            sodaMachine.item = psoda;
            sodaMachine.SetTextures(AssetMan.Get<Texture2D>("VendingTex/PsodaMachine"), AssetMan.Get<Texture2D>("VendingTex/PsodaMachine_Out"));
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_psodamachine", sodaMachine.gameObject);
            ObjMan.Add("Obj/PsodaMachine", sodaMachine);

            // PSODA Mini (if BSODA Mini is enabled)
            if (!base.Enabled) return;

            ItemObject miniPsoda = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_SmallPsoda", "Desc_RecChars_SmallPsoda")
            .SetEnum("RecChars_SmallPsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "drink"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PsodaTex/SmallPsoda_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("PsodaTex/SmallPsoda_Large"), 50f))
            .SetShopPrice(400)
            .SetGeneratorCost(60)
            .Build();

            ITM_BSODA miniPsodaSpray = GameObject.Instantiate(psodaSpray, MTM101BaldiDevAPI.prefabTransform);
            miniPsodaSpray.name = "Itm_SmallPsoda";
            miniPsodaSpray.spriteRenderer.transform.localScale = Vector3.one * 0.625f;
            miniPsodaSpray.speed = 30f;
            miniPsodaSpray.time = 30f;
            miniPsoda.item = miniPsodaSpray;

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_smallpsoda", miniPsoda);
            ObjMan.Add("Itm/PsodaMini", miniPsoda);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int num, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true)
            {
                scene.shopItems = scene.shopItems.AddToArray(ObjMan.Get<ItemObject>("Itm/Psoda").Weighted(25));
                return;
            }
            if (title.StartsWith("F") && num >= 2)
                scene.shopItems = scene.shopItems.AddToArray(ObjMan.Get<ItemObject>("Itm/Psoda").Weighted(5+num*2));
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Psoda", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Psoda", GenerationStageFlags.Addend);

            if (!title.StartsWith("F") || num < 1) return;

            GameObject psodaMachine = ObjMan.Get<SodaMachine>("Obj/PsodaMachine").gameObject;

            List<StructureWithParameters> structures = new(lvl.forcedStructures.Where(x =>
                x.prefab is Structure_EnvironmentObjectPlacer));
            foreach (WeightedStructureWithParameters potential in lvl.potentialStructures)
            {
                if (potential.selection?.prefab is Structure_EnvironmentObjectPlacer)
                    structures.Add(potential.selection);
            }
            foreach (StructureWithParameters strct in structures)
            {
                if (strct.parameters.prefab == null || strct.parameters.prefab.Length == 0) continue;
                WeightedGameObject sodaMachine = strct.parameters.prefab.FirstOrDefault(x =>
                    x.selection?.name == "SodaMachine" &&
                    x.selection?.GetInstanceID() >= 0);
                if (sodaMachine == null) continue;

                strct.parameters.prefab = strct.parameters.prefab.AddToArray(psodaMachine.Weighted(6));
            }
        }

        [CaudexLoadEvent(LoadingEventOrder.Post)]
        private void ModifyRoomAssets()
        {
            // Modify RoomAssets containing vending machines to include PSODA machines if possible
            Transform psodaMachine = ObjMan.Get<SodaMachine>("Obj/PsodaMachine").transform;
            RoomAsset[] roomsWithSodaMachines = Resources.FindObjectsOfTypeAll<RoomAsset>()
                .Where(x => x.basicSwaps?.Count > 0).ToArray();

            foreach (RoomAsset room in roomsWithSodaMachines)
            {
                foreach (BasicObjectSwapData swapData in room.basicSwaps)
                {
                    if (swapData.prefabToSwap == null ||
                        swapData.prefabToSwap.name != "SodaMachine" ||
                        swapData.prefabToSwap.GetInstanceID() < 0)
                        continue;

                    swapData.potentialReplacements = swapData.potentialReplacements.AddToArray(psodaMachine.Weighted(6));
                }
            }
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load icon assets            
            AssetMan.Add("EditorSpr/Object_PsodaMachine", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "PsodaMachine.png"));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.selectableShopItems.AddRange(["recchars_psoda"]);
            EditorInterface.AddObjectVisualWithMeshCollider("recchars_psodamachine", LevelLoaderPlugin.Instance.basicObjects["recchars_psodamachine"], convex: true);

            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => {
                EditorInterfaceModes.InsertToolInCategory(mode, "items", ObjMan.ContainsKey("Itm/PsodaMini") ? "item_recchars_smallbsoda" : "item_bsoda", new ItemTool("recchars_psoda").SetModdedFrame());
                EditorInterfaceModes.InsertToolInCategory(mode, "objects", "object_bsodamachine", new ObjectTool("recchars_psodamachine", AssetMan.Get<Sprite>("EditorSpr/Object_PsodaMachine")).SetModdedFrame());
            });

            // PSODA Mini
            if (!ObjMan.ContainsKey("Itm/PsodaMini")) return;

            LevelStudioPlugin.Instance.selectableShopItems.AddRange(["recchars_smallpsoda"]);
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompliant) => EditorInterfaceModes.InsertToolInCategory(mode, "items", "item_recchars_psoda", new ItemTool("recchars_smallpsoda").SetModdedFrame()));
        }
    }
}