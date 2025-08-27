using BaldisBasicsPlusAdvanced.API;
using BaldisBasicsPlusAdvanced.Game.Objects.Plates.KitchenStove;

using BepInEx.Configuration;

using HarmonyLib;

using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;

using BaldiLevelEditor;
using PlusLevelLoader;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LegacyEditor;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelLoader;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;
using BepInEx.Bootstrap;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.Advanced;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Util.Extensions;
using UncertainLuei.CaudexLib.Registers;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Eveyone's Bsodaa"), CaudexModuleSaveTag("Mdl_Bsodaa")]
    [CaudexModuleConfig("Modules", "Bsodaa",
        "Adds Baldi and Playtime from Eveything is Bsodaa, with their own room and mechanic.", true)]
    public sealed class Module_Bsodaa : RecCharsModule
    {
        private readonly ModuleSaveSystem_Bsodaa saveSystem = new();
        public override ModuleSaveSystem SaveSystem => saveSystem;

        protected override void Loaded()
        {
            // Load patches
            Hooks.PatchAll(typeof(BsodaaSavePatches));
            RecommendedCharsPlugin.PatchCompat(typeof(BsodaaHelperExpelBlacklist), RecommendedCharsPlugin.AdvancedGuid);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Room", "Bsodaa"), x => "BsodaaRoom/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Npc", "Bsodaa"), x => "BsodaaTex/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Item", "Bsodaa"), x => "BsodaaItm/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(BasePlugin), "Audio", "Bsodaa"), "BsodaaAud/");

            LoadMiniBsoda();
            CreateBsodaaRoomBlueprint();
            LoadBsodaaHelper();
            LoadEveyBsodaa();

            CaudexGeneratorEvents.AddAction(CaudexGeneratorEventType.NpcPrep, AddBsodaaHelpers);
        }

        private void LoadMiniBsoda()
        {
            // Diet BSODA Mini
            ItemObject miniDietBsoda = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_SmallDietBsoda", "Desc_RecChars_SmallDietBsoda")
            .SetEnum("RecChars_SmallDietBsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "drink"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaaItm/SmallDietBsoda_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaaItm/SmallDietBsoda_Large"), 50f))
            .SetShopPrice(160)
            .SetGeneratorCost(40)
            .Build();

            miniDietBsoda.name = "RecChars SmallDietBsoda";

            ITM_BSODA miniBsodaSpray = GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.DietBsoda).value.item, MTM101BaldiDevAPI.prefabTransform);
            miniBsodaSpray.name = "Itm_SmallDietBsoda";
            miniBsodaSpray.spriteRenderer.transform.localScale = Vector3.one * 0.625f;
            miniBsodaSpray.time = 1.8f;
            miniBsodaSpray.speed = 26f;

            miniDietBsoda.item = miniBsodaSpray;

            AssetMan.Add("SmallDietBsodaItem", miniDietBsoda);


            // BSODA Mini
            ItemObject miniBsoda = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_SmallBsoda", "Desc_RecChars_SmallBsoda")
            .SetEnum("RecChars_SmallBsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "drink"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaaItm/SmallBsoda_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaaItm/SmallBsoda_Large"), 50f))
            .SetShopPrice(320)
            .SetGeneratorCost(55)
            .Build();

            miniDietBsoda.name = "RecChars SmallBsoda";

            miniBsodaSpray = GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item, MTM101BaldiDevAPI.prefabTransform);
            miniBsodaSpray.name = "Itm_SmallBsoda";
            miniBsodaSpray.spriteRenderer.transform.localScale = Vector3.one * 0.625f;
            miniBsodaSpray.time = 18f;
            miniBsodaSpray.speed = 26f;
            miniBsodaSpray.gameObject.AddComponent<VanillaBsodaComponent>();

            miniBsoda.item = miniBsodaSpray;

            AssetMan.Add("SmallBsodaItem", miniBsoda);
        }

        private void LoadBsodaaHelper()
        {
            // Essentially this other guy will not be like the below guy, as in she's a glorified structure rather than an
            // NPC.

            GameObject helperObj = new("BsodaaHelper", typeof(BsodaaHelper), typeof(CapsuleCollider), typeof(PropagatedAudioManager));
            helperObj.transform.parent = MTM101BaldiDevAPI.prefabTransform;
            helperObj.transform.localPosition = Vector3.zero;

            BsodaaHelper helper = helperObj.GetComponent<BsodaaHelper>();
            helper.audMan = helperObj.GetComponent<PropagatedAudioManager>();
            GameObject.DestroyImmediate(helper.audMan.audioDevice.gameObject);
            ((PropagatedAudioManager)helper.audMan).minDistance = 10f;
            ((PropagatedAudioManager)helper.audMan).maxDistance = 150f;

            helper.audMan.overrideSubtitleColor = true;
            helper.audMan.subtitleColor = new(110f/255f, 134f/255f, 1f);

            helper.audOops = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/BHelp_OutOf"), "Vfx_RecChars_BsodaaHelper_Oops", SoundType.Voice, helper.audMan.subtitleColor);
            helper.audLaugh = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/BHelp_GiveSoda"), "Vfx_RecChars_BsodaaHelper_Laugh", SoundType.Voice, helper.audMan.subtitleColor);
            helper.audSad = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/BHelp_Sprayed"), "Vfx_RecChars_BsodaaHelper_Sad", SoundType.Voice, helper.audMan.subtitleColor);

            helper.audCount =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/BHelp_1"), "Vfx_Playtime_1", SoundType.Voice, helper.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/BHelp_2"), "Vfx_Playtime_2", SoundType.Voice, helper.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/BHelp_3"), "Vfx_Playtime_3", SoundType.Voice, helper.audMan.subtitleColor)
            ];

            GameObject spriteObj = new("Sprite", typeof(SpriteRenderer));
            spriteObj.transform.parent = helperObj.transform;
            spriteObj.transform.localPosition = Vector3.up * -2.16f;
            spriteObj.layer = LayerMask.NameToLayer("Billboard");

            helper.sprite = spriteObj.GetComponent<SpriteRenderer>();
            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(3, 1, 100f, Vector2.one/2f, AssetMan.Get<Texture2D>("BsodaaTex/BsodaaHelper"));

            helper.sprite.sprite = sprites[0];
            helper.sprite.material = AssetMan.Get<Material>("BillboardMaterial");
            helper.sprEmpty = sprites[1];
            helper.sprSprayed = sprites[2];

            helper.itmBsoda = ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value;
            helper.itmDietBsoda = ItemMetaStorage.Instance.FindByEnum(Items.DietBsoda).value;
            helper.itmSmallBsoda = AssetMan.Get<ItemObject>("SmallDietBsodaItem");

            CapsuleCollider collider = helper.GetComponent<CapsuleCollider>();
            collider.isTrigger = true;
            collider.height = 2.5f;
            collider.radius = 0.4f;
            collider.center = Vector3.down;

            AssetMan.Add("BsodaaHelperObject", helper);

            // Dummy NPC for Principal's Office poster
            GameObject helperNpcObj = new("BsodaaHelperDummyNpc");
            helperNpcObj.transform.parent = MTM101BaldiDevAPI.prefabTransform;
            NPC dummy = helperNpcObj.AddComponent<BsodaaHelperDummyNpc>();
            dummy.ignorePlayerOnSpawn = true;
            dummy.potentialRoomAssets = [];
            dummy.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("BsodaaTex/pri_bsodaahelper"), "PST_PRI_RecChars_BsodaaHelper1", "PST_PRI_RecChars_BsodaaHelper2");

            AssetMan.Add("BsodaaHelperPoster", dummy);
        }

        private void LoadEveyBsodaa()
        {
            EveyBsodaa bsodaaGuy = new NPCBuilder<EveyBsodaa>(Plugin)
                .SetName("Bsodaa")
                .SetEnum("RecChars_Bsodaa")
                .SetPoster(AssetMan.Get<Texture2D>("BsodaaTex/pri_bsodaa"), "PST_PRI_RecChars_Bsodaa1", "PST_PRI_RecChars_Bsodaa2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(["lower_balloon_frenzy_priority", "adv_exclusion_hammer_immunity"])
                .AddPotentialRoomAssets(CreateBsodaaRoomAssets())
                .AddLooker()
                .AddTrigger()
                .IgnorePlayerOnSpawn()
                .Build();

            EveyBsodaa.charEnum = bsodaaGuy.Character;

            Sprite[] sprites = CaudexAssetLoader.SplitSpriteSheet(AssetMan.Get<Texture2D>("BsodaaTex/Bsodaa_Idle"), 106, 256, 3, 32f);

            bsodaaGuy.spriteRenderer[0].transform.localPosition = Vector3.up * -1.08f;
            bsodaaGuy.spriteRenderer[0].sprite = sprites[0];

            EveyBsodaa.animations = new Dictionary<string, Sprite[]>()
            {
                {"Idle", new Sprite[] { sprites[0] }},
                {"Happy", new Sprite[] { sprites[1] }},
                {"Upset", new Sprite[] { sprites[2] }},
            };

            sprites = CaudexAssetLoader.SplitSpriteSheet(AssetMan.Get<Texture2D>("BsodaaTex/Bsodaa_Shoot"), 106, 256, 6, 32f);

            EveyBsodaa.animations.Add("Charge",
            [
                sprites[4],
                sprites[4],
                sprites[4],
                sprites[4],
                sprites[3],
                sprites[2],
                sprites[1],
                sprites[0]
            ]);
            EveyBsodaa.animations.Add("Shoot",
            [
                sprites[5],
                sprites[5],
                sprites[5],
                bsodaaGuy.spriteRenderer[0].sprite
            ]);

            bsodaaGuy.navigator.accel = 10f;
            bsodaaGuy.navigator.speed = 14f;
            bsodaaGuy.navigator.maxSpeed = 14f;

            bsodaaGuy.looker.layerMask = NPCMetaStorage.Instance.Get(Character.Principal).value.looker.layerMask;

            bsodaaGuy.animator = bsodaaGuy.gameObject.AddComponent<CustomSpriteAnimator>();
            bsodaaGuy.animator.spriteRenderer = bsodaaGuy.spriteRenderer[0];

            bsodaaGuy.audMan = bsodaaGuy.GetComponent<AudioManager>();
            bsodaaGuy.audMan.subtitleColor = new Color(3f/255f, 36f/255f, 1f);

            PineDebugNpcIconPatch.icons.Add(bsodaaGuy.Character, AssetMan.Get<Texture2D>("BsodaaTex/BorderBsodaa"));
            CharacterRadarColorPatch.colors.Add(bsodaaGuy.Character, bsodaaGuy.audMan.subtitleColor);

            bsodaaGuy.audCharging = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/Evey_Charging"), "Sfx_RecChars_Bsodaa_Charging", SoundType.Effect, bsodaaGuy.audMan.subtitleColor);
            bsodaaGuy.audReloaded = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/Evey_Thanks"), "Vfx_RecChars_Bsodaa_Thanks", SoundType.Voice, bsodaaGuy.audMan.subtitleColor);

            bsodaaGuy.audSuccess =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/Evey_Happy1"), "Vfx_RecChars_Bsodaa_Happy1", SoundType.Voice, bsodaaGuy.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/Evey_Happy2"), "Vfx_RecChars_Bsodaa_Happy2", SoundType.Voice, bsodaaGuy.audMan.subtitleColor)
            ];

            bsodaaGuy.projectilePre = RecommendedCharsPlugin.SwapComponentSimple<ITM_BSODA, EveyBsodaaSpray>(GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item, MTM101BaldiDevAPI.prefabTransform));
            bsodaaGuy.projectilePre.spriteRenderer.sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaaTex/Bsodaa_Spray"), 8f);
            bsodaaGuy.projectilePre.time = 10f;
            bsodaaGuy.projectilePre.name = "Bsodaa_Spray";

            AssetMan.Add("BsodaaNpc", bsodaaGuy);
        }

        private void CreateBsodaaRoomBlueprint()
        {
            CaudexRoomBlueprint bsodaaRoom = new(Plugin, "BsodaaRoom", "RecChars_Bsodaa");

            bsodaaRoom.texFloor = AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaCarpet");
            bsodaaRoom.texWall = AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaWall");
            bsodaaRoom.texCeil = AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaCeiling");
            bsodaaRoom.keepTextures = true;

            bsodaaRoom.doorMats = ObjectCreators.CreateDoorDataObject("BsodaaDoor", AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaDoor_Open"), AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaDoor_Closed"));

            bsodaaRoom.lightObj = GameObject.Instantiate(((DrReflex)NPCMetaStorage.Instance.Get(Character.DrReflex).value).potentialRoomAssets[0].selection.lightPre, MTM101BaldiDevAPI.prefabTransform);
            bsodaaRoom.lightObj.name = "BsodaaLight";
            MeshRenderer light = bsodaaRoom.lightObj.GetComponentInChildren<MeshRenderer>();
            light.sharedMaterial = new Material(light.sharedMaterial)
            {
                name = "BsodaaRoom_Light",
                mainTexture = AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaLight")
            };

            bsodaaRoom.mapMaterial = ObjectCreators.CreateMapTileShader(AssetMan.Get<Texture2D>("BsodaaRoom/Map_Bsodaa"));
            bsodaaRoom.color = new(57f/255f, 87f/255f, 159f/255f);

            bsodaaRoom.posterChance = 0.1f;

            GameObject roomFunction = new("BsodaaRoomFunction", typeof(RoomFunctionContainer), typeof(BsodaaRoomFunction));
            roomFunction.transform.parent = MTM101BaldiDevAPI.prefabTransform;
            roomFunction.transform.localPosition = Vector3.zero;
            bsodaaRoom.functionContainer = roomFunction.GetComponent<RoomFunctionContainer>();
            bsodaaRoom.functionContainer.functions =
            [
                roomFunction.GetComponent<BsodaaRoomFunction>()
            ];

            AssetMan.Add("BsodaaRoomBlueprint", bsodaaRoom);
        }

        private WeightedRoomAsset[] CreateBsodaaRoomAssets()
        {
            CaudexRoomBlueprint blueprint = AssetMan.Get<CaudexRoomBlueprint>("BsodaaRoomBlueprint");
            SodaMachine dietSodaMachine = Resources.FindObjectsOfTypeAll<SodaMachine>().First(x => x.name == "DietSodaMachine" && x.GetInstanceID() >= 0);
            BsodaaHelper helper = AssetMan.Get<BsodaaHelper>("BsodaaHelperObject");

            List<WeightedRoomAsset> rooms = [];

            RoomAsset newRoom = blueprint.CreateAsset("Sugary0");
            rooms.Add(newRoom.Weighted(150));
            newRoom.cells =
            [
                RoomAssetHelper.Cell(0,0,12),
                RoomAssetHelper.Cell(1,0,4),
                RoomAssetHelper.Cell(2,0,6),
                RoomAssetHelper.Cell(0,1,9),
                RoomAssetHelper.Cell(1,1,0),
                RoomAssetHelper.Cell(2,1,3),
                RoomAssetHelper.Cell(1,2,11)
            ];
            newRoom.standardLightCells = [new(1, 1)];
            newRoom.potentialDoorPositions =
            [
                new(0,0),
                new(1,0),
                new(2,0)
            ];
            newRoom.entitySafeCells =
            [
                new(1,0),
                new(1,1)
            ];
            newRoom.blockedWallCells =
            [
                new(0,1),
                new(2,1)
            ];
            newRoom.basicObjects =
            [
                RoomAssetHelper.ObjectPlacement(dietSodaMachine, new Vector3(5f,0f,15f), 0f),
                RoomAssetHelper.ObjectPlacement(dietSodaMachine, new Vector3(25f,0f,15f), 0f),
                RoomAssetHelper.ObjectPlacement(helper, new Vector3(15f,5f,25f), 0f)
            ];

            // "Thumbs Up" shape
            newRoom = blueprint.CreateAsset("Luei0");
            rooms.Add(newRoom.Weighted(50));
            newRoom.cells =
            [
                RoomAssetHelper.Cell(0,0,12),
                RoomAssetHelper.Cell(1,0,4),
                RoomAssetHelper.Cell(2,0,6),
                RoomAssetHelper.Cell(0,1,8),
                RoomAssetHelper.Cell(1,1,1),
                RoomAssetHelper.Cell(2,1,3),
                RoomAssetHelper.Cell(0,2,11)
            ];
            newRoom.standardLightCells = [new(1, 1)];
            newRoom.potentialDoorPositions =
            [
                new(0,0),
                new(1,0),
                new(2,1)
            ];
            newRoom.entitySafeCells =
            [
                new(1,0),
                new(1,1)
            ];
            newRoom.blockedWallCells =
            [
                new(2,0),
                new(1,1)
            ];
            newRoom.basicObjects =
            [
                RoomAssetHelper.ObjectPlacement(dietSodaMachine, new Vector3(15f,0f,15f), 0f),
                RoomAssetHelper.ObjectPlacement(dietSodaMachine, new Vector3(25f,0f,5f), 90f),
                RoomAssetHelper.ObjectPlacement(helper, new Vector3(5f,5f,25f), 0f)
            ];

            // Based on B-Side Skid's idea
            newRoom = blueprint.CreateAsset("Luei1");
            rooms.Add(newRoom.Weighted(50));
            newRoom.cells =
            [
                RoomAssetHelper.Cell(0,0,13),
                RoomAssetHelper.Cell(1,0,4),
                RoomAssetHelper.Cell(2,0,4),
                RoomAssetHelper.Cell(3,0,4),
                RoomAssetHelper.Cell(4,0,7),
                RoomAssetHelper.Cell(1,1,9),
                RoomAssetHelper.Cell(2,1,0),
                RoomAssetHelper.Cell(3,1,3),
                RoomAssetHelper.Cell(2,2,11)
            ];
            newRoom.standardLightCells = [new(2, 1)];
            newRoom.potentialDoorPositions =
            [
                new(2,0),
                new(1,1),
                new(3,1)
            ];
            newRoom.entitySafeCells =
            [
                new(2,0),
                new(2,1)
            ];
            newRoom.blockedWallCells =
            [
                new(0,0),
                new(4,0)
            ];
            newRoom.basicObjects =
            [
                RoomAssetHelper.ObjectPlacement(dietSodaMachine, new Vector3(5f,0f,5f), -90f),
                RoomAssetHelper.ObjectPlacement(dietSodaMachine, new Vector3(45f,0f,5f), 90f),
                RoomAssetHelper.ObjectPlacement(helper, new Vector3(25f,5f,25f), 0f)
            ];

            // "Staircase" shape
            newRoom = blueprint.CreateAsset("Luei2");
            rooms.Add(newRoom.Weighted(50));
            newRoom.cells =
            [
                RoomAssetHelper.Cell(0,0,12),
                RoomAssetHelper.Cell(1,0,4),
                RoomAssetHelper.Cell(2,0,7),

                RoomAssetHelper.Cell(0,1,8),
                RoomAssetHelper.Cell(1,1,3),

                RoomAssetHelper.Cell(0,2,11)
            ];
            newRoom.standardLightCells = [new(1, 1)];
            newRoom.potentialDoorPositions =
            [
                new(0,0),
                new(1,0),
                new(0,1)
            ];
            newRoom.entitySafeCells = [new(0,0)];
            newRoom.blockedWallCells =
            [
                new(2,0),
                new(0,2)
            ];
            newRoom.basicObjects =
            [
                RoomAssetHelper.ObjectPlacement(dietSodaMachine, new Vector3(5f,0f,25f), 90f),
                RoomAssetHelper.ObjectPlacement(dietSodaMachine, new Vector3(25f,0f,5f), 0f),
                RoomAssetHelper.ObjectPlacement(helper, new Vector3(15f,5f,15f), 0f)
            ];

            return [.. rooms];
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelLoaderGuid, LoadingEventOrder.Pre)]
        private void RegisterToLevelLoader()
        {
            PlusLevelLoaderPlugin.Instance.npcAliases.Add("recchars_bsodaa", AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
            PlusLevelLoaderPlugin.Instance.prefabAliases.Add("recchars_bsodaahelper", AssetMan.Get<BsodaaHelper>("BsodaaHelperObject").gameObject);

            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_smalldietbsoda", AssetMan.Get<ItemObject>("SmallDietBsodaItem"));
            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_smallbsoda", AssetMan.Get<ItemObject>("SmallBsodaItem"));

            CaudexRoomBlueprint blueprint = AssetMan.Get<CaudexRoomBlueprint>("BsodaaRoomBlueprint");
            LevelLoaderCompatHelper.AddRoom(blueprint);
            PlusLevelLoaderPlugin.Instance.textureAliases.Add("recchars_bsodaaflor", blueprint.texFloor);
            PlusLevelLoaderPlugin.Instance.textureAliases.Add("recchars_bsodaawall", blueprint.texWall);
            PlusLevelLoaderPlugin.Instance.textureAliases.Add("recchars_bsodaaceil", blueprint.texCeil);
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LegacyEditorGuid, LoadingEventOrder.Pre)]
        private void RegisterToLegacyEditor()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Editor", "Bsodaa"), x => "BsodaaEditor/" + x.name);

            LegacyEditorCompatHelper.AddCharacterObject("recchars_bsodaa", AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
            LegacyEditorCompatHelper.AddObject("recchars_bsodaahelper", AssetMan.Get<BsodaaHelper>("BsodaaHelperObject"), Vector3.up*5);
            BaldiLevelEditorPlugin.itemObjects.Add("recchars_smalldietbsoda", AssetMan.Get<ItemObject>("SmallDietBsodaItem"));
            BaldiLevelEditorPlugin.itemObjects.Add("recchars_smallbsoda", AssetMan.Get<ItemObject>("SmallBsodaItem"));

            LegacyEditorCompatHelper.AddRoomDefaultTextures("recchars_bsodaaroom", "recchars_bsodaaflor", "recchars_bsodaawall", "recchars_bsodaaceil");

            new ExtNpcTool("recchars_bsodaa", "BsodaaEditor/Npc_bsodaa").AddToEditor("characters");
            new ExtRoomObjTool("recchars_bsodaahelper", "BsodaaEditor/Npc_bsodaahelper", "recchars_bsodaaroom").AddToEditor("characters");
            new ExtItemTool("recchars_smalldietbsoda", "BsodaaEditor/Itm_smalldietbsoda").AddToEditor("items");
            new ExtItemTool("recchars_smallbsoda", "BsodaaEditor/Itm_smallbsoda").AddToEditor("items");
            new ExtFloorTool("recchars_bsodaaroom", "BsodaaEditor/Floor_bsodaa").AddToEditor("halls");
        }
            
        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Pre)]
        private void AdvancedCompat()
        {
            ApiManager.AddNewSymbolMachineWords(Plugin, "BSODA");
            ApiManager.AddNewTips(Plugin, "Adv_Elv_Tip_RecChars_BsodaaHelper", "Adv_Elv_Tip_RecChars_BsodaaHelperSpray");
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Post)]
        private void AdvancedRecipes()
        {
            ItemObject smallBsoda = AssetMan.Get<ItemObject>("SmallBsodaItem");
            ItemObject smallDietBsoda = AssetMan.Get<ItemObject>("SmallDietBsodaItem");

            BepInEx.PluginInfo advInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.AdvancedGuid];
            
            AdvancedCompatHelper.RemoveStoveRecipes(advInfo, (x, y) => x.Length == 1 && x[0].itemType.ToStringExtended() == "IceBoots");
            AdvancedCompatHelper.AddStoveRecipe(Plugin, [ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("IceBoots"), advInfo).value], [smallDietBsoda, smallDietBsoda]);
            AdvancedCompatHelper.AddStoveRecipe(Plugin, [smallBsoda], [smallDietBsoda, smallDietBsoda]);
            AdvancedCompatHelper.AddStoveRecipe(Plugin, [smallDietBsoda, smallDietBsoda], [smallBsoda]);
        }

        [CaudexLoadEvent(LoadingEventOrder.Final)]
        private void IdentifyBaseBsodaSpray()
        {
            // Add a component to identify base BSODA sprays
            ITM_BSODA bsoda = (ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item;
            bsoda.gameObject.AddComponent<VanillaBsodaComponent>();
            bsoda.MarkAsNeverUnload();
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();

                if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
                else
                    scene.potentialNPCs.CopyNpcWeight(Character.DrReflex, AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
                return;
            }

            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.potentialNPCs.CopyNpcWeight(Character.DrReflex, AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
                }
                else if (id == 1)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
            }
        }
        private void AddBsodaaHelpers(LevelGenerator gen)
        {
            NPC helperDummy = RecommendedCharsPlugin.AssetMan.Get<NPC>("BsodaaHelperPoster");
            int bsodaas = gen.Ec.npcsToSpawn.Where(x => x != null && x.Character == EveyBsodaa.charEnum).ToArray().Length;
            for (int i = 0; i < bsodaas; i++)
                gen.Ec.npcsToSpawn.Add(helperDummy);
        }

        /* The dummy NPC already despawns itself upon spawning, so this isn't necessary 
        private void RemoveBsodaaHelpers(LevelGenerator gen)
        {
            NPC helperDummy = RecommendedCharsPlugin.AssetMan.Get<NPC>("BsodaaHelperPoster");
            List<Cell> npcSpawnTiles = new List<Cell>(gen.Ec.npcSpawnTile);
            bool changesFound = false;

            for (int i = gen.Ec.npcsToSpawn.Count - 1; i >= 0; i--)
            {
                if (gen.Ec.npcsToSpawn[i] != helperDummy) continue;

                changesFound = true;
                gen.Ec.npcsToSpawn.RemoveAt(i);
                npcSpawnTiles.RemoveAt(i);
            }
            if (changesFound)
                gen.Ec.npcSpawnTile = npcSpawnTiles.ToArray();
        } */
    }
}
