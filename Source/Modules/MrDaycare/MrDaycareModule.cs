using BaldisBasicsPlusAdvanced.API;

using BepInEx.Configuration;
using BepInEx.Bootstrap;

using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;
using UncertainLuei.CaudexLib.Objects;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelLoader;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.FragileWindows;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;

using APIConnector;
using UncertainLuei.CaudexLib.Registers;
using PlusStudioLevelLoader;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Mr. Daycare"), CaudexModuleSaveTag("Mdl_MrDaycare")]
    [CaudexModuleConfig("Modules", "MrDaycare",
        "Adds Mr. Daycare from Dave's House, as well the Pie and Door Key items.", true)]
    public sealed class Module_MrDaycare : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Room", "Daycare"), x => "DaycareRoom/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Npc", "Daycare"), x => "DaycareTex/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Item", "Daycare"), x => "DaycareItm/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(BasePlugin), "Audio", "Daycare"), "DaycareAud/");

            AssetMan.Add("PieThrow", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "PieThrow.wav"), "", SoundType.Effect, Color.white, 0f));
            AssetMan.Add("PieSplat", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "PieSplat.wav"), "Sfx_RecChars_PieSplat", SoundType.Effect, Color.white));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "MrDaycare.json5");

            // Load patches
            Hooks.PatchAll(typeof(MrDaycarePatches));
            Hooks.PatchAll(typeof(DaycareRoomPatches));
            RecommendedCharsPlugin.PatchCompat(typeof(MrDaycareAdvancedPatches), RecommendedCharsPlugin.AdvancedGuid);
            RecommendedCharsPlugin.PatchCompat(typeof(MrDaycareEcoFriendlyPatches), RecommendedCharsPlugin.EcoFriendlyGuid);
            RecommendedCharsPlugin.PatchCompat(typeof(MrDaycareFragilePatches), RecommendedCharsPlugin.FragileWindowsGuid);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            // Create posters
            AssetMan.Add("DaycareRulesPoster", CreatePoster("DaycareRoom/pst_daycarerules", "DaycarePoster_Rules"));
            AssetMan.Add("DaycareInfoPoster", CreatePoster("DaycareRoom/pst_daycareinfo", "DaycarePoster_Info"));
            AssetMan.Add("DaycareClockPoster", CreatePoster("DaycareRoom/pst_daycareclock", "DaycarePoster_Clock"));

            CreateDaycareBlueprint();
            LoadItems();
            LoadMrDaycare();

            CaudexEvents.OnItemUse += SetGuiltForItems;
            CaudexGeneratorEvents.AddAction(CaudexGeneratorEventType.NpcPrep, AddPosterToLevel);
        }

        private void LoadItems()
        {
            // Pie
            ItemObject pie = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_Pie", "Desc_RecChars_Pie")
            .SetEnum("RecChars_Pie")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, ["food", "recchars_daycare_exempt", "adv_good", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareItm/Pie_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareItm/Pie_Large"), 50f))
            .SetShopPrice(500)
            .SetGeneratorCost(75)
            .Build();

            pie.name = "RecChars Pie";

            Gum gumClone = GameObject.Instantiate(((Beans)NPCMetaStorage.Instance.Get(Character.Beans).value).gumPre, MTM101BaldiDevAPI.prefabTransform);

            ITM_Pie pieUse = gumClone.gameObject.AddComponent<ITM_Pie>();
            pie.item = pieUse;
            pie.item.name = "Itm_Pie";

            pieUse.entity = gumClone.entity;
            pieUse.audMan = gumClone.audMan;

            pieUse.audThrow = AssetMan.Get<SoundObject>("PieThrow");
            pieUse.audSplat = AssetMan.Get<SoundObject>("PieSplat");

            pieUse.flyingSprite = gumClone.flyingSprite;
            Sprite thrownPieSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareItm/Pie_Large"), 25f);
            thrownPieSprite.name = "Pie_Thrown";
            pieUse.flyingSprite.GetComponent<SpriteRenderer>().sprite = thrownPieSprite;

            pieUse.groundedSprite = gumClone.groundedSprite;
            pieUse.groundedSprite.transform.localPosition = Vector3.back * -0.1f;
            pieUse.groundedSprite.GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareItm/PieSplat"), 10f);

            pieUse.noBillboardMat = AssetMan.Get<Material>("NoBillboardMaterial");

            GameObject.DestroyImmediate(gumClone);

            AssetMan.Add("PieItem", pie);


            // Door Key
            ItemObject keyItemObject = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_DoorKey", "Desc_RecChars_DoorKey")
            .SetEnum("RecChars_DoorKey")
            .SetMeta(ItemFlags.MultipleUse, ["key", "crmp_contraband"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareItm/DoorKey_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareItm/DoorKey_Large"), 50f))
            .SetShopPrice(750)
            .SetGeneratorCost(90)
            .SetItemComponent<ITM_DoorKey>()
            .BuildAsMulti(3);
            ((ITM_DoorKey)keyItemObject.item).layerMask = ((ITM_Acceptable)ItemMetaStorage.Instance.FindByEnum(Items.DetentionKey).value.item).layerMask;

            AssetMan.Add("DoorKeyItem", keyItemObject);
        }

        private void LoadMrDaycare()
        {
            MrDaycare daycare = new NPCBuilder<MrDaycare>(Plugin)
                .SetName("MrDaycare")
                .SetEnum("RecChars_MrDaycare")
                .SetPoster(AssetMan.Get<Texture2D>("DaycareTex/pri_daycare"), "PST_PRI_RecChars_Daycare1", "PST_PRI_RecChars_Daycare2")
                .AddMetaFlag(NPCFlags.Standard | NPCFlags.MakeNoise)
                .SetMetaTags(["faculty", "no_balloon_frenzy"])
                .AddPotentialRoomAssets(CreateDaycareRooms())
                .AddLooker()
                .AddTrigger()
                .AddHeatmap()
                .SetWanderEnterRooms()
                .IgnorePlayerOnSpawn()
                .Build();

            MrDaycare.charEnum = daycare.character;

            daycare.spriteRenderer[0].transform.localPosition = Vector3.up * -1f;
            daycare.spriteRenderer[0].sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareTex/MrDaycare"), 65f);

            daycare.audMan = daycare.GetComponent<AudioManager>();
            daycare.audMan.subtitleColor = new(192/255f, 242/255f, 75/255f);

            daycare.audDetention = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_Timeout"), "Vfx_RecChars_Daycare_Timeout", SoundType.Voice, daycare.audMan.subtitleColor);
            daycare.audSeconds = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_Seconds"), "Vfx_RecChars_Daycare_Seconds", SoundType.Voice, daycare.audMan.subtitleColor);

            daycare.audTimes =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_30"), "Vfx_RecChars_Daycare_30", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_60"), "Vfx_RecChars_Daycare_60", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_100"), "Vfx_RecChars_Daycare_100", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_200"), "Vfx_RecChars_Daycare_200", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_500"), "Vfx_RecChars_Daycare_500", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_3161"), "Vfx_RecChars_Daycare_3161", SoundType.Voice, daycare.audMan.subtitleColor)
            ];

            daycare.audNoRunning = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoRunning"), "Vfx_RecChars_Daycare_NoRunning", SoundType.Voice, daycare.audMan.subtitleColor);
            daycare.audNoDrinking = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoDrinking"), "Vfx_RecChars_Daycare_NoDrinking", SoundType.Voice, daycare.audMan.subtitleColor);
            daycare.audNoEating = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoEating"), "Vfx_RecChars_Daycare_NoEating", SoundType.Voice, daycare.audMan.subtitleColor);
            daycare.audNoEscaping = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoEscaping"), "Vfx_RecChars_Daycare_NoEscaping", SoundType.Voice, daycare.audMan.subtitleColor);

            MrDaycare.audRuleBreaks = new Dictionary<string, SoundObject>()
            {
                { "Running" , daycare.audNoRunning},
                { "Drinking" , daycare.audNoDrinking},
                { "Eating" , daycare.audNoEating},
                { "DaycareEscaping" , daycare.audNoEscaping},
                { "Throwing" , ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoThrowing"), "Vfx_RecChars_Daycare_NoThrowing", SoundType.Voice, daycare.audMan.subtitleColor)},
                { "LoudSound" , ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoLoudSound"), "Vfx_RecChars_Daycare_NoLoudSound", SoundType.Voice, daycare.audMan.subtitleColor)}
            };


            // Set all these lines to silence (in the event another mod calls a function from base principal)
            daycare.audComing = Resources.FindObjectsOfTypeAll<SoundObject>().First(x => x.name == "Silence" && x.GetInstanceID() >= 0);
            daycare.audWhistle = daycare.audComing;
            daycare.audNoAfterHours = daycare.audComing;
            daycare.audNoFaculty = daycare.audComing;
            daycare.audNoBullying = daycare.audComing;
            daycare.audNoLockers = daycare.audComing;
            daycare.audNoStabbing = daycare.audComing;
            daycare.audScolds = [daycare.audComing];

            daycare.whistleChance = 0;
            daycare.detentionNoise = 125;

            Principal principle = (Principal)NPCMetaStorage.Instance.Get(Character.Principal).value;
            daycare.Navigator.accel = principle.Navigator.accel;

            daycare.Navigator.speed = 36f;
            daycare.Navigator.maxSpeed = 36f;
            daycare.Navigator.passableObstacles = principle.Navigator.passableObstacles;
            daycare.Navigator.preciseTarget = principle.Navigator.preciseTarget;

            MrDaycare unnerfedDaycare = GameObject.Instantiate(daycare, MTM101BaldiDevAPI.prefabTransform);
            unnerfedDaycare.name = "MrDaycare Unnerfed";
            ObjMan.Add("Npc_MrDaycare_Unnerfed", unnerfedDaycare);

            daycare.Navigator.speed = 30f;
            daycare.Navigator.maxSpeed = 30f;
            daycare.maxTimeoutLevel = 1;
            daycare.ruleSensitivityMul = 1;
            ObjMan.Add("Npc_MrDaycare_Nerfed", daycare);

            PineDebugNpcIconPatch.icons.Add(daycare.Character, AssetMan.Get<Texture2D>("DaycareTex/BorderDaycare"));
            CharacterRadarColorPatch.colors.Add(daycare.Character, daycare.audMan.subtitleColor);

            ObjMan.Add("Npc_MrDaycare", RecommendedCharsConfig.nerfMrDaycare.Value ? daycare : unnerfedDaycare);
        }

        private void CreateDaycareBlueprint()
        {
            CaudexRoomBlueprint daycareRoom = new(Plugin, "Daycare", "RecChars_Daycare");

            DaycareDoorAssets.template = ObjectCreators.CreateDoorDataObject("DaycareDoor", AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Open"), AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Shut"));
            DaycareDoorAssets.locked = ObjectCreators.CreateDoorDataObject("DaycareDoor", AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Open"), AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Locked"));

            DaycareDoorAssets.mask = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<StandardDoor>().First().mask[0]);
            DaycareDoorAssets.mask.name = "DaycareDoor_Mask";
            DaycareDoorAssets.mask.SetMaskTexture(AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Mask"));

            DaycareDoorAssets.open = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "Doors_DaveOpen.wav"), "Sfx_Doors_StandardOpen", SoundType.Effect, Color.white);
            DaycareDoorAssets.shut = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "Doors_DaveShut.wav"), "Sfx_Doors_StandardShut", SoundType.Effect, Color.white);
            DaycareDoorAssets.unlock = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "Doors_DaveUnlock.wav"), "Sfx_Doors_StandardUnlock", SoundType.Effect, Color.white);

            daycareRoom.texFloor = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Floor");
            daycareRoom.texWall = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Wall");
            daycareRoom.texCeil = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Ceiling");
            daycareRoom.keepTextures = true;

            daycareRoom.doorMats = DaycareDoorAssets.template;

            daycareRoom.lightObj = GameObject.Instantiate(((DrReflex)NPCMetaStorage.Instance.Get(Character.DrReflex).value).potentialRoomAssets[0].selection.lightPre, MTM101BaldiDevAPI.prefabTransform);
            daycareRoom.lightObj.name = "DaycareLight";
            MeshRenderer light = daycareRoom.lightObj.GetComponentInChildren<MeshRenderer>();
            light.sharedMaterial = new Material(light.sharedMaterial)
            {
                name = "Daycare_CeilingLight",
                mainTexture = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_CeilingLight")
            };

            daycareRoom.mapMaterial = ObjectCreators.CreateMapTileShader(AssetMan.Get<Texture2D>("DaycareRoom/Map_Daycare"));
            daycareRoom.color = Color.green;

            daycareRoom.posterChance = 0.1f;

            daycareRoom.windowSet = ObjectCreators.CreateWindowObject("Daycare_Window", AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow"), AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow_Broken"), AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow_Mask"));
            daycareRoom.windowSet.mask.name = "DaycareWindow_Mask";
            daycareRoom.windowChance = 0.35f;

            DetentionRoomFunction detRoomFunction = Resources.FindObjectsOfTypeAll<DetentionRoomFunction>().First(x => x.name == "OfficeRoomFunction" && x.GetInstanceID() >= 0);
            DaycareRoomFunction dcRoomFunction = RecommendedCharsPlugin.SwapComponentSimple<DetentionRoomFunction, DaycareRoomFunction>(GameObject.Instantiate(detRoomFunction, MTM101BaldiDevAPI.prefabTransform));
            dcRoomFunction.name = "DaycareRoomFunction";
            dcRoomFunction.gaugeSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareTex/TimeoutIcon"), 25f);
            dcRoomFunction.animationsCompat = false;
            AssetMan.Add("DaycareRoomFunction", dcRoomFunction);

            daycareRoom.functionContainer = dcRoomFunction.GetComponent<RoomFunctionContainer>();
            GameObject.DestroyImmediate(daycareRoom.functionContainer.GetComponent<CharacterPostersRoomFunction>());
            GameObject.DestroyImmediate(daycareRoom.functionContainer.GetComponent<RuleFreeZone>());

            DaycareRuleFreeZone ruleFreeZone = daycareRoom.functionContainer.gameObject.AddComponent<DaycareRuleFreeZone>();
            ruleFreeZone.excludeEscaping = false;

            daycareRoom.functionContainer.functions =
            [
                dcRoomFunction,
                ruleFreeZone,
                daycareRoom.functionContainer.GetComponent<CoverRoomFunction>()
            ];

            AssetMan.Add("DaycareBlueprint", daycareRoom);
        }

        private WeightedRoomAsset[] CreateDaycareRooms()
        {
            CaudexRoomBlueprint blueprint = AssetMan.Get<CaudexRoomBlueprint>("DaycareBlueprint");

            List<WeightedRoomAsset> rooms = [];

            RoomAsset newRoom = blueprint.CreateAsset("Sugary0");
            rooms.Add(newRoom.Weighted(100));

            newRoom.cells = RoomAssetHelper.CellRect(3, 3);
            newRoom.posterDatas =
            [
                RoomAssetHelper.PosterData(0,1,AssetMan.Get<PosterObject>("DaycareInfoPoster"),Direction.West),
                RoomAssetHelper.PosterData(1,2,AssetMan.Get<PosterObject>("DaycareClockPoster"),Direction.North),
                RoomAssetHelper.PosterData(2,1,AssetMan.Get<PosterObject>("DaycareRulesPoster"),Direction.East)
            ];
            newRoom.standardLightCells = [new(1, 1)];
            newRoom.potentialDoorPositions =
            [
                new(0,0),
                new(1,0),
                new(2,0),
                new(0,2),
                new(2,2)
            ];
            newRoom.entitySafeCells =
            [
                new(1,0),
                new(1,1),
                new(2,1)
            ];
            return [.. rooms];
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Post)]
        private void EnableAnimationsCompat()
        {
            AssetMan.Get<DaycareRoomFunction>("DaycareRoomFunction").animationsCompat = true;
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelLoaderGuid, LoadingEventOrder.Pre)]
        private void RegisterToLevelLoader()
        {
            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_mrdaycare", ObjMan.Get<MrDaycare>("Npc_MrDaycare_Nerfed"));
            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_mrdaycare_og", ObjMan.Get<MrDaycare>("Npc_MrDaycare_Unnerfed"));

            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_pie", AssetMan.Get<ItemObject>("PieItem"));
            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_doorkey", AssetMan.Get<ItemObject>("DoorKeyItem"));

            CaudexRoomBlueprint blueprint = AssetMan.Get<CaudexRoomBlueprint>("DaycareBlueprint");
            LevelLoaderCompatHelper.AddRoom(blueprint);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_daycareflor", blueprint.texFloor);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_daycarewall", blueprint.texWall);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_daycareceil", blueprint.texCeil);
            LevelLoaderPlugin.Instance.windowObjects.Add("recchars_daycare", blueprint.windowSet);
            LevelLoaderPlugin.Instance.lightTransforms.Add("recchars_daycare", blueprint.lightObj);

            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_daycare", ObjMan.Get<MrDaycare>("Npc_MrDaycare").Poster);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_daycareinfo", AssetMan.Get<PosterObject>("DaycareInfoPoster"));
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_daycarerules", AssetMan.Get<PosterObject>("DaycareRulesPoster"));
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_daycareclock", AssetMan.Get<PosterObject>("DaycareClockPoster"));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Pre)]
        private void AdvancedCompat()
        {
            // Make Mr. Daycare scold the player for using throwing/shooting items
            BepInEx.PluginInfo advInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.AdvancedGuid];
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("MysteriousTeleporter"), advInfo).tags.Add("recchars_daycare_throwable");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("TeleportationBomb"), advInfo).tags.Add("recchars_daycare_throwable");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("CookedChickenLeg"), advInfo).tags.Add("recchars_daycare_exempt");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("RawChickenLeg"), advInfo).tags.Add("recchars_daycare_exempt");

            // Add new words and tips
            ApiManager.AddNewSymbolMachineWords(Plugin, "Moldy", "Dave", "house");
            ApiManager.AddNewTips(Plugin, "Adv_Elv_Tip_RecChars_Pie", "Adv_Elv_Tip_RecChars_DoorKey",
                "Adv_Elv_Tip_RecChars_MrDaycareExceptions", "Adv_Elv_Tip_RecChars_MrDaycareEarly");
        }

        private bool _connectorActive = false;
        private bool _connectorErrored = false;

        [CaudexLoadEventMod(RecommendedCharsPlugin.ConnectorGuid, LoadingEventOrder.Pre)]
        private void CheckIfConnectorIsActive()
        {
            if (ConnectorBasicsPlugin.Connected)
                _connectorActive = true;
            else
                ConnectorError();
        }

        private void ConnectorError()
        {
            if (!_connectorErrored)
                RecommendedCharsPlugin.Log.LogError("Thinker API Connector wasn't loaded properly! Make sure you either reinstall the connector or disable Assembly Cache in BepInEx's config!");
            _connectorErrored = true;
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.FragileWindowsGuid, LoadingEventOrder.Pre)]
        private void FragileWindowsCompat()
        {
            // Dave Windowlet >u<
            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2,2,256,Vector2.one/2f,AssetLoader.TextureFromMod(BasePlugin, "Textures", "Npc", "Compat", "DaveWindowlet.png"));
            FragileWindowsCompatHelper.AddWindowlet<DaveWindowlet>("Dave", sprites[0], sprites[3], new(81/255f, 38/255f, 10/255f), 3);
            DaveWindowlet.sprLo = sprites[1];
            DaveWindowlet.sprHi = sprites[2];

            // Make Mr. Daycare scold the player for using throwing/shooting items
            if (!_connectorActive)
            {
                ConnectorError();
                return;
            }
            BepInEx.PluginInfo fragileInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.FragileWindowsGuid];
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("Stone"), fragileInfo).tags.Add("recchars_daycare_throwable");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("ShardSoda"), fragileInfo).tags.Add("recchars_daycare_throwable");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("Marble"), fragileInfo).tags.Add("recchars_daycare_throwable");
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.EcoFriendlyGuid, LoadingEventOrder.Pre)]
        private void EcoFriendlyCompat()
        {
            // Make Mr. Daycare scold the player for using throwing/shooting items
            if (!_connectorActive)
            {
                ConnectorError();
                return;
            }
            BepInEx.PluginInfo ecoInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.EcoFriendlyGuid];
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("Wrench"), ecoInfo).tags.Add("recchars_daycare_throwable");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("Sibling"), ecoInfo).tags.Add("recchars_daycare_throwable");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("OblongSlotNameThingy"), ecoInfo).tags.Add("recchars_daycare_throwable");
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.CrazyBabyGuid, LoadingEventOrder.Pre)]
        private void CrazyBabyCompat()
        {
            // Make Mr. Daycare scold the player for using throwing/shooting items
            if (!_connectorActive)
            {
                ConnectorError();
                return;
            }
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("BabyEye"), Chainloader.PluginInfos[RecommendedCharsPlugin.CrazyBabyGuid])
                .tags.Add("recchars_daycare_throwable");
        }

        [CaudexLoadEvent(LoadingEventOrder.Post)]
        private void PostLoad()
        {
            // Add Mr. Daycare Rule Free Zones to everything except the Principal's office
            RuleFreeZone[] zones = Resources.FindObjectsOfTypeAll<RuleFreeZone>();
            RoomFunctionContainer container;
            foreach (RuleFreeZone ruleFreeZone in zones)
            {
                if (!ruleFreeZone.TryGetComponent(out container))
                    continue;

                if (container.functions.FirstOrDefault(x => x is DaycareRuleFreeZone) == null && // Does not already have a DaycareRuleFreeZone
                    container.functions.FirstOrDefault(x => x is DetentionRoomFunction) == null) // Is not a Principal's office
                    container.functions.Add(container.gameObject.AddComponent<DaycareRuleFreeZone>()); // Make Mr. Daycare ignore rule breaks
            }

            List<Items> keyItems =
            [
                Items.DetentionKey
            ];
            foreach (ItemMetaData meta in ItemMetaStorage.Instance.FindAllWithTags(false, "shape_key"))
            {
                if (!keyItems.Contains(meta.id))
                    keyItems.Add(meta.id);
            }
            ITM_DoorKey.keyEnums = [.. keyItems];
        }

        private PosterObject CreatePoster(string path, string name)
        {
            PosterObject poster = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>(path), []);
            poster.name = name;
            return poster;
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("PieItem").Weighted(50));
                scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("DoorKeyItem").Weighted(25));

                if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<MrDaycare>("Npc_MrDaycare"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
                else
                    scene.potentialNPCs.CopyNpcWeight(Character.Beans, ObjMan.Get<MrDaycare>("Npc_MrDaycare"));
                return;
            }

            if (title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("PieItem").Weighted(50));
                if (id > 0)
                    scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("DoorKeyItem").Weighted(25));

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.potentialNPCs.CopyNpcWeight(Character.Beans, ObjMan.Get<MrDaycare>("Npc_MrDaycare"));
                }
                else if (id == 0)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<MrDaycare>("Npc_MrDaycare"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
            }
        }

        private void FloorAddendLvl(string title, int id, LevelObject lvl)
        {
            if (title == "END" || title.StartsWith("F"))
            {
                lvl.potentialItems = lvl.potentialItems.AddToArray(AssetMan.Get<ItemObject>("PieItem").Weighted(25));
                if (title == "END" || id > 0)
                    lvl.potentialItems = lvl.potentialItems.AddToArray(AssetMan.Get<ItemObject>("DoorKeyItem").Weighted(10));
                return;
            }
        }

        private void AddPosterToLevel(LevelGenerator gen)
        {
            if (gen.scene == null) return;
            if (gen.Ec.npcsToSpawn.FirstOrDefault(x => x != null && x.Character == MrDaycare.charEnum) == null) return;

            gen.ld.posters = gen.ld.posters.AddToArray(AssetMan.Get<PosterObject>("DaycareRulesPoster").Weighted(50));
        }

        private static void SetGuiltForItems(ItemManager itemMan, ItemObject itm)
        {
            ItemMetaData meta = itm.GetMeta();
            if (meta == null || meta.tags.Contains("recchars_daycare_exempt")) return;

            if (meta.tags.Contains("food") && !meta.tags.Contains("drink"))
            {
                DaycareGuiltManager.GetInstance(itemMan.pm).BreakRule("Eating", 0.8f, 0.25f);
                return;
            }
            if (meta.tags.Contains("recchars_daycare_throwable"))
            {
                DaycareGuiltManager.GetInstance(itemMan.pm).BreakRule("Throwing", 0.8f, 0.25f);
                return;
            }
            if (meta.tags.Contains("recchars_daycare_loud") &&
                !itemMan.pm.ec.silent &&
                !itemMan.pm.ec.CellFromPosition(IntVector2.GetGridPosition(itemMan.transform.position)).Silent)
            {
                DaycareGuiltManager.GetInstance(itemMan.pm).BreakRule("LoudSound", 1.5f, 0.5f);
                return;
            }
        }
    }
}
