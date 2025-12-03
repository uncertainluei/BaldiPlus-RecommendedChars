using BaldisBasicsPlusAdvanced.API;
using BepInEx.Bootstrap;

using HarmonyLib;

using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI;
using MTM101BaldAPI.Components;

using PlusStudioLevelLoader;

using System.Collections.Generic;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.Advanced;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelLoader;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Registers;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;
using MTM101BaldAPI.Components.Animation;


namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    //[CaudexModule("Eveyone's Bsodaa"), CaudexModuleSaveTag("Mdl_Bsodaa")]
    [CaudexModuleConfig("Modules", "Bsodaa",
        "Adds Baldi and Playtime from Eveything is Bsodaa, with their own room and mechanic.", true)]
    public sealed class Module_Bsodaa : RecCharsModule
    {
        private readonly ModuleSaveSystem_Bsodaa saveSystem = new();
        public override ModuleSaveSystem SaveSystem => saveSystem;

        protected override void Initialized()
        {
            // Load texture and audio assets
            AddTexturesToAssetMan("BsodaaTex/", ["Textures", "Npc", "Bsodaa"]);
            AddTexturesToAssetMan("BsodaaItm/", ["Textures", "Item", "Bsodaa"]);
            AddTexturesToAssetMan("BsodaaRoom/", ["Textures", "Room", "Bsodaa"]);
            AddAudioToAssetMan("BsodaaAud/", ["Audio", "Bsodaa"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Bsodaa.json5");

            // Load patches
            Hooks.PatchAll(typeof(BsodaaSavePatches));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            LoadMiniBsoda();
            CreateBsodaaRoomBlueprint();
            LoadBsodaaHelper();
            LoadEveyBsodaa();

            CaudexGeneratorEvents.AddAction(CaudexGeneratorEventType.NpcPrep, AddBsodaaHelpers);
        }

        private void LoadMiniBsoda()
        {
            // BSODA Mini
            ItemObject miniBsoda = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_SmallBsoda", "Desc_RecChars_SmallBsoda")
            .SetEnum("RecChars_SmallBsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "drink"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaaItm/SmallBsoda_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaaItm/SmallBsoda_Large"), 50f))
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
            ObjMan.Add("Itm_BsodaMini", miniBsoda);


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

            miniBsodaSpray = GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.DietBsoda).value.item, MTM101BaldiDevAPI.prefabTransform);
            miniBsodaSpray.name = "Itm_SmallDietBsoda";
            miniBsodaSpray.spriteRenderer.transform.localScale = Vector3.one * 0.625f;
            miniBsodaSpray.time = 1.8f;
            miniBsodaSpray.speed = 26f;

            miniDietBsoda.item = miniBsodaSpray;

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_smalldietbsoda", miniDietBsoda);
            ObjMan.Add("Itm_DietBsodaMini", miniDietBsoda);
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
            helper.itmSmallBsoda = ObjMan.Get<ItemObject>("Itm_DietBsodaMini");

            CapsuleCollider collider = helper.GetComponent<CapsuleCollider>();
            collider.isTrigger = true;
            collider.height = 2.5f;
            collider.radius = 0.4f;
            collider.center = Vector3.down;

            ObjMan.Add("Npc_BsodaaHelper", helper);
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_bsodaahelper", helper.gameObject);

            helper = GameObject.Instantiate(helper,MTM101BaldiDevAPI.prefabTransform);
            helper.name = "BsodaaHelper Diet";
            helper.forceDietMode = true;
            ObjMan.Add("Npc_BsodaaHelper_Diet", helper);

            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_bsodaahelper_diet", helper.gameObject);

            // Dummy NPC for Principal's Office poster
            GameObject helperNpcObj = new("BsodaaHelperDummyNpc");
            helperNpcObj.transform.parent = MTM101BaldiDevAPI.prefabTransform;
            NPC dummy = helperNpcObj.AddComponent<BsodaaHelperDummyNpc>();
            dummy.ignorePlayerOnSpawn = true;
            dummy.potentialRoomAssets = [];
            dummy.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("BsodaaTex/pri_bsodaahelper"), "PST_PRI_RecChars_BsodaaHelper1", "PST_PRI_RecChars_BsodaaHelper2");
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_bsodaahelper", dummy.Poster);
            
            ObjMan.Add("Npc_BsodaaHelperDummy", dummy);
        }

        private void LoadEveyBsodaa()
        {
            EveyBsodaa bsodaaGuy = new NPCBuilder<EveyBsodaa>(Plugin)
                .SetName("Bsodaa")
                .SetEnum("RecChars_Bsodaa")
                .SetPoster(AssetMan.Get<Texture2D>("BsodaaTex/pri_bsodaa"), "PST_PRI_RecChars_Bsodaa1", "PST_PRI_RecChars_Bsodaa2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(["lower_balloon_frenzy_priority", "adv_exclusion_hammer_immunity"])
                .AddPotentialRoomAssets()
                .AddLooker()
                .AddTrigger()
                .IgnorePlayerOnSpawn()
                .Build();

            EveyBsodaa.charEnum = bsodaaGuy.Character;

            Sprite[] sprites = AssetLoader.SpritesFromSpriteSheetCount(AssetMan.Get<Texture2D>("BsodaaTex/Bsodaa_Idle"), 106, 256, 32f, 3);

            bsodaaGuy.spriteRenderer[0].transform.localPosition = Vector3.up * -1.08f;
            bsodaaGuy.spriteRenderer[0].sprite = sprites[0];

            bsodaaGuy.navigator.accel = 10f;
            bsodaaGuy.navigator.speed = 14f;
            bsodaaGuy.navigator.maxSpeed = 14f;

            bsodaaGuy.looker.layerMask = NPCMetaStorage.Instance.Get(Character.Principal).value.looker.layerMask;

            bsodaaGuy.animator = bsodaaGuy.gameObject.AddComponent<CustomSpriteRendererAnimator>();
            bsodaaGuy.animator.renderer = bsodaaGuy.spriteRenderer[0];

            bsodaaGuy.animator.AddAnimation("Idle", new(8, [sprites[0]]));
            bsodaaGuy.animator.AddAnimation("Happy", new(8, [sprites[1]]));
            bsodaaGuy.animator.AddAnimation("Upset", new(8, [sprites[2]]));

            sprites = AssetLoader.SpritesFromSpriteSheetCount(AssetMan.Get<Texture2D>("BsodaaTex/Bsodaa_Shoot"), 106, 256, 32f, 6);

            bsodaaGuy.animator.AddAnimation("Charge", new([
                new(sprites[4], 0.5f),
                new(sprites[3], 0.125f),
                new(sprites[2], 0.125f),
                new(sprites[1], 0.125f),
                new(sprites[0], 0.125f)
            ]));
            bsodaaGuy.animator.AddAnimation("Shoot", new([
                new(sprites[5], 0.375f),
                new(bsodaaGuy.spriteRenderer[0].sprite, 0.125f)
            ]));

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

            bsodaaGuy.projectilePre = SwapComponentSimple<ITM_BSODA, EveyBsodaaSpray>(GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item, MTM101BaldiDevAPI.prefabTransform));
            bsodaaGuy.projectilePre.spriteRenderer.sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BsodaaTex/Bsodaa_Spray"), 8f);
            bsodaaGuy.projectilePre.time = 10f;
            bsodaaGuy.projectilePre.name = "Bsodaa_Spray";

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_bsodaa", bsodaaGuy);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_bsodaa", bsodaaGuy.Poster);

            bsodaaGuy.potentialRoomAssets = RoomAssetsFromDirectory(ObjMan.Get<CaudexRoomBlueprint>("Room_Bsodaa"), "Bsodaa",
                50, 50, 50, 25, 25, 25, 25, 150);
            ObjMan.Add("Npc_Bsodaa", bsodaaGuy);
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

            LevelLoaderCompatHelper.AddRoom(bsodaaRoom);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_bsodaaflor", bsodaaRoom.texFloor);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_bsodaawall", bsodaaRoom.texWall);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_bsodaaceil", bsodaaRoom.texCeil);
            LevelLoaderPlugin.Instance.lightTransforms.Add("recchars_bsodaa", bsodaaRoom.lightObj);
            ObjMan.Add("Room_Bsodaa", bsodaaRoom);
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
            ItemObject smallBsoda = ObjMan.Get<ItemObject>("Itm_BsodaMini");
            ItemObject smallDietBsoda = ObjMan.Get<ItemObject>("Itm_DietBsodaMini");

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
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<EveyBsodaa>("Npc_Bsodaa"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
                else
                    scene.potentialNPCs.CopyNpcWeight(Character.DrReflex, ObjMan.Get<EveyBsodaa>("Npc_Bsodaa"));
                return;
            }

            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.potentialNPCs.CopyNpcWeight(Character.DrReflex, ObjMan.Get<EveyBsodaa>("Npc_Bsodaa"));
                }
                else if (id == 1)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<EveyBsodaa>("Npc_Bsodaa"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
            }
        }
        private void AddBsodaaHelpers(LevelGenerator gen)
        {
            NPC helperDummy = ObjMan.Get<NPC>("Npc_BsodaaHelperDummy");
            int bsodaas = gen.Ec.npcsToSpawn.Where(x => x != null && x.Character == EveyBsodaa.charEnum).ToArray().Length;
            for (int i = 0; i < bsodaas; i++)
                gen.Ec.npcsToSpawn.Add(helperDummy);
        }
    }
}
