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

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class Module_CaAprilFools : Module
    {
        public override string Name => "CA April Fools";

        public override Action LoadAction => Load;
        public override Action PostLoadAction => PostLoad;
        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleCaAprilFools;

        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Item", "CAItems"), x => "CAItems/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "MMCoin"), x => "MMCoinTex/" + x.name);

            AssetMan.Add("Boing", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "Boing.wav"), "RecChars_Sfx_Boing", SoundType.Effect, Color.white));

            LoadItems();
            LoadFixedMap();
            LoadManMemeCoinNpc();

            LevelGeneratorEventPatch.OnNpcAdd += TrySpawnManMemeCoin;
        }

        private void LoadItems()
        {
            // Flamin' Hot Cheetos
            ItemObject cheetos = new ItemBuilder(Info)
            .SetNameAndDescription("RecChars_Itm_FlaminHotCheetos", "RecChars_Desc_FlaminHotCheetos")
            .SetEnum("RecChars_FlaminHotCheetos")
            .SetMeta(ItemFlags.Persists, new string[] { "food", "recchars_daycare_exempt", "cann_hate", "adv_perfect", "adv_sm_potential_reward" })
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/FlaminHotCheetos_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/FlaminHotCheetos_Large"), 50f))
            .SetShopPrice(800)
            .SetGeneratorCost(85)
            .SetItemComponent<ITM_FlaminHotCheetos>()
            .Build();

            cheetos.name = "RecChars FlaminHotCheetos";

            ITM_FlaminHotCheetos cheetosItm = (ITM_FlaminHotCheetos)cheetos.item;
            cheetosItm.name = "Itm_FlaminHotCheetos";
            cheetosItm.gaugeSprite = cheetos.itemSpriteSmall;
            cheetosItm.audEat = ((ITM_ZestyBar)ItemMetaStorage.Instance.FindByEnum(Items.ZestyBar).value.item).audEat;

            AssetMan.Add("FlaminHotCheetosItem", cheetos);

            // Cherry BSODA
            ItemObject cherryBsoda = new ItemBuilder(Info)
            .SetNameAndDescription("RecChars_Itm_CherryBsoda", "RecChars_Desc_CherryBsoda")
            .SetEnum("RecChars_CherryBsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, new string[] { "food", "drink", "adv_perfect", "adv_sm_potential_reward" })
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

            ITM_CherryBsoda cherryBsodaUse = bsodaClone.gameObject.AddComponent<ITM_CherryBsoda>();

            cherryBsoda.item = cherryBsodaUse;
            cherryBsoda.item.name = "Itm_CherryBsoda";

            cherryBsodaUse.bsoda = bsodaClone;
            cherryBsodaUse.boing = AssetMan.Get<SoundObject>("Boing");

            ITM_GrapplingHook hook = (ITM_GrapplingHook)ItemMetaStorage.Instance.FindByEnum(Items.GrapplingHook).value.item;
            bsodaClone.entity.collisionLayerMask = hook.entity.collisionLayerMask;
            cherryBsodaUse.layerMask = hook.layerMask;

            AssetMan.Add("CherryBsodaItem", cherryBsoda);


            // Ultimate Apple
            ItemObject ultiApple = new ItemBuilder(Info)
            .SetNameAndDescription("RecChars_Itm_UltimateApple", "RecChars_Desc_UltimateApple")
            .SetEnum("RecChars_UltimateApple")
            .SetMeta(ItemFlags.NoUses, new string[] { "food", "crmp_contraband", "adv_forbidden_present" })
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
            ItemMetaData manglesMeta = new ItemMetaData(Info, new ItemObject[0]);
            manglesMeta.flags = ItemFlags.MultipleUse;
            manglesMeta.tags.AddRange(new string[] {"food", "recchars_daycare_exempt", "adv_good", "adv_sm_potential_reward"});
            // The Mangles would have this "homemade" flavor, thus you can feed that to Cann

            Items manglesEnum = EnumExtensions.ExtendEnum<Items>("RecChars_Mangles");

            ItemBuilder manglesBuilder = new ItemBuilder(Info)
            .SetNameAndDescription("RecChars_Itm_Mangles1", "RecChars_Desc_Mangles")
            .SetEnum(manglesEnum)
            .SetMeta(manglesMeta)
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/Mangles_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CAItems/Mangles_Large"), 50f))
            .SetShopPrice(500)
            .SetGeneratorCost(60)
            .SetItemComponent<ITM_Mangles>();

            ItemObject manglesItemObject = manglesBuilder.Build();
            manglesItemObject.name = "RecChars Mangles1";
            ITM_Mangles manglesItm = (ITM_Mangles)manglesItemObject.item;
            manglesItm.name = "Itm_Mangles1";
            manglesItm.audEat = cheetosItm.audEat;

            manglesBuilder.SetNameAndDescription("RecChars_Itm_Mangles2", "RecChars_Desc_Mangles");
            ItemObject manglesItemObject2 = manglesBuilder.Build();
            manglesItemObject2.name = "RecChars Mangles2";
            manglesItm = (ITM_Mangles)manglesItemObject2.item;
            manglesItm.name = "Itm_Mangles2";
            manglesItm.audEat = cheetosItm.audEat;
            manglesItm.nextStage = manglesItemObject;

            manglesBuilder.SetNameAndDescription("RecChars_Itm_Mangles3", "RecChars_Desc_Mangles");
            manglesItemObject = manglesItemObject2;
            manglesItemObject2 = manglesBuilder.Build();
            manglesItemObject2.name = "RecChars Mangles3";
            manglesItm = (ITM_Mangles)manglesItemObject2.item;
            manglesItm.name = "Itm_Mangles3";
            manglesItm.audEat = cheetosItm.audEat;
            manglesItm.nextStage = manglesItemObject;

            AssetMan.Add("ManglesItem", manglesItemObject2);
        }

        // TODO: Implement this in my own "bugfix" mod
        private void LoadFixedMap()
        {
            ItemObject map = ItemMetaStorage.Instance.FindByEnum(Items.Map).value;
            map.addToInventory = false;

            ITM_MapFixed fixedMap = RecommendedCharsPlugin.CloneComponent<ITM_Map, ITM_MapFixed>((ITM_Map)map.item);
            fixedMap.ding = Resources.FindObjectsOfTypeAll<SoundObject>().First(x => x.name == "CashBell");
            map.item = fixedMap;
        }

        private void LoadManMemeCoinNpc()
        {
            ManMemeCoin coin = new NPCBuilder<ManMemeCoin>(Info)
                .SetName("ManMemeCoin")
                .SetEnum("RecChars_ManMemeCoin")
                .SetPoster(AssetMan.Get<Texture2D>("MMCoinTex/pri_manmeme"), "RecChars_Pst_ManMeme1", "RecChars_Pst_ManMeme2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(new string[] {"no_balloon_frenzy", "adv_exclusion_hammer_immunity", "adv_ev_cold_school_immunity"})
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

        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.levelTitle == "END")
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<ManMemeCoin>("ManMemeCoinNpc"));
        }

        private void PostLoad()
        {
            ManMemeCoinEvents.InitializeBaseEvents();
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
