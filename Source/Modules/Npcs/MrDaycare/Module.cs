using BaldisBasicsPlusAdvanced.API;
using BepInEx.Bootstrap;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;

using PlusStudioLevelLoader;

using System.Collections.Generic;
using System.Linq;

using TMPro;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Registers;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Mr. Daycare"), CaudexModuleSaveTag("Mdl_MrDaycare")]
    [CaudexModuleConfig("Modules", "MrDaycare",
        "Adds Mr. Daycare from Dave's House.", true)]
    public sealed class Module_MrDaycare : RecCharsModule
    {
        internal override byte IconId => 4;

        protected override void Initialized()
        {
            // Load texture and audio assets
            ObjectCreation.AddTexturesToAssetManWLegacy("DaycareTex/", ["Textures", "Npc", "Daycare"]);
            ObjectCreation.AddTexturesToAssetManWLegacy("DaycareRoom/", ["Textures", "Environment", "Room", "Daycare"]);
            ObjectCreation.AddTexturesToAssetMan("DaycarePoster/", ["Textures", "Environment", "Poster", "Daycare"]);
            ObjectCreation.AddAudioToAssetMan("DaycareAud/", ["Audio", "Npc", "Daycare"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "MrDaycare.json5");

            // Load patches
            Hooks.PatchAll(typeof(MrDaycarePatches));
            Hooks.PatchAll(typeof(DaycareRoomPatches));
            RecommendedCharsPlugin.PatchCompat(typeof(MrDaycareAdvancedPatches), RecommendedCharsPlugin.AdvancedGuid);
            //RecommendedCharsPlugin.PatchCompat(typeof(MrDaycareEcoFriendlyPatches), RecommendedCharsPlugin.EcoFriendlyGuid);
            //RecommendedCharsPlugin.PatchCompat(typeof(MrDaycareFragilePatches), RecommendedCharsPlugin.FragileWindowsGuid);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            // Create posters
            ObjectCreation.CreatePoster("DaycarePoster/pst_daycarerules", "DaycarePoster_Rules", "daycarerules", new PosterTextData() {
                color = Color.black,
                textKey = "PST_RecChars_DaycareRules",
                font = BaldiFonts.ComicSans12.FontAsset(),
                fontSize = 12,
                alignment = TextAlignmentOptions.Center,
                position = new(55,31),
                size = new(145,196)
            });
            ObjectCreation.CreatePoster("DaycarePoster/pst_daycareinfo", "DaycarePoster_Info", "daycareinfo", new PosterTextData() {
                color = Color.black,
                textKey = "PST_RecChars_DaycareInfo1",
                font = BaldiFonts.ComicSans18.FontAsset(),
                fontSize = 18,
                alignment = TextAlignmentOptions.Center,
                position = new(57,177),
                size = new(140,48)
            }, new PosterTextData() {
                color = Color.black,
                textKey = "PST_RecChars_DaycareInfo2",
                font = BaldiFonts.ComicSans12.FontAsset(),
                fontSize = 12,
                alignment = TextAlignmentOptions.Center,
                position = new(57,37),
                size = new(140,144)
            });
            ObjectCreation.CreatePoster("DaycarePoster/pst_daycareclock", "DaycarePoster_Clock", "daycareclock");

            CreateDaycareBlueprint();
            LoadMrDaycare();

            CaudexGeneratorEvents.AddAction(CaudexGeneratorEventType.NpcPrep, AddPosterToLevel);

            CaudexEvents.OnItemUse += (im, itm) => {
                ItemMetaData meta = itm.GetMeta();
                if (meta == null || meta.tags.Contains("recchars:daycare_exempt")) return;

                if (meta.tags.Contains("food") && !meta.tags.Contains("drink"))
                {
                    DaycareGuiltManager.GetInstance(im.pm).BreakRule("Eating", 0.8f, 0.25f);
                    return;
                }
                if (meta.tags.Contains("recchars:daycare_throwable"))
                {
                    DaycareGuiltManager.GetInstance(im.pm).BreakRule("Throwing", 0.8f, 0.25f);
                    return;
                }
                if (meta.tags.Contains("recchars:daycare_loud") &&
                    !im.pm.ec.silent &&
                    !im.pm.ec.CellFromPosition(IntVector2.GetGridPosition(im.transform.position)).Silent)
                {
                    DaycareGuiltManager.GetInstance(im.pm).BreakRule("LoudSound", 1.5f, 0.5f);
                    return;
                }
            };
        }
        private void LoadMrDaycare()
        {
            MrDaycare daycare = new NPCBuilder<MrDaycare>(Plugin)
                .SetName("MrDaycare")
                .SetEnum("RecChars_MrDaycare")
                .SetPoster(AssetMan.Get<Texture2D>("DaycareTex/pri_daycare"), "PST_PRI_RecChars_Daycare1", "PST_PRI_RecChars_Daycare2")
                .AddMetaFlag(NPCFlags.Standard | NPCFlags.MakeNoise)
                .SetMetaTags(["faculty", "no_balloon_frenzy", "ignoreGabby", "RebProtection"])
                .AddLooker()
                .AddTrigger()
                .AddHeatmap()
                .SetWanderEnterRooms()
                .IgnorePlayerOnSpawn()
                .Build();

            MrDaycare.charEnum = daycare.character;

            Sprite[] sprites;
            if (RecommendedCharsPlugin.PartyMode)
                sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 65f, new Vector2(0.5f, 0.4f), AssetMan.Get<Texture2D>("DaycareTex/MrDaycare_Party"));
            else
                sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 65f, Vector2.one/2f, AssetMan.Get<Texture2D>("DaycareTex/MrDaycare"));

            daycare.spriteRenderer[0].transform.localPosition = Vector3.up * -1f;
            daycare.normalSprite = sprites[0];
            daycare.chasingSprite = sprites[1];
            daycare.spriteRenderer[0].sprite = daycare.normalSprite;

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
            daycare.audComing = AssetMan.Get<SoundObject>("Sfx/Silence");
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

            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_daycare", daycare.Poster);
            daycare.potentialRoomAssets = ObjectCreation.RoomAssetsFromDirectory(ObjMan.Get<CaudexRoomBlueprint>("Room/Daycare"), "Daycare");

            MrDaycare unnerfedDaycare = GameObject.Instantiate(daycare, MTM101BaldiDevAPI.prefabTransform);
            unnerfedDaycare.name = "MrDaycare Unnerfed";
            ObjMan.Add("Npc/MrDaycare_Unnerfed", unnerfedDaycare);

            daycare.Navigator.speed = 30f;
            daycare.Navigator.maxSpeed = 30f;
            daycare.maxTimeoutLevel = 1;
            daycare.ruleSensitivityMul = 1;
            ObjMan.Add("Npc/MrDaycare_Nerfed", daycare);

            PineDebugNpcIcons.AddIcon([daycare, unnerfedDaycare], "BorderDaycare.png");
            CharacterRadarColorPatch.colors.Add(daycare.Character, daycare.audMan.subtitleColor);

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_mrdaycare", daycare);
            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_mrdaycare_og", unnerfedDaycare);
            SetMrDaycarePrefab();
            SurpriseNpc.AddVisual(new SurpriseNpcVisualSprite(daycare));
            RecommendedCharsConfig.nerfMrDaycare.SettingChanged += (x, y) =>
            {
                SetMrDaycarePrefab();
                UpdateMrDaycareInstances();
            };
        }

        private void CreateDaycareBlueprint()
        {
            CaudexRoomBlueprint daycareRoom = new(Plugin, "Daycare", "RecChars_Daycare");

            DaycareDoorAssets.template = ObjectCreators.CreateDoorDataObject("DaycareDoor", AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Open"), AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Shut"));
            DaycareDoorAssets.locked = ObjectCreators.CreateDoorDataObject("DaycareDoor_Locked", AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Open"), AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Locked"));

            StandardDoor door = AssetFinder.FindAllOfType<StandardDoor>(true).First();
            DaycareDoorAssets.mask = new(door.mask[0]);
            DaycareDoorAssets.mask.name = "DaycareDoor_Mask";
            DaycareDoorAssets.mask.SetMaskTexture(AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Mask"));

            DaycareDoorAssets.open = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "Doors_DaveOpen.wav"), "Sfx_Doors_StandardOpen", SoundType.Effect, Color.white);
            DaycareDoorAssets.shut = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "Doors_DaveShut.wav"), "Sfx_Doors_StandardShut", SoundType.Effect, Color.white);
            DaycareDoorAssets.unlock = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "Doors_DaveUnlock.wav"), "Sfx_Doors_StandardUnlock", SoundType.Effect, Color.white);

            daycareRoom.texFloor = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Floor");
            daycareRoom.texWall = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Wall");
            daycareRoom.texCeil = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Ceiling");
            daycareRoom.keepTextures = true;

            daycareRoom.doorMats = ObjectCreators.CreateDoorDataObject("DaycareStandardDoor", AssetMan.Get<Texture2D>("DaycareRoom/DaveStandardDoor_Open"), AssetMan.Get<Texture2D>("DaycareRoom/DaveStandardDoor_Shut"));

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

            daycareRoom.windowSet = ObjectCreators.CreateWindowObject("Daycare_Window",
                AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow"),
                AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow_Broken"),
                AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow_Mask"));
            daycareRoom.windowSet.mask.name = "DaycareWindow_Mask";
            daycareRoom.windowChance = 0.35f;

            DaycareNotebookDoor daycareDoor = GameObject.Instantiate(door, MTM101BaldiDevAPI.prefabTransform).SwapComponentSimple<StandardDoor, DaycareNotebookDoor>();
            daycareDoor.name = "DaycareNotebookDoor";
            daycareDoor.mask[0] = DaycareDoorAssets.mask;
            daycareDoor.mask[1] = DaycareDoorAssets.mask;
            daycareDoor.audDoorOpen = DaycareDoorAssets.open;
            daycareDoor.audDoorShut = DaycareDoorAssets.shut;
            daycareDoor.audDoorUnlock = DaycareDoorAssets.unlock;
            LevelLoaderPlugin.Instance.doorPrefabs.Add("recchars_bookgate", daycareDoor);
            ObjMan.Add("Door_Daycare", daycareDoor);

            DetentionRoomFunction detRoomFunction = AssetFinder.FindOfTypeWithName<DetentionRoomFunction>("OfficeRoomFunction", true);
            detRoomFunction = GameObject.Instantiate(detRoomFunction, MTM101BaldiDevAPI.prefabTransform);
            detRoomFunction.name = "DaycareRoomFunction";

            daycareRoom.functionContainer = detRoomFunction.GetComponent<RoomFunctionContainer>();

            GameObject.DestroyImmediate(daycareRoom.functionContainer.GetComponent<CharacterPostersRoomFunction>());
            GameObject.DestroyImmediate(daycareRoom.functionContainer.GetComponent<RuleFreeZone>());
            GameObject.DestroyImmediate(daycareRoom.functionContainer.GetComponent<CoverRoomFunction>()); // this won't house character posters so this is irrelevant

            daycareRoom.functionContainer.functions.Clear();
            daycareRoom.functionContainer.AddDoorAssigner(daycareDoor);
            daycareRoom.functionContainer.AddFunction<MrDaycareHolderFunction>();
            
            DaycareTimeoutRoomFunction dcRoomFunction = detRoomFunction.SwapComponentSimple<DetentionRoomFunction, DaycareTimeoutRoomFunction>();
            dcRoomFunction.gaugeSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareTex/TimeoutIcon"), 25f);

            daycareRoom.functionContainer.AddFunction(dcRoomFunction);

            DaycareRuleFreeZone ruleFreeZone = daycareRoom.functionContainer.AddFunction<DaycareRuleFreeZone>();
            ruleFreeZone.excludeEscaping = false;

            // Randomly place Info, Rules and Clock posters for rooms in generated levels
            daycareRoom.functionContainer.AddChalkboardBuilder(ObjMan.Get<PosterObject>("Pst/DaycarePoster_Info"));
            daycareRoom.functionContainer.AddChalkboardBuilder(ObjMan.Get<PosterObject>("Pst/DaycarePoster_Rules"));
            daycareRoom.functionContainer.AddChalkboardBuilder(ObjMan.Get<PosterObject>("Pst/DaycarePoster_Clock"));

            ObjectCreation.AddRoom(daycareRoom);

            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_daycareflor", daycareRoom.texFloor);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_daycarewall", daycareRoom.texWall);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_daycareceil", daycareRoom.texCeil);
            LevelLoaderPlugin.Instance.windowObjects.Add("recchars_daycare", daycareRoom.windowSet);
            LevelLoaderPlugin.Instance.lightTransforms.Add("recchars_daycare", daycareRoom.lightObj);
            ObjMan.Add("Room/Daycare", daycareRoom);
        }

        /*[CaudexLoadEventMod(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Post)]
        private void EnableAnimationsCompat()
            => GameObject.DestroyImmediate(ObjMan.Get<DaycareNotebookDoor>("Door_Daycare").GetComponent<StandardDoorExtraMaterials>());*/

        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Pre)]
        private void AdvancedCompat()
        {
            // Make Mr. Daycare scold the player for using throwing/shooting items
            BepInEx.PluginInfo advInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.AdvancedGuid];
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("MysteriousTeleporter"), advInfo).tags.Add("recchars:daycare_throwable");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("TeleportationBomb"), advInfo).tags.Add("recchars:daycare_throwable");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("CookedChickenLeg"), advInfo).tags.Add("recchars:daycare_exempt");
            ItemMetaStorage.Instance.FindByEnumFromMod(EnumExtensions.GetFromExtendedName<Items>("RawChickenLeg"), advInfo).tags.Add("recchars:daycare_exempt");

            // Add new words and tips
            ApiManager.AddNewSymbolMachineWords(Plugin, "Muko", "Dave", "Bambi", "house");
            ApiManager.AddNewTips(Plugin, "Adv_Elv_Tip_RecChars_Pie", "Adv_Elv_Tip_RecChars_DoorKey",
                "Adv_Elv_Tip_RecChars_MrDaycareExceptions", "Adv_Elv_Tip_RecChars_MrDaycareEarly");
        }

        // Thinker's mods haven't been updated for like ever so unless that actually updates then 
        /*[CaudexLoadEventMod(RecommendedCharsPlugin.FragileWindowsGuid, LoadingEventOrder.Pre)]
        private void FragileWindowsCompat()
        {
            // Dave Windowlet >u<
            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2,2,256,Vector2.one/2f,AssetLoader.TextureFromMod(BasePlugin, "Textures", "Compat", "FragileWindows", "DaveWindowlet.png"));
            FragileWindowsCompatHelper.AddWindowlet<DaveWindowlet>("Dave", sprites[0], sprites[3], new(81/255f, 38/255f, 10/255f), 3);
            DaveWindowlet.sprLo = sprites[1];
            DaveWindowlet.sprHi = sprites[2];

            // Make Mr. Daycare scold the player for using throwing/shooting items
            BepInEx.PluginInfo fragileInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.FragileWindowsGuid];
            foreach (ItemMetaData meta in ItemMetaStorage.Instance.GetAllFromMod(fragileInfo))
            {
                switch (meta.value.itemType.ToStringExtended())
                {
                case "ShardSoda":
                    meta.tags.Add("food");
                    meta.tags.Add("drink");
                    meta.tags.Add("recchars:daycare_throwable");
                    break;
                case "Marble":
                    meta.tags.Add("caudex:always_trigger_event");
                    meta.tags.Add("recchars:daycare_throwable");
                    break;
                case "Stone":
                case "WindowBlaster1":
                case "WindowBlaster2":
                case "WindowBlaster3":
                case "WindowBlaster4":
                case "WindowBlaster5":
                    meta.tags.Add("recchars:daycare_throwable");
                    break;
                }
            }
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.EcoFriendlyGuid, LoadingEventOrder.Pre)]
        private void EcoFriendlyCompat()
        {
            // Make Mr. Daycare scold the player for using throwing/shooting items
            BepInEx.PluginInfo ecoInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.EcoFriendlyGuid];
            foreach (ItemMetaData meta in ItemMetaStorage.Instance.GetAllFromMod(ecoInfo))
            {
                switch (meta.value.itemType.ToStringExtended())
                {
                case "Wrench":
                case "Sibling":
                case "OblongSlotNameThingy":
                case "BBGun_1":
                case "BBGun_2":
                case "BBGun_3":
                case "BBGun_4":
                    meta.tags.Add("recchars:daycare_throwable");
                    break;
                }
            }
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.CrazyBabyGuid, LoadingEventOrder.Pre)]
        private void CrazyBabyCompat()
        {
            // Make Mr. Daycare scold the player for using throwing/shooting items
            BepInEx.PluginInfo crazyBabyInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.CrazyBabyGuid];
            foreach (ItemMetaData meta in ItemMetaStorage.Instance.GetAllFromMod(crazyBabyInfo))
            {
                switch (meta.value.itemType.ToStringExtended())
                {
                case "BabyEye":
                case "DonutGun6":
                case "DonutGun5":
                case "DonutGun4":
                case "DonutGun3":
                case "DonutGun2":
                case "DonutGun1":
                    meta.tags.Add("recchars:daycare_throwable");
                    break;
                }
            }
        }*/

        [CaudexLoadEvent(LoadingEventOrder.Post)]
        private void UpdateRuleFreeZones()
        {
            // Add Mr. Daycare Rule Free Zones to everything except the Principal's office
            RuleFreeZone[] zones = Resources.FindObjectsOfTypeAll<RuleFreeZone>();
            RoomFunctionContainer container;
            foreach (RuleFreeZone ruleFreeZone in zones)
            {
                if (!ruleFreeZone.TryGetComponent(out container))
                    continue;

                if (!container.functions.FirstOrDefault(x => x is DaycareRuleFreeZone) && // Does not already have a DaycareRuleFreeZone
                    !container.functions.FirstOrDefault(x => x is DetentionRoomFunction)) // Is not a Principal's office
                    container.AddFunction<DaycareRuleFreeZone>(); // Make Mr. Daycare ignore rule breaks
            }
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true)
            {
                scene.MarkAsNeverUnload();
                if (RecommendedCharsConfig.guaranteeSpawnChar)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<MrDaycare>("Npc/MrDaycare"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
                else
                    scene.potentialNPCs.CopyNpcWeight(Character.Beans, ObjMan.Get<MrDaycare>("Npc/MrDaycare"));
                return;
            }
            if (title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();
                if (!RecommendedCharsConfig.guaranteeSpawnChar)
                {
                    scene.potentialNPCs.CopyNpcWeight(Character.Beans, ObjMan.Get<MrDaycare>("Npc/MrDaycare"));
                }
                else if (id == 0)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<MrDaycare>("Npc/MrDaycare"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
            }
        }

        private void SetMrDaycarePrefab()
        {
            ObjMan.Add("Npc/MrDaycare", ObjMan.Get<MrDaycare>(RecommendedCharsConfig.nerfMrDaycare.Value ? "Npc/MrDaycare_Nerfed" : "Npc/MrDaycare_Unnerfed"));
            //NPCMetaStorage.Instance.Get(MrDaycare.charEnum).ReflectionSetVariable("defaultKey", ObjMan.Get<MrDaycare>("Npc/MrDaycare").name);
        }

        private void UpdateMrDaycareInstances()
        {
            SceneObject scene;
            int i, c;
            foreach (SceneObjectMetadata sceneMeta in SceneObjectMetaStorage.Instance.All().Where(x => x.tags.Contains("endless") == true || x.title.StartsWith("F")))
            {
                scene = sceneMeta.value;
                if (RecommendedCharsConfig.guaranteeSpawnChar)
                {
                    for (i = 0, c = scene.forcedNpcs.Length; i < c; i++)
                    {
                        if (scene.forcedNpcs[i].character == MrDaycare.charEnum)
                            scene.forcedNpcs[i] = ObjMan.Get<MrDaycare>("Npc/MrDaycare");
                    }
                    continue;
                }
                for (i = 0, c = scene.potentialNPCs.Count; i < c; i++)
                {
                    if (scene.potentialNPCs[i].selection?.character == MrDaycare.charEnum)
                        scene.potentialNPCs[i].selection = ObjMan.Get<MrDaycare>("Npc/MrDaycare");
                }
            }
        }

        private void AddPosterToLevel(LevelGenerator gen)
        {
            if (gen.scene == null) return;
            if (gen.Ec.npcsToSpawn.FirstOrDefault(x => x != null && x.Character == MrDaycare.charEnum) == null) return;

            gen.ld.posters = gen.ld.posters.AddToArray(ObjMan.Get<PosterObject>("Pst/DaycarePoster_Rules").Weighted(50));
        }
    }
}
