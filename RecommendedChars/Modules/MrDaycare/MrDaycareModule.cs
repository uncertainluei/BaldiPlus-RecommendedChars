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

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleExp;

        public override Action LoadAction => Load;
        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;

        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Room", "Daycare"), x => "DaycareRoom/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "Daycare"), x => "DaycareTex/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Item", "Pie"), x => "Pie/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(Plugin), "Audio", "Daycare"), "DaycareAud/");

            AssetMan.Add("PieThrow", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "PieThrow.wav"), "RecChars_Sfx_PieThrow", SoundType.Effect, Color.white));
            AssetMan.Add("PieSplat", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "PieSplat.wav"), "RecChars_Sfx_PieSplat", SoundType.Effect, Color.white));
            AssetMan.Add("DaveDoorOpen", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "Doors_DaveOpen.wav"), "Sfx_Doors_StandardOpen", SoundType.Effect, Color.white));
            AssetMan.Add("DaveDoorShut", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(Plugin, "Audio", "Sfx", "Doors_DaveShut.wav"), "Sfx_Doors_StandardShut", SoundType.Effect, Color.white));

            LoadPie();

            AssetMan.Add("DaycareRulesPoster", CreatePoster("DaycareRoom/pst_daycarerules", "DaycarePoster_Rules"));
            LoadMrDaycare();
        }

        private void LoadPie()
        {
            ItemObject pie = new ItemBuilder(Info)
            .SetNameAndDescription("RecChars_Itm_Pie", "RecChars_Desc_Pie")
            .SetEnum("RecChars_CherryBsoda")
            .SetMeta(ItemFlags.Persists | ItemFlags.CreatesEntity, new string[] { "food", "recchars_daycare_exempt" })
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("Pie/Pie_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("Pie/Pie_Large"), 50f))
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
            Sprite thrownPieSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("Pie/Pie_Large"), 25f);
            thrownPieSprite.name = "Pie_Thrown";
            pieUse.flyingSprite.GetComponent<SpriteRenderer>().sprite = thrownPieSprite;

            pieUse.groundedSprite = gumClone.groundedSprite;
            pieUse.groundedSprite.transform.localPosition = Vector3.back * -0.1f;
            pieUse.groundedSprite.GetComponent<SpriteRenderer>().sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("Pie/PieSplat"), 10f);

            pieUse.noBillboardMat = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "SpriteStandard_NoBillboard" && x.GetInstanceID() >= 0);

            GameObject.DestroyImmediate(gumClone);

            AssetMan.Add("PieItem", pie);
        }

        private void LoadMrDaycare()
        {
            MrDaycare daycare = new NPCBuilder<MrDaycare>(Info)
                .SetName("MrDaycare")
                .SetEnum("RecChars_MrDaycare")
                .SetPoster(AssetMan.Get<Texture2D>("DaycareTex/pri_daycare"), "RecChars_Pst_Daycare1", "RecChars_Pst_Daycare2")
                .AddMetaFlag(NPCFlags.Standard | NPCFlags.MakeNoise)
                .AddPotentialRoomAsset(LoadDaycareRoom(), 100)
                .AddLooker()
                .AddTrigger()
                .AddHeatmap()
                .SetWanderEnterRooms()
                .IgnorePlayerOnSpawn()
                .Build();

            daycare.spriteRenderer[0].transform.localPosition = Vector3.up * -1f;
            daycare.spriteRenderer[0].sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareTex/MrDaycare"), 65f);

            daycare.audMan = daycare.GetComponent<AudioManager>();
            daycare.audMan.subtitleColor = new Color(192f/255f, 242f/255f, 75f/255f);

            daycare.audDetention = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_Timeout"), "RecChars_Daycare_Timeout", SoundType.Voice, daycare.audMan.subtitleColor);
            daycare.audSeconds = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_Seconds"), "RecChars_Daycare_Seconds", SoundType.Voice, daycare.audMan.subtitleColor);

            daycare.audTimes = new SoundObject[]
            {
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_30"), "RecChars_Daycare_30", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_60"), "RecChars_Daycare_60", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_100"), "RecChars_Daycare_100", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_200"), "RecChars_Daycare_200", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_500"), "RecChars_Daycare_500", SoundType.Voice, daycare.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_3161"), "RecChars_Daycare_3161", SoundType.Voice, daycare.audMan.subtitleColor)
            };
            MrDaycare.audRuleBreaks = new Dictionary<string, SoundObject>()
            {
                { "Running" , ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoRunning"), "RecChars_Daycare_NoRunning", SoundType.Voice, daycare.audMan.subtitleColor)},
                { "Drinking" , ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoDrinking"), "RecChars_Daycare_NoDrinking", SoundType.Voice, daycare.audMan.subtitleColor)},
                { "Eating" , ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoEating"), "RecChars_Daycare_NoEating", SoundType.Voice, daycare.audMan.subtitleColor)},
                { "DaycareEscaping" , ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoEscaping"), "RecChars_Daycare_NoEscaping", SoundType.Voice, daycare.audMan.subtitleColor)},
                { "Throwing" , ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoThrowing"), "RecChars_Daycare_NoThrowing", SoundType.Voice, daycare.audMan.subtitleColor)},
                { "LoudSound" , ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("DaycareAud/Day_NoLoudSound"), "RecChars_Daycare_NoLoudSound", SoundType.Voice, daycare.audMan.subtitleColor)}
            };
            MrDaycare.audRuleBreaks.Add("DaycareEating", MrDaycare.audRuleBreaks["Eating"]);

            daycare.detentionNoise = 125;

            Principal principle = (Principal)NPCMetaStorage.Instance.Get(Character.Principal).value;
            daycare.Navigator.speed = principle.Navigator.speed;
            daycare.Navigator.accel = principle.Navigator.accel;
            daycare.Navigator.maxSpeed = 35f;
            daycare.Navigator.passableObstacles = principle.Navigator.passableObstacles;
            daycare.Navigator.preciseTarget = principle.Navigator.preciseTarget;

            PineDebugNpcIconPatch.icons.Add(daycare.Character, AssetMan.Get<Texture2D>("DaycareTex/BorderDaycare"));

            AssetMan.Add("MrDaycareNpc", daycare);
        }

        private RoomAsset LoadDaycareRoom()
        {
            DaycareDoorAssets.template = ObjectCreators.CreateDoorDataObject("DaycareDoor", AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Open"), AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Shut"));
            DaycareDoorAssets.mask = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<StandardDoor>().First().mask[0]);
            DaycareDoorAssets.mask.name = "DaycareDoor_Mask";
            DaycareDoorAssets.mask.SetMaskTexture(AssetMan.Get<Texture2D>("DaycareRoom/DaveDoor_Mask"));

            DaycareDoorAssets.open = AssetMan.Get<SoundObject>("DaveDoorOpen");
            DaycareDoorAssets.shut = AssetMan.Get<SoundObject>("DaveDoorShut");

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

            daycareRoomAsset.cells = new List<CellData>()
            {
                RoomAssetHelper.Cell(0,0,12),
                RoomAssetHelper.Cell(1,0,4),
                RoomAssetHelper.Cell(2,0,6),
                RoomAssetHelper.Cell(0,1,8),
                RoomAssetHelper.Cell(1,1,0),
                RoomAssetHelper.Cell(2,1,2),
                RoomAssetHelper.Cell(0,2,9),
                RoomAssetHelper.Cell(1,2,1),
                RoomAssetHelper.Cell(2,2,3)
            };

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
            daycareRoomAsset.roomFunctionContainer.functions = new List<RoomFunction>()
            {
                daycareRoomAsset.roomFunctionContainer .GetComponent<RuleFreeZone>(),
                roomFunction,
                daycareRoomAsset.roomFunctionContainer .GetComponent<CoverRoomFunction>()
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
    }
}
