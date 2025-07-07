using BepInEx.Configuration;

using HarmonyLib;

using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI;

using System;
using System.Collections.Generic;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;
using UnityEngine.SceneManagement;
using System.ComponentModel;
using Unity.Collections.LowLevel.Unsafe;
using PlusLevelLoader;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class Module_CaAprilFools : Module
    {
        public override string Name => "CA April Fools";

        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;
        public override Action<string, int, CustomLevelObject> LevelAddendAction => FloorAddendLvl;

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleCaAprilFools;

        [ModuleLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Item", "CaAprilFools"), x => "CAItems/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "MMCoin"), x => "MMCoinTex/" + x.name);

            AssetMan.Add("Boing", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "Boing.wav"), "Sfx_RecChars_CherryBsodaBoing", SoundType.Effect, Color.white));
            AssetMan.Add("CartoonEating", Resources.FindObjectsOfTypeAll<SoundObject>().First(x => x.name == "CartoonEating" && x.GetInstanceID() >= 0));

            LoadItems();
            //LoadFixedMap();
            LoadManMemeCoinNpc();
            ManMemeCoinEvents.InitializeBaseEvents();

            LevelGeneratorEventPatch.OnNpcAdd += TrySpawnManMemeCoin;
        }

        private void LoadItems()
        {
            // Flamin' Hot Cheepers
            ItemObject puffs = new ItemBuilder(Info)
            .SetNameAndDescription("Itm_RecChars_FlaminPuffs", "Desc_RecChars_FlaminPuffs")
            .SetEnum("RecChars_FlaminPuffs")
            .SetMeta(ItemFlags.Persists, ["food", "recchars_daycare_exempt", "cann_hate", "adv_perfect", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/FlaminPuffs_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/FlaminPuffs_Large"), 50f))
            .SetShopPrice(800)
            .SetGeneratorCost(85)
            .SetItemComponent<ITM_FlaminPuffs>()
            .Build();

            puffs.name = "RecChars FlaminPuffs";

            ITM_FlaminPuffs puffsItm = (ITM_FlaminPuffs)puffs.item;
            puffsItm.name = "Itm_FlaminPuffs";
            puffsItm.gaugeSprite = puffs.itemSpriteSmall;
            puffsItm.audEat = AssetMan.Get<SoundObject>("CartoonEating");

            AssetMan.Add("FlaminPuffsItem", puffs);


            // Cherry BSODA
            ItemObject cherryBsoda = new ItemBuilder(Info)
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
            bsodaClone.time = 10f;
            bsodaClone.moveMod.movementMultiplier = 0.45f;

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
            ItemObject ultiApple = new ItemBuilder(Info)
            .SetNameAndDescription("Itm_RecChars_UltimateApple", "Desc_RecChars_UltimateApple")
            .SetEnum("RecChars_UltimateApple")
            .SetMeta(ItemFlags.NoUses, ["food", "crmp_contraband", "adv_forbidden_present"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/UltimateApple_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/UltimateApple_Large"), 50f))
            .SetShopPrice(2500)
            .SetGeneratorCost(100)
            .Build();

            ultiApple.name = "RecChars UltimateApple";
            ultiApple.item = ItemMetaStorage.Instance.FindByEnum(Items.Apple).value.item;

            Baldi_UltimateApple.ultiAppleEnum = ultiApple.itemType;
            Baldi_UltimateApple.ultiAppleSprites = AssetLoader.SpritesFromSpritesheet(2, 1, 32f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("CAItems/BaldiUltimateApple"));

            AssetMan.Add("UltimateAppleItem", ultiApple);


            // Can of Mangles
            ItemMetaData manglesMeta = new(Info, []);
            manglesMeta.flags = ItemFlags.MultipleUse;
            manglesMeta.tags.AddRange(["food", "recchars_daycare_exempt", "adv_good", "adv_sm_potential_reward"]);
            // The Mangles would have this "homemade" flavor, thus you can feed that to Cann

            Items manglesEnum = EnumExtensions.ExtendEnum<Items>("RecChars_Mangles");

            ItemBuilder manglesBuilder = new ItemBuilder(Info)
            .SetNameAndDescription("Itm_RecChars_Mangles1", "Desc_RecChars_Mangles")
            .SetEnum(manglesEnum)
            .SetMeta(manglesMeta)
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/Mangles_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/Mangles_Large"), 50f))
            .SetShopPrice(500)
            .SetGeneratorCost(60)
            .SetItemComponent<ITM_Mangles>();

            SoundObject audEat = ((ITM_ZestyBar)ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value.item).audEat;

            ItemObject manglesItemObject = manglesBuilder.Build();
            manglesItemObject.name = "RecChars Mangles1";
            ITM_Mangles manglesItm = (ITM_Mangles)manglesItemObject.item;
            manglesItm.name = "Itm_Mangles1";
            manglesItm.audEat = audEat;

            manglesBuilder.SetNameAndDescription("Itm_RecChars_Mangles2", "Desc_RecChars_Mangles");
            ItemObject manglesItemObject2 = manglesBuilder.Build();
            manglesItemObject2.name = "RecChars Mangles2";
            manglesItm = (ITM_Mangles)manglesItemObject2.item;
            manglesItm.name = "Itm_Mangles2";
            manglesItm.audEat = audEat;
            manglesItm.nextStage = manglesItemObject;

            manglesBuilder.SetNameAndDescription("Itm_RecChars_Mangles3", "Desc_RecChars_Mangles");
            manglesItemObject = manglesItemObject2;
            manglesItemObject2 = manglesBuilder.Build();
            manglesItemObject2.name = "RecChars Mangles3";
            manglesItm = (ITM_Mangles)manglesItemObject2.item;
            manglesItm.name = "Itm_Mangles3";
            manglesItm.audEat = audEat;
            manglesItm.nextStage = manglesItemObject;

            AssetMan.Add("ManglesItem", manglesItemObject2);
        }

        /*
        private void LoadFixedMap()
        {
            ItemObject map = ItemMetaStorage.Instance.FindByEnum(Items.Map).value;
            map.addToInventory = false;

            ITM_MapFixed fixedMap = RecommendedCharsPlugin.CloneComponent<ITM_Map, ITM_MapFixed>((ITM_Map)map.item);
            fixedMap.ding = Resources.FindObjectsOfTypeAll<SoundObject>().First(x => x.name == "CashBell");
            map.item = fixedMap;
        }*/

        private void LoadManMemeCoinNpc()
        {
            ManMemeCoin coin = new NPCBuilder<ManMemeCoin>(Info)
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

            Sprite[] sprites = RecommendedCharsPlugin.SplitSpriteSheet(AssetMan.Get<Texture2D>("MMCoinTex/ManMemeCoin"), 128, 128, 6, 25f);

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

        [ModuleCompatLoadEvent(RecommendedCharsPlugin.LevelLoaderGuid, LoadingEventOrder.Pre)]
        private void RegisterToLevelLoader()
        {
            PlusLevelLoaderPlugin.Instance.npcAliases.Add("recchars_manmemecoin", AssetMan.Get<ManMemeCoin>("ManMemeCoinNpc"));

            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_flaminpuffs", AssetMan.Get<ItemObject>("FlaminPuffs"));
            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_cherrybsoda", AssetMan.Get<ItemObject>("CherryBsodaItem"));
            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_ultimateapple", AssetMan.Get<ItemObject>("UltimateAppleItem"));
            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_mangles",  AssetMan.Get<ItemObject>("ManglesItem"));

            PlusLevelLoaderPlugin.Instance.prefabAliases.Add("recchars_cherrysodamachine", AssetMan.Get<SodaMachine>("CherrySodaMachine").gameObject);
        }

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

        [ModuleLoadEvent(LoadingEventOrder.Final)]
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
            else if (!meta.tags.Contains("main")) return;

            int chance = gen.scene.levelNo;
            SceneObject next = gen.scene;
            while (next = next.nextLevel)
                chance++;

            chance = new System.Random(gen.seed + 41223).Next(chance);
            if (chance == gen.scene.levelNo)
                gen.Ec.npcsToSpawn.Add(coin);
        }
    }
}
