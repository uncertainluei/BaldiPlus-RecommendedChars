using BepInEx.Configuration;

using HarmonyLib;

using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI;

using BaldiLevelEditor;
using PlusLevelLoader;

using System.Collections.Generic;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LegacyEditor;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;
using BaldisBasicsPlusAdvanced.API;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("CA April Fools"), CaudexModuleSaveTag("Mdl_CaAprilFools")]
    [CaudexModuleConfig("Modules", "CaAprilFools",
        "Adds a few features based on the April Fools updates from the Chaos Awakens Minecraft mod.", true)]
    public sealed class Module_CaAprilFools : RecCharsModule
    {

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Item", "CaAprilFools"), x => "CAItems/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Npc", "MMCoin"), x => "MMCoinTex/" + x.name);

            AssetMan.Add("Boing", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "Boing.wav"), "Sfx_RecChars_CherryBsodaBoing", SoundType.Effect, Color.white));

            LoadItems();
            LoadManMemeCoinNpc();
            ManMemeCoinEvents.InitializeBaseEvents();

            LevelGeneratorEventPatch.OnNpcAdd += TrySpawnManMemeCoin;
        }

        private void LoadItems()
        {
            // Flamin' Hot Cheepers
            ItemObject puffs = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_FlaminPuffs", "Desc_RecChars_FlaminPuffs")
            .SetEnum("RecChars_FlaminPuffs")
            .SetMeta(ItemFlags.Persists, ["food", "recchars_daycare_exempt", "cann_hate", "adv_perfect", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/FlaminPuffs_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/FlaminPuffs_Large"), 50f))
            .SetShopPrice(800)
            .SetGeneratorCost(85)
            .SetItemComponent<ITM_FlaminPuffs>()
            .Build();

            puffs.name = "RecChars FlaminPuffs";

            SoundObject audEat = ((ITM_ZestyBar)ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value.item).audEat;

            ITM_FlaminPuffs puffsItm = (ITM_FlaminPuffs)puffs.item;
            puffsItm.name = "Itm_FlaminPuffs";
            puffsItm.gaugeSprite = puffs.itemSpriteSmall;
            puffsItm.audEat = audEat;

            AssetMan.Add("FlaminPuffsItem", puffs);


            // Cherry BSODA
            ItemObject cherryBsoda = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_CherryBsoda", "Desc_RecChars_CherryBsoda")
            .SetEnum("RecChars_CherryBsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "drink", "adv_perfect", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/CherryBsoda_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/CherryBsoda_Large"), 50f))
            .SetShopPrice(600)
            .SetGeneratorCost(75)
            .Build();

            cherryBsoda.name = "RecChars CherryBsoda";

            ITM_BSODA bsodaClone = GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item, MTM101BaldiDevAPI.prefabTransform);
            bsodaClone.enabled = false;
            bsodaClone.spriteRenderer.sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/CherryBsoda_Spray"), 12f);
            bsodaClone.speed = 40f;
            bsodaClone.time = 15f;
            bsodaClone.moveMod.movementMultiplier = 0.33f;

            ITM_CherryBsoda cherryBsodaUse = bsodaClone.gameObject.AddComponent<ITM_CherryBsoda>();

            cherryBsoda.item = cherryBsodaUse;
            cherryBsoda.item.name = "Itm_CherryBsoda";

            cherryBsodaUse.bsoda = bsodaClone;
            cherryBsodaUse.boing = AssetMan.Get<SoundObject>("Boing");

            ITM_GrapplingHook hook = (ITM_GrapplingHook)ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value.item;
            bsodaClone.entity.collisionLayerMask = hook.entity.collisionLayerMask;
            cherryBsodaUse.layerMask = hook.layerMask;

            AssetMan.Add("CherryBsodaItem", cherryBsoda);


            // Cherry BSODA Machine
            SodaMachine sodaMachine = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<SodaMachine>().First(x => x.name == "SodaMachine" && x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform);
            sodaMachine.name = "RecChars CherrySodaMachine";
            sodaMachine.item = cherryBsoda;

            Renderer renderer = sodaMachine.meshRenderer;

            renderer.sharedMaterials =
            [
                renderer.sharedMaterials[0],
                new(renderer.sharedMaterials[1])
                {
                    name = "CherryBsodaMachine",
                    mainTexture = AssetMan.Get<Texture2D>("CAItems/CherryBsodaMachine")
                }
            ];
            sodaMachine.outOfStockMat = new(sodaMachine.outOfStockMat)
            {
                name = "CherryBsodaMachine_Out",
                mainTexture = AssetMan.Get<Texture2D>("CAItems/CherryBsodaMachine_Out")
            };

            AssetMan.Add("CherrySodaMachine", sodaMachine);


            // Ultimate Apple
            ItemObject ultiApple = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_UltimateApple", "Desc_RecChars_UltimateApple")
            .SetEnum("RecChars_UltimateApple")
            .SetMeta(ItemFlags.NoUses, ["food", "crmp_contraband", "adv_forbidden_present"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/UltimateApple_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/UltimateApple_Large"), 50f))
            .SetShopPrice(2500)
            .SetGeneratorCost(100)
            .SetItemComponent(ItemMetaStorage.Instance.FindByEnum(Items.Apple).value.item)
            .Build();

            ultiApple.name = "RecChars UltimateApple";

            Baldi_UltimateApple.ultiAppleEnum = ultiApple.itemType;
            Baldi_UltimateApple.ultiAppleSprites = AssetLoader.SpritesFromSpritesheet(2, 1, 32f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("CAItems/BaldiUltimateApple"));

            AssetMan.Add("UltimateAppleItem", ultiApple);


            // Can of Mangles
            ItemObject manglesItemObject = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_Mangles", "Desc_RecChars_Mangles")
            .SetEnum("RecChars_Mangles")
            // The Mangles would have this "homemade" flavor, thus you can feed that to Cann
            .SetMeta(ItemFlags.MultipleUse, ["food", "recchars_daycare_exempt", "adv_good", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/Mangles_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/Mangles_Large"), 50f))
            .SetShopPrice(500)
            .SetGeneratorCost(60)
            .SetItemComponent<ITM_Mangles>()
            .BuildAsMulti(3);
            ((ITM_Mangles)manglesItemObject.item).audEat = audEat;

            AssetMan.Add("ManglesItem", manglesItemObject);
        }

        private void LoadManMemeCoinNpc()
        {
            ManMemeCoin coin = new NPCBuilder<ManMemeCoin>(Plugin)
                .SetName("ManMemeCoin")
                .SetEnum("RecChars_ManMemeCoin")
                .SetPoster(AssetMan.Get<Texture2D>("MMCoinTex/pri_manmeme"), "PST_PRI_RecChars_ManMeme1", "PST_PRI_RecChars_ManMeme2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(["no_balloon_frenzy", "adv_exclusion_hammer_immunity", "adv_ev_cold_school_immunity"])
                .SetAirborne()
                .AddLooker()
                .AddTrigger()
                .Build();

            coin.looker.ignorePlayerVisibility = true;

            Sprite[] sprites = CaudexAssetLoader.SplitSpriteSheet(AssetMan.Get<Texture2D>("MMCoinTex/ManMemeCoin"), 128, 128, 6, 25f);

            coin.spriteRenderer[0].transform.localPosition = Vector3.zero;
            coin.spriteRenderer[0].gameObject.AddComponent<PickupBob>();
            coin.spriteRenderer[0].sprite = sprites[0];

            PineDebugNpcIconPatch.icons.Add(coin.Character, AssetMan.Get<Texture2D>("MMCoinTex/BorderMMCoin"));
            CharacterRadarColorPatch.colors.Add(coin.Character, new(206/255f, 165/255f, 66/255f));

            coin.Navigator.avoidRooms = true;
            coin.Navigator.Entity.SetHeight(6f);
            coin.normalLayer = LayerMask.NameToLayer("ClickableEntities");
            coin.Navigator.Entity.defaultLayer = coin.normalLayer;
            coin.gameObject.layer = coin.normalLayer;

            ManMemeCoin.animation = new Dictionary<string, Sprite[]> { { "Spin", sprites } };
            coin.animator = coin.gameObject.AddComponent<CustomSpriteAnimator>();
            coin.animator.spriteRenderer = coin.spriteRenderer[0];

            LookAtGuy theTest = (LookAtGuy)NPCMetaStorage.Instance.Get(Character.LookAt).value;
            coin.explosionPre = theTest.explosionPrefab;
            coin.explosionSound = theTest.audExplode;

            AssetMan.Add("ManMemeCoinNpc", coin);
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelLoaderGuid, LoadingEventOrder.Pre)]
        private void RegisterToLevelLoader()
        {
            PlusLevelLoaderPlugin.Instance.npcAliases.Add("recchars_manmemecoin", AssetMan.Get<ManMemeCoin>("ManMemeCoinNpc"));

            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_flaminpuffs", AssetMan.Get<ItemObject>("FlaminPuffsItem"));
            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_cherrybsoda", AssetMan.Get<ItemObject>("CherryBsodaItem"));
            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_ultimateapple", AssetMan.Get<ItemObject>("UltimateAppleItem"));
            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_mangles",  AssetMan.Get<ItemObject>("ManglesItem"));

            PlusLevelLoaderPlugin.Instance.prefabAliases.Add("recchars_cherrysodamachine", AssetMan.Get<SodaMachine>("CherrySodaMachine").gameObject);
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LegacyEditorGuid, LoadingEventOrder.Pre)]
        private void RegisterToLegacyEditor()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Editor", "CaAprilFools"), x => "CAEditor/" + x.name);

            LegacyEditorCompatHelper.AddCharacterObject("recchars_manmemecoin", AssetMan.Get<ManMemeCoin>("ManMemeCoinNpc"));

            BaldiLevelEditorPlugin.itemObjects.Add("recchars_flaminpuffs", AssetMan.Get<ItemObject>("FlaminPuffsItem"));
            BaldiLevelEditorPlugin.itemObjects.Add("recchars_cherrybsoda", AssetMan.Get<ItemObject>("CherryBsodaItem"));
            BaldiLevelEditorPlugin.itemObjects.Add("recchars_ultimateapple", AssetMan.Get<ItemObject>("UltimateAppleItem"));
            BaldiLevelEditorPlugin.itemObjects.Add("recchars_mangles", AssetMan.Get<ItemObject>("ManglesItem"));

            LegacyEditorCompatHelper.AddObject("recchars_cherrysodamachine", AssetMan.Get<SodaMachine>("CherrySodaMachine"));

            new ExtNpcTool("recchars_manmemecoin", "CAEditor/Npc_manmemecoin").AddToEditor("characters");

            new ExtItemTool("recchars_flaminpuffs", "CAEditor/Itm_flaminpuffs").AddToEditor("items");
            new ExtItemTool("recchars_cherrybsoda", "CAEditor/Itm_cherrybsoda").AddToEditor("items");
            new ExtItemTool("recchars_ultimateapple", "CAEditor/Itm_ultimateapple").AddToEditor("items");
            new ExtItemTool("recchars_mangles", "CAEditor/Itm_mangles").AddToEditor("items");

            new ExtRotatableTool("recchars_cherrysodamachine", "CAEditor/Object_cherrysodamachine").AddToEditor("objects");
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Pre)]
        private void AdvancedCompat()
        {
            ItemObject flaminPuffs = AssetMan.Get<ItemObject>("FlaminPuffsItem");
            flaminPuffs.item = RecommendedCharsPlugin.SwapComponentSimple<ITM_FlaminPuffs, ITM_FlaminPuffs_AdvancedCompat>((ITM_FlaminPuffs)flaminPuffs.item);

            ApiManager.AddNewSymbolMachineWords(Plugin, "Monk", "Tone", "Chaos", "Meme", "Pear", "Weird", "Zeed", "Roy", "Oreo");
            ApiManager.AddNewTips(Plugin, "Adv_Elv_Tip_RecChars_ManMemeSpawning", "Adv_Elv_Tip_RecChars_ManMemeCoin",
                "Adv_Elv_Tip_RecChars_FlaminPuffsWarmth", "Adv_Elv_Tip_RecChars_FlaminPuffsWindows",
                "Adv_Elv_Tip_RecChars_CherryBsoda", "Adv_Elv_Tip_RecChars_UltimateApple");
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<ManMemeCoin>("ManMemeCoinNpc"));
                scene.shopItems = scene.shopItems.AddRangeToArray(
                [
                    AssetMan.Get<ItemObject>("ManglesItem").Weighted(40),
                    AssetMan.Get<ItemObject>("CherryBsodaItem").Weighted(25),
                    AssetMan.Get<ItemObject>("FlaminPuffsItem").Weighted(20),
                    AssetMan.Get<ItemObject>("UltimateAppleItem").Weighted(5)
                ]);
                return;
            }
            if (title.StartsWith("F"))
            {
                if (id >= 2)
                {
                    scene.shopItems = scene.shopItems.AddRangeToArray(
                    [
                        AssetMan.Get<ItemObject>("ManglesItem").Weighted(6+id*2),
                        AssetMan.Get<ItemObject>("CherryBsodaItem").Weighted(5+id*2),
                        AssetMan.Get<ItemObject>("FlaminPuffsItem").Weighted(4+id*2)
                    ]);
                }
                if (scene.nextLevel?.nextLevel == null) // Is the final floor
                    scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("UltimateAppleItem").Weighted(10));
            }
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int id, CustomLevelObject lvl)
        {
            if (title != "END" && (!title.StartsWith("F") || id < 1)) return;

            GameObject cherryMachine = AssetMan.Get<SodaMachine>("CherrySodaMachine").gameObject;

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

                strct.parameters.prefab = strct.parameters.prefab.AddToArray(cherryMachine.Weighted(6));
            }
        }

        [CaudexLoadEvent(LoadingEventOrder.Final)]
        private void PostLoad()
        {
            ManMemeCoinEvents.InitializePostEvents();

            // Modify RoomAssets containing vending machines to include Cherry BSODA machines if possible
            Transform cherryMachine = AssetMan.Get<SodaMachine>("CherrySodaMachine").transform;
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

                    swapData.potentialReplacements = swapData.potentialReplacements.AddToArray(cherryMachine.Weighted(6));
                }
            }
        }

        private void TrySpawnManMemeCoin(LevelGenerator gen)
        {
            if (gen.scene == null) return;

            ManMemeCoin coin = AssetMan.Get<ManMemeCoin>("ManMemeCoinNpc");
            if (gen.Ec.npcsToSpawn.Contains(coin)) return;

            SceneObjectMetadata meta = SceneObjectMetaStorage.Instance.Get(gen.scene);
            if (meta == null)
            {
                if (!gen.scene.levelTitle.StartsWith("F")) return;
            }
            else if (!meta.tags.Contains("main") && !meta.tags.Contains("recchars_mmcoin_spawns")) return;

            int floorCount = gen.scene.levelNo;
            SceneObject next = gen.scene;
            while (next != null && next.manager is MainGameManager)
            {
                floorCount++;
                if (floorCount >= 10)
                {
                    floorCount = 10;
                    break;
                }
                next = next.nextLevel;
            }
            if (floorCount == 0) return;

            if (new System.Random(gen.seed + 41223).Next(floorCount) == (gen.scene.levelNo % floorCount))
                gen.Ec.npcsToSpawn.Add(coin);
        }
    }
}
