using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class Module_MrDaycare : Module
    {
        public override string Name => "Mr. Daycare";

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleMrDaycare;

        public override Action LoadAction => Load;
        public override Action PostLoadAction => PostLoad;
        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;
        public override Action<string, int, CustomLevelObject> LevelAddendAction => FloorAddendLvl;

        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Room", "Daycare"), x => "DaycareRoom/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "Daycare"), x => "DaycareTex/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Item", "Daycare"), x => "DaycareItm/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(Plugin), "Audio", "Daycare"), "DaycareAud/");

            AssetMan.Add("PieThrow", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "PieThrow.wav"), "", SoundType.Effect, Color.white, 0f));
            AssetMan.Add("PieSplat", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "PieSplat.wav"), "Sfx_RecChars_PieSplat", SoundType.Effect, Color.white));

            LoadItems();

            AssetMan.Add("DaycareRulesPoster", CreatePoster("DaycareRoom/pst_daycarerules", "DaycarePoster_Rules"));
            LoadMrDaycare();

            LevelGeneratorEventPatch.OnNpcAdd += AddPosterToLevel;
        }

        private void LoadItems()
        {
            // Pie
            ItemObject pie = new ItemBuilder(Info)
            .SetNameAndDescription("Itm_RecChars_Pie", "Desc_RecChars_Pie")
            .SetEnum("RecChars_Pie")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, new string[] { "food", "recchars_daycare_exempt", "adv_good", "adv_sm_potential_reward" })
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
            ItemMetaData doorKeyMeta = new ItemMetaData(Info, new ItemObject[0]);
            doorKeyMeta.flags = ItemFlags.MultipleUse;
            doorKeyMeta.tags.AddRange(new string[] { "key", "crmp_contraband" });

            Items keyEnum = EnumExtensions.ExtendEnum<Items>("RecChars_DoorKey");

            ItemBuilder keyBuilder = new ItemBuilder(Info)
            .SetNameAndDescription("Itm_RecChars_DoorKey1", "Desc_RecChars_DoorKey")
            .SetEnum(keyEnum)
            .SetMeta(doorKeyMeta)
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareItm/DoorKey_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareItm/DoorKey_Large"), 50f))
            .SetShopPrice(600)
            .SetGeneratorCost(75)
            .SetItemComponent<ITM_DoorKey>();

            ItemObject keyItemObject = keyBuilder.Build();
            keyItemObject.name = "RecChars DoorKey1";
            ITM_DoorKey keyItm = (ITM_DoorKey)keyItemObject.item;
            keyItm.name = "Itm_DoorKey1";
            keyItm.layerMask = ((ITM_Acceptable)ItemMetaStorage.Instance.FindByEnum(Items.DetentionKey).value.item).layerMask;

            keyBuilder.SetNameAndDescription("Itm_RecChars_DoorKey2", "Desc_RecChars_DoorKey");
            keyBuilder.SetItemComponent<ITM_DoorKey>(null);
            ItemObject keyItemObject2 = keyBuilder.Build();
            keyItemObject2.name = "RecChars DoorKey2";
            keyItm = GameObject.Instantiate(keyItm, MTM101BaldiDevAPI.prefabTransform);
            keyItemObject2.item = keyItm;
            keyItm.name = "Itm_DoorKey2";
            keyItm.nextStage = keyItemObject;

            keyBuilder.SetNameAndDescription("Itm_RecChars_DoorKey3", "Desc_RecChars_DoorKey");
            keyItemObject = keyItemObject2;
            keyItemObject2 = keyBuilder.Build();
            keyItemObject2.name = "RecChars DoorKey3";
            keyItm = GameObject.Instantiate(keyItm, MTM101BaldiDevAPI.prefabTransform);
            keyItemObject2.item = keyItm;
            keyItm.name = "Itm_DoorKey3";
            keyItm.nextStage = keyItemObject;

            AssetMan.Add("DoorKeyItem", keyItemObject2);
        }

        private void LoadMrDaycare()
        {
            MrDaycare daycare = new NPCBuilder<MrDaycare>(Info)
                .SetName("MrDaycare")
                .SetEnum("RecChars_MrDaycare")
                .SetPoster(AssetMan.Get<Texture2D>("DaycareTex/pri_daycare"), "PST_PRI_RecChars_Daycare1", "PST_PRI_RecChars_Daycare2")
                .AddMetaFlag(NPCFlags.Standard | NPCFlags.MakeNoise)
                .SetMetaTags(new string[] { "faculty", "no_balloon_frenzy" })
                .AddPotentialRoomAsset(LoadDaycareRoom(), 100)
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
            daycare.audMan.subtitleColor = new Color(192f/255f, 242f/255f, 75f/255f);

            daycare.audDetention = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_Timeout"), "Vfx_RecChars_Daycare_Timeout", SoundType.Voice, daycare.audMan.subtitleColor);
            daycare.audSeconds = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_Seconds"), "Vfx_RecChars_Daycare_Seconds", SoundType.Voice, daycare.audMan.subtitleColor);

            daycare.audTimes = new SoundObject[]
            {
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_30"), "Vfx_RecChars_Daycare_30", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_60"), "Vfx_RecChars_Daycare_60", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_100"), "Vfx_RecChars_Daycare_100", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_200"), "Vfx_RecChars_Daycare_200", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_500"), "Vfx_RecChars_Daycare_500", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_3161"), "Vfx_RecChars_Daycare_3161", SoundType.Voice, daycare.audMan.subtitleColor)
            };

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
            daycare.audNoAfterHours = daycare.audComing;
            daycare.audNoBullying = daycare.audComing;
            daycare.audNoLockers = daycare.audComing;
            daycare.audNoStabbing = daycare.audComing;
            daycare.audScolds = new SoundObject[] {daycare.audComing};

            daycare.detentionNoise = 125;

            Principal principle = (Principal)NPCMetaStorage.Instance.Get(Character.Principal).value;
            daycare.Navigator.accel = principle.Navigator.accel;

            daycare.Navigator.speed = 36f;
            daycare.Navigator.maxSpeed = 36f;
            if (RecommendedCharsConfig.nerfedMrDaycare.Value)
            {
                daycare.Navigator.speed = 30f;
                daycare.Navigator.maxSpeed = 30f;
                daycare.maxTimeoutLevel = 1;
                daycare.ruleSensitivityMul = 1;
            }

            daycare.Navigator.passableObstacles = principle.Navigator.passableObstacles;
            daycare.Navigator.preciseTarget = principle.Navigator.preciseTarget;

            PineDebugNpcIconPatch.icons.Add(daycare.Character, AssetMan.Get<Texture2D>("DaycareTex/BorderDaycare"));
            CharacterRadarColorPatch.colors.Add(daycare.Character, daycare.audMan.subtitleColor);

            AssetMan.Add("MrDaycareNpc", daycare);
        }

        private RoomAsset LoadDaycareRoom()
        {
            DaycareDoorAssets.template = ObjectCreators.CreateDoorDataObject("DaycareDoor", AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Open"), AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Shut"));
            DaycareDoorAssets.locked = ObjectCreators.CreateDoorDataObject("DaycareDoor", AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Open"), AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Locked"));

            DaycareDoorAssets.mask = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<StandardDoor>().First().mask[0]);
            DaycareDoorAssets.mask.name = "DaycareDoor_Mask";
            DaycareDoorAssets.mask.SetMaskTexture(AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Mask"));

            DaycareDoorAssets.open = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "Doors_DaveOpen.wav"), "Sfx_Doors_StandardOpen", SoundType.Effect, Color.white);
            DaycareDoorAssets.shut = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "Doors_DaveShut.wav"), "Sfx_Doors_StandardShut", SoundType.Effect, Color.white);
            DaycareDoorAssets.unlock = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "Doors_DaveUnlock.wav"), "Sfx_Doors_StandardUnlock", SoundType.Effect, Color.white);

            RoomAsset daycareRoomAsset = RoomAsset.CreateInstance<RoomAsset>();
            ((ScriptableObject)daycareRoomAsset).name = "Room_Daycare_0";
            daycareRoomAsset.name = "Daycare_0";

            daycareRoomAsset.type = RoomType.Room;
            daycareRoomAsset.category = EnumExtensions.ExtendEnum<RoomCategory>("RecChars_Daycare");

            daycareRoomAsset.florTex = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Floor");
            daycareRoomAsset.wallTex = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Wall");
            daycareRoomAsset.ceilTex = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_Ceiling");
            daycareRoomAsset.doorMats = DaycareDoorAssets.template;
            daycareRoomAsset.keepTextures = true;

            daycareRoomAsset.lightPre = GameObject.Instantiate(((DrReflex)NPCMetaStorage.Instance.Get(Character.DrReflex).value).potentialRoomAssets[0].selection.lightPre, MTM101BaldiDevAPI.prefabTransform);
            daycareRoomAsset.lightPre.name = "DaycareLight";

            MeshRenderer light = daycareRoomAsset.lightPre.GetComponentInChildren<MeshRenderer>();
            light.sharedMaterial = new Material(light.sharedMaterial)
            {
                name = "Daycare_CeilingLight",
                mainTexture = AssetMan.Get<Texture2D>("DaycareRoom/Daycare_CeilingLight")
            };

            daycareRoomAsset.mapMaterial = ObjectCreators.CreateMapTileShader(AssetMan.Get<Texture2D>("DaycareRoom/Map_Daycare"));
            daycareRoomAsset.color = Color.green;

            daycareRoomAsset.cells = RoomAssetHelper.CellRect(3, 3);

            daycareRoomAsset.posterChance = 0.1f;
            daycareRoomAsset.posters = new List<WeightedPosterObject>();

            daycareRoomAsset.posterDatas = new List<PosterData>()
            {
                RoomAssetHelper.PosterData(0,1,CreatePoster("DaycareRoom/pst_daycareinfo","DaycarePoster_Info"),Direction.West),
                RoomAssetHelper.PosterData(1,2,CreatePoster("DaycareRoom/pst_daycareclock","DaycarePoster_Clock"),Direction.North),
                RoomAssetHelper.PosterData(2,1,AssetMan.Get<PosterObject>("DaycareRulesPoster"),Direction.East)
            };

            daycareRoomAsset.standardLightCells = new List<IntVector2>() { new IntVector2(1, 1) };
            daycareRoomAsset.potentialDoorPositions = new List<IntVector2>()
            {
                new IntVector2(0,0),
                new IntVector2(1,0),
                new IntVector2(2,0),
                new IntVector2(0,2),
                new IntVector2(2,2)
            };

            daycareRoomAsset.windowObject = ObjectCreators.CreateWindowObject("Daycare_Window", AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow"), AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow_Broken"), AssetMan.Get<Texture2D>("DaycareRoom/DaycareWindow_Mask"));
            daycareRoomAsset.windowChance = 0.25f;

            daycareRoomAsset.entitySafeCells = new List<IntVector2>()
            {
                new IntVector2(1,0),
                new IntVector2(1,1),
                new IntVector2(2,1)
            };

            DetentionRoomFunction detention = Resources.FindObjectsOfTypeAll<DetentionRoomFunction>().First(x => x.name == "OfficeRoomFunction" && x.GetInstanceID() >= 0);
            DaycareRoomFunction roomFunction = RecommendedCharsPlugin.CloneComponent<DetentionRoomFunction, DaycareRoomFunction>(GameObject.Instantiate(detention, MTM101BaldiDevAPI.prefabTransform));
            roomFunction.name = "DaycareRoomFunction";

            roomFunction.gaugeSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareTex/TimeoutIcon"), 25f);

            daycareRoomAsset.roomFunctionContainer = roomFunction.GetComponent<RoomFunctionContainer>();
            GameObject.DestroyImmediate(daycareRoomAsset.roomFunctionContainer.GetComponent<CharacterPostersRoomFunction>());
            GameObject.DestroyImmediate(daycareRoomAsset.roomFunctionContainer.GetComponent<RuleFreeZone>());

            DaycareRuleFreeZone ruleFreeZone = daycareRoomAsset.roomFunctionContainer.gameObject.AddComponent<DaycareRuleFreeZone>();
            ruleFreeZone.excludeEscaping = false;

            daycareRoomAsset.roomFunctionContainer.functions = new List<RoomFunction>()
            {
                roomFunction,
                ruleFreeZone,
                daycareRoomAsset.roomFunctionContainer.GetComponent<CoverRoomFunction>()
            };

            return daycareRoomAsset;
        }

        private PosterObject CreatePoster(string path, string name)
        {
            PosterObject poster = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>(path), new PosterTextData[0]);
            poster.name = name;
            return poster;
        }

        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("PieItem").Weighted(50));
                scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("DoorKeyItem").Weighted(30));

                if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<MrDaycare>("MrDaycareNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
                else
                    scene.potentialNPCs.CopyCharacterWeight(Character.Beans, AssetMan.Get<MrDaycare>("MrDaycareNpc"));
                return;
            }

            if (title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("PieItem").Weighted(50));
                scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("DoorKeyItem").Weighted(30));

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.potentialNPCs.CopyCharacterWeight(Character.Beans, AssetMan.Get<MrDaycare>("MrDaycareNpc"));
                }
                else if (id == 0)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<MrDaycare>("MrDaycareNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
            }
        }

        private void FloorAddendLvl(string title, int id, LevelObject lvl)
        {
            if (title == "END" || title.StartsWith("F"))
            {
                lvl.potentialItems = lvl.potentialItems.AddToArray(AssetMan.Get<ItemObject>("PieItem").Weighted(25));
                lvl.potentialItems = lvl.potentialItems.AddToArray(AssetMan.Get<ItemObject>("DoorKeyItem").Weighted(15));
                return;
            }
        }

        private void AddPosterToLevel(LevelGenerator gen)
        {
            if (gen.scene == null) return;
            if (gen.Ec.npcsToSpawn.FirstOrDefault(x => x != null && x.Character == MrDaycare.charEnum) == null) return;

            gen.ld.posters = gen.ld.posters.AddToArray(AssetMan.Get<PosterObject>("DaycareRulesPoster").Weighted(50));
        }

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

            List<Items> keyItems = new List<Items>()
            {
                Items.DetentionKey
            };
            foreach (ItemMetaData meta in ItemMetaStorage.Instance.FindAllWithTags(false, "shape_key"))
            {
                if (!keyItems.Contains(meta.id))
                    keyItems.Add(meta.id);
            }
            ITM_DoorKey.keyEnums = keyItems.ToArray();
        }
    }
}
