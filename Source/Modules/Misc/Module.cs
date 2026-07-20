using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;
using PlusStudioLevelLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Miscellaneous"), CaudexModulePriority(10)]
    public sealed class Module_Misc : RecCharsModule
    {
        internal override byte IconId => 19;

        protected override void Initialized()
        {
            // Load texture and audio assets
            ObjectCreation.AddTexturesToAssetMan("CakeTex/", ["Textures", "Environment", "Structure", "Cake"]);
            ObjectCreation.AddTexturesToAssetMan("PartyElevateTex/", ["Textures", "Environment", "Structure", "PartyElevator"]);
            ObjectCreation.AddTexturesToAssetMan("SecretPstTex/", ["Textures", "Environment", "Poster", "Secret"]);
            ObjectCreation.AddTexturesToAssetMan("SecretAreaTex/", ["Textures", "Environment", "Room", "Secret"]);
            ObjectCreation.AddTexturesToAssetMan("NpcOverlays/", ["Textures", "Npc", "Overlays"]);
            ObjectCreation.AddAudioToAssetMan("NpcSurprises/", ["Audio", "Npc", "Misc"]);

            // Load patches
            // Hooks.PatchAll(typeof(SpoilerAreaPatches));
        }

        private WeightedRoomAsset[] newCafeterias;

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            LoadPosters();

            _baseMat = AssetMan.Get<Material>("Mat/TileBase");
            _baseShader = AssetFinder.FindOfTypeWithName<Shader>("Shader Graphs/Standard", true);

            // BBCR Party Style Objects
            LoadCakeObj(
                CreateMaterial("CakeTex/CakeSide"),
                CreateMaterial("CakeTex/CakeTop"),
                CreateMaterial("CakeTex/Candle")
            );
            LoadPartyElevatorObj(
                CreateMaterial(AssetFinder.FindOfTypeWithName<Texture2D>("DiamongPlateFloor", true), "DiamondPlateFloor", true),
                CreateMaterial("PartyElevateTex/PantographSide"),
                CreateMaterial("PartyElevateTex/PantographFront"),
                CreateMaterial("PartyElevateTex/MetalFence", true)
            );
            LoadArtPaintings();

            CreatePartyRooms();
            CreatePartyWinObjects();
            CreatePartyWinSurpriseNpcs();

            //LoadPartyWinLevel();
        }

        private void LoadPosters()
        {
            // bee poster
            ObjectCreation.CreatePoster(AssetLoader.TextureFromMod(BasePlugin, "Textures", "Environment", "Poster", "bee.png"), "bee");

            // Ending level posters
            ObjectCreation.CreatePoster("SecretPstTex/FerrisWheel", "FerrisWheel");
            ObjectCreation.CreatePoster("SecretPstTex/MagicEDwayne", "MagicEDwayne");
            ObjectCreation.CreatePoster("SecretPstTex/JonJedi", "JonJedi");
            ObjectCreation.CreatePoster("SecretPstTex/TheSun", "TheSun");

            ObjectCreation.CreatePoster("SecretPstTex/Meaning", "Meaning", new PosterTextData()
            {
                color = Color.black,
                textKey = "PST_RecChars_Meaning_1",
                font = BaldiFonts.ComicSans18.FontAsset(),
                fontSize = 18,
                alignment = TextAlignmentOptions.Top,
                position = new(0, 184),
                size = new(256, 72)
            }, new PosterTextData()
            {
                color = Color.black,
                textKey = "PST_RecChars_Meaning_2",
                font = BaldiFonts.ComicSans18.FontAsset(),
                fontSize = 18,
                alignment = TextAlignmentOptions.Bottom,
                position = new(0, 0),
                size = new(256, 72)
            });

            PosterObject comic = ObjectCreation.CreatePoster("SecretPstTex/Comic_Andy1", "Comic_Andy", "andycomic", new PosterTextData()
            {
                color = Color.gray,
                textKey = "PST_RecChars_Comic_Andy1_1",
                font = BaldiFonts.ComicSans12.FontAsset(),
                fontSize = 12,
                alignment = TextAlignmentOptions.TopLeft,
                position = new(32, 180),
                size = new(192, 12)
            }, new PosterTextData()
            {
                color = Color.black,
                textKey = "PST_RecChars_Comic_Andy1_2",
                font = BaldiFonts.ComicSans12.FontAsset(),
                fontSize = 12,
                alignment = TextAlignmentOptions.Center,
                position = new(100, 160),
                size = new(50, 12)
            }, new PosterTextData()
            {
                color = Color.black,
                textKey = "PST_RecChars_Comic_Andy1_3",
                font = BaldiFonts.ComicSans12.FontAsset(),
                fontSize = 12,
                alignment = TextAlignmentOptions.Top,
                position = new(140, 80),
                size = new(64, 32)
            });
            comic.multiPosterArray = new PosterObject[4];
            comic.multiPosterArray[0] = comic;
            comic.multiPosterArray[1] = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("SecretPstTex/Comic_Andy2"), 
            [
                new()
                {
                    color = Color.black,
                    textKey = "PST_RecChars_Comic_Andy2_1",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.Top,
                    position = new(32, 166),
                    size = new(112, 26)
                }, 
                new()
                {
                    color = Color.black,
                    textKey = "PST_RecChars_Comic_Andy2_2",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.Center,
                    position = new(86, 64),
                    size = new(69, 46)
                }
            ]);
            comic.multiPosterArray[1].name = "Comic_Andy2";
            comic.multiPosterArray[2] = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("SecretPstTex/Comic_Andy3"),
            [
                new()
                {
                    color = Color.gray,
                    textKey = "PST_RecChars_Comic_Andy3_1",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.TopLeft,
                    position = new(32, 180),
                    size = new(144, 12)
                },
                new()
                {
                    color = Color.black,
                    textKey = "PST_RecChars_Comic_Andy3_2",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.Center,
                    position = new(32, 136),
                    size = new(75, 40)
                },
                new()
                {
                    color = Color.black,
                    textKey = "PST_RecChars_Comic_Andy3_3",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.Center,
                    position = new(128, 144),
                    size = new(40, 32)
                }
            ]);
            comic.multiPosterArray[2].name = "Comic_Andy3";
            comic.multiPosterArray[3] = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("SecretPstTex/Comic_Andy4"),
            [
                new()
                {
                    color = Color.black,
                    textKey = "PST_RecChars_Comic_Andy4_1",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.Left,
                    position = new(32, 166),
                    size = new(64, 26)
                },
                new()
                {
                    color = Color.black,
                    textKey = "PST_RecChars_Comic_Andy4_2",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.Right,
                    position = new(160, 176),
                    size = new(64, 16)
                },
            ]);
            comic.multiPosterArray[3].name = "Comic_Andy4";

            PosterObject activityPoster = AssetFinder.FindOfTypeWithName<PosterObject>("Chk_Act_MathMachine", true);
            activityPoster = GameObject.Instantiate(activityPoster);
            activityPoster.name = "Chk_Act_Paintings";
            activityPoster.textData[0].textKey = "PST_RecChars_CHK_PaintingsTitle";
            activityPoster.textData[1].textKey = "PST_RecChars_CHK_PaintingsDesc";
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_chk_paintings", activityPoster);

            ExtendedPosterObject wantedPoster = ScriptableObject.CreateInstance<ExtendedPosterObject>();
            wantedPoster.name = "WantedBulletin";
            wantedPoster.baseTexture = AssetFinder.FindOfTypeWithName<Texture2D>("BulletinBoard_Blank", true);
            wantedPoster.overlayData = [new(AssetMan.Get<Texture2D>("SecretPstTex/WantedOverlay"), new(0,0))];
            wantedPoster.textData = [
                new()
                {
                    color = Color.black,
                    style = FontStyles.Bold,
                    textKey = "PST_RecChars_WantedBulletin_1",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.Bottom,
                    position = new(68, 184),
                    size = new(110, 12)
                },
                new()
                {
                    color = Color.black,
                    textKey = "PST_RecChars_WantedBulletin_2",
                    font = BaldiFonts.ComicSans12.FontAsset(),
                    fontSize = 12,
                    alignment = TextAlignmentOptions.Top,
                    position = new(70, 60),
                    size = new(110, 50)
                }
            ];
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_wantedbulletin", wantedPoster);

            ObjectCreation.CreatePoster(AssetFinder.FindOfTypeWithName<Texture2D>("pst_black", true), "EndOfLine", [new()
            {
                color = Color.white,
                textKey = "PST_RecChars_EndOfLine",
                font = BaldiFonts.ComicSans36.FontAsset(),
                fontSize = 36,
                alignment = TextAlignmentOptions.Center,
                position = new(16, 16),
                size = new(224, 224)
            }]);
        }

        private Material _baseMat;
        private Shader _baseShader;
        private Material CreateMaterial(Texture2D tex, string name, bool zWrite = false)
        {
            Material newMat = new(_baseMat) { name = name };
            newMat.shader = _baseShader;
            newMat.SetMainTexture(tex);
            newMat.enableInstancing = false;
            if (zWrite)
                newMat.SetFloat("_Offset", 0.015f);
            return newMat;
        }
        private Material CreateMaterial(string path, bool zWrite = false)
            => CreateMaterial(AssetMan.Get<Texture2D>(path), Path.GetFileNameWithoutExtension(path), zWrite);

        private void LoadCakeObj(params Material[] mats)
        {
            // Cake model
            Dictionary<string, Material> materials = new()
            {
                { "ClassicCakeSide", mats[0]},
                { "ClassicCakeTop", mats[1] },
                { "ClassicCakeCandle", mats[2] }
            };
            GameObject cakeObject = AssetLoader.ModelFromModManualMaterials(BasePlugin, materials, "Meshes", "ClassicPartyCake.obj");
            cakeObject.ConvertToPrefab(true);
            CapsuleCollider cakeCollider = cakeObject.AddComponent<CapsuleCollider>();
            cakeCollider.radius = 18f;
            cakeCollider.height = 50;
            NavMeshObstacle obstacle = cakeObject.AddComponent<NavMeshObstacle>();
            obstacle.carveOnlyStationary = true;
            obstacle.carving = true;
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.radius = 18f;
            obstacle.height = 25f;
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_cake", cakeObject);

            // Candle flame
            Sprite candleSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CakeTex/CandleFlame"), 10f);
            GameObject candleObject = ObjectCreation.CreateSpriteBillboard(candleSprite, default, MTM101BaldiDevAPI.prefabTransform, "Flame").gameObject;

            cakeObject = GameObject.Instantiate(cakeObject, MTM101BaldiDevAPI.prefabTransform);
            cakeObject.name = "ClassicPartyCake_WithFlame";
            candleObject = GameObject.Instantiate(candleObject, cakeObject.transform);
            candleObject.transform.position = Vector3.up * 33f;
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_cakewcandle", cakeObject);
        }

        private void LoadPartyElevatorObj(params Material[] mats)
        {
            Dictionary<string, Material> materials = new()
            {
                { "DiamondPlateFloor", mats[0]},
                { "PantographSide", mats[1] },
                { "PantographFront", mats[2] },
                { "MetalFence", mats[3] }
            };
            GameObject elevatorObject = AssetLoader.ModelFromModManualMaterials(BasePlugin, materials, "Meshes", "PartyElevator.obj");
            elevatorObject.ConvertToPrefab(true);
            BoxCollider elevatorCollider = elevatorObject.AddComponent<BoxCollider>();
            elevatorCollider.center = Vector3.up * 15f;
            elevatorCollider.size = new(10f,30f,10f);
            NavMeshObstacle obstacle = elevatorObject.AddComponent<NavMeshObstacle>();
            obstacle.carveOnlyStationary = true;
            obstacle.carving = true;
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = Vector3.up * 15f;
            obstacle.size = new(10f,30f,10f);
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_partyelevator", elevatorObject);
        }

        private void LoadArtPaintings()
        {
            Sprite[] paintingSprites = AssetLoader.SpritesFromSpritesheet(2, 4, 70f, Vector2.one*0.5f, AssetMan.Get<Texture2D>("SecretAreaTex/ArtPaintings"));

            Notebook notebookObj = GameObject.Instantiate(AssetFinder.FindAllOfType<Notebook>(true).First(), MTM101BaldiDevAPI.prefabTransform);
            Painting paintingObj = notebookObj.gameObject.AddComponent<Painting>();
            paintingObj.name = "Painting_0";
            paintingObj.sprite = notebookObj.sprite;
            GameObject.DestroyImmediate(notebookObj);
            paintingObj.sprite.sprite = paintingSprites[0];
            paintingObj.audShatter = AssetMan.Get<SoundObject>("Sfx/FakeShatter");
            paintingObj.particlePre = ObjectCreation.CreateSpriteBillboard(null, default, MTM101BaldiDevAPI.prefabTransform, "PaintingParticle")
                .gameObject.AddComponent<PaintingParticle>();
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_painting0", paintingObj.gameObject);

            for (int i = 1; i < paintingSprites.Length; i++)
            {
                paintingObj = GameObject.Instantiate(paintingObj, MTM101BaldiDevAPI.prefabTransform);
                paintingObj.name = "Painting_"+i;
                paintingObj.sprite.sprite = paintingSprites[i];
                LevelLoaderPlugin.Instance.basicObjects.Add("recchars_painting"+i, paintingObj.gameObject);
            }
        }

        private void CreatePartyRooms()
        {
            Balloon[] balloons = ((PartyEvent)RandomEventMetaStorage.Instance.Get(RandomEventType.Party).value).balloon;

            RoomAsset cafeteria = Resources.FindObjectsOfTypeAll<RoomAsset>().First(x => x.GetInstanceID() >= 0 && x.roomFunctionContainer != null && x.roomFunctionContainer.name.StartsWith("Cafeteria"));
            CaudexRoomBlueprint cafeBlueprint = new(Plugin, "PartyCafeteria", cafeteria);
            ObjMan.Add("Room/CafeParty", cafeBlueprint);
            cafeBlueprint.functionContainer = GameObject.Instantiate(cafeBlueprint.functionContainer, MTM101BaldiDevAPI.prefabTransform);
            cafeBlueprint.functionContainer.name = "CafeteriaPartyRoomFunction";
            BalloonRoomFunction balloonFunction = cafeBlueprint.functionContainer.AddFunction<BalloonRoomFunction>();
            balloonFunction.balloonCount = 10;
            balloonFunction.balloonPres = balloons;
            ObjectCreation.AddRoom(cafeBlueprint);

            newCafeterias = ObjectCreation.RoomAssetsFromDirectory(cafeBlueprint, Path.Combine("Cafeteria", "Party"));

            ObjectCreation.AddRoom(cafeBlueprint, "recchars_partycafeterianonanas");
            RoomFunctionContainer noNanasFunction = GameObject.Instantiate(cafeBlueprint.functionContainer, MTM101BaldiDevAPI.prefabTransform);
            noNanasFunction.name = "CafeteriaPartyRoomFunction_NoNanas";
            noNanasFunction.RemoveFunction<NanaPeelRoomFunction>();
            LevelLoaderPlugin.Instance.roomSettings["recchars_partycafeterianonanas"].container = noNanasFunction;

            // Ending Variant
            ObjectCreation.AddRoom(cafeBlueprint, "recchars_partycafeteriawin");
            noNanasFunction = GameObject.Instantiate(noNanasFunction, MTM101BaldiDevAPI.prefabTransform);
            noNanasFunction.name = "CafeteriaPartyRoomFunction_Win";
            noNanasFunction.AddFunction<PartyWinRoomFunction>();
            SkyboxRoomFunction skyboxFunc = noNanasFunction.GetComponent<SkyboxRoomFunction>();
            noNanasFunction.functions.Remove(skyboxFunc);
            skyboxFunc = skyboxFunc.gameObject.SwapComponent<SkyboxRoomFunction, PartyWinSkyboxRoomFunction>(false);
            noNanasFunction.AddFunction(skyboxFunc);
            skyboxFunc.skybox.modifiedMeshHeight = 2;

            LevelLoaderPlugin.Instance.roomSettings["recchars_partycafeteriawin"].container = noNanasFunction;
        }

        private void CreatePartyWinObjects()
        {
            // Invisible Wall
            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            newObject.name = "InvisibleWall";
            newObject.ConvertToPrefab(true);
            MeshRenderer renderer = newObject.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateMaterial(AssetLoader.TextureFromMod(BasePlugin, "Textures", "Compat", "LevelStudio", "InvisWallPlaceholder.png"),
                "InvisibleWall");
            renderer.enabled = false;
            newObject.transform.localScale = Vector3.one*10f;
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_invisiblewall", newObject);

            // Triggers
            newObject = new("ElevatorTrigger", typeof(PartyLiftTrigger), typeof(BoxCollider));
            newObject.ConvertToPrefab(true);
            BoxCollider trigger = newObject.GetComponent<BoxCollider>();
            trigger.size = Vector3.one*5f;
            trigger.isTrigger = true;
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_elevatortrigger", newObject);
            AssetMan.Add("Mat/ElevatorTrigger", CreateMaterial(AssetLoader.TextureFromMod(BasePlugin, "Textures", "Compat", "LevelStudio", "TriggerPlaceholder.png"),
                "ElevatorTrigger"));

            newObject = new("CandleTrigger", typeof(PartyBlowTrigger), typeof(BoxCollider));
            newObject.ConvertToPrefab(true);
            trigger = newObject.GetComponent<BoxCollider>();
            trigger.size = Vector3.one * 3f;
            trigger.isTrigger = true;
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_candletrigger", newObject);

            // Baldi's Office Room
            RoomFunctionContainer container = new GameObject("BaldiOfficeRoomFunction", typeof(RoomFunctionContainer)).GetComponent<RoomFunctionContainer>();
            container.gameObject.ConvertToPrefab(true);
            container.functions = [];
            Balloon[] balloons = ObjMan.Get<CaudexRoomBlueprint>("Room/CafeParty").functionContainer.GetComponent<BalloonRoomFunction>().balloonPres;
            container.AddFunction<BalloonRoomFunction>().balloonPres = balloons;
            StandardDoorMats doorMat = AssetFinder.FindOfTypeWithName<StandardDoorMats>("BaldiLabDoorSet", true);
            doorMat = ObjectCreators.CreateDoorDataObject("BaldiOfficeDoor",
                AssetMan.Get<Texture2D>("SecretAreaTex/BaldiOfficeDoor_Open").OverlayTexture((Texture2D)doorMat.open.mainTexture, null, Color.clear, Color.magenta),
                AssetMan.Get<Texture2D>("SecretAreaTex/BaldiOfficeDoor_Shut").OverlayTexture((Texture2D)doorMat.shut.mainTexture, null, Color.clear, Color.magenta)
            );
            RoomSettings settings = new(RoomCategory.Office, RoomType.Room, Color.yellow, doorMat);
            settings.container = container;
            LevelLoaderPlugin.Instance.roomSettings.Add("recchars_baldioffice", settings);

            // Baldloons + Spawner
            Sprite[] sprites = AssetLoader.SpritesFromSpriteSheetCount(AssetMan.Get<Texture2D>("SecretAreaTex/Baldloons"), 128, 256, 32, 5);
            Balloon[] baldloons = new Balloon[4];
            SpriteRenderer spriteImg;
            for (int i = 0; i < 4; i++)
            {
                baldloons[i] = GameObject.Instantiate(balloons[0], MTM101BaldiDevAPI.prefabTransform);
                baldloons[i].name = "Baldloon_"+i;
                spriteImg = baldloons[i].GetComponentInChildren<SpriteRenderer>();
                spriteImg.sprite = sprites[i];
                spriteImg.transform.localPosition = Vector3.up * -0.54f;
            }

            newObject = new GameObject("Structure_PartyWinBalloonSpawner", typeof(BalloonSpawnerStructure));
            newObject.ConvertToPrefab(true);
            ObjMan.Add("Strct/PartyWinBaldloonSpawner", new StructureWithParameters()
            {
                prefab = newObject.GetComponent<BalloonSpawnerStructure>(),
                parameters = new() { prefab = baldloons.Select(x => x.gameObject.Weighted(100)).ToArray(), minMax = [new(3,9)]}
            });

            // Secret ending tape
            ItemObject baseTape = ItemMetaStorage.Instance.FindByEnum(Items.Tape).value;
            
            ItemObject endingItem = new ItemBuilder(Plugin)
                .SetNameAndDescription("Itm_RecChars_Tape", "Desc_Nothing")
                .SetSprites(baseTape.itemSpriteSmall, baseTape.itemSpriteLarge)
                .SetShopPrice(0)
                .SetEnum("RecChars_EndingTape")
                .SetMeta(ItemFlags.Unobtainable, [])
                .SetItemComponent<ITM_PartySecretTape>()
                .Build();
            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_endingtape", endingItem);
            
            
            string key = "Vfx_RecChars_SecretTape"; // Technically unnecessary but this is done for readability sake

            ITM_PartySecretTape.itemEnum = endingItem.itemType;
            
            ITM_PartySecretTape.speech = ObjectCreators.CreateSoundObject(AssetFinder.FindAllOfType<AudioClip>(false).First(x => x.length > 3f), key+"1", SoundType.Voice, Color.white, 166.87f);
            ITM_PartySecretTape.speech.encrypted = true;
            ITM_PartySecretTape.speech.additionalKeys = [
                new() {encrypted = true, key = key+2, time = 2.17f},
                new() {encrypted = true, key = key+3, time = 3.8f},
                new() {encrypted = true, key = key+4, time = 7.7f},
                new() {encrypted = true, key = key+5, time = 9.85f},
                new() {encrypted = true, key = key+6, time = 13.52f},
                new() {encrypted = true, key = key+7, time = 16.84f},
                new() {encrypted = true, key = key+2, time = 20.2f},
                new() {encrypted = true, key = key+8, time = 21.2f},
                new() {encrypted = true, key = key+9, time = 29.5f},
                new() {encrypted = true, key = key+10, time = 35.5f},
                new() {encrypted = true, key = key+11, time = 41.6f},
                new() {encrypted = true, key = key+12, time = 49f},
                new() {encrypted = true, key = key+13, time = 52.5f},
                new() {encrypted = true, key = key+14, time = 55.65f},
                new() {encrypted = true, key = key+15, time = 58.9f},
                new() {encrypted = true, key = key+16, time = 63.7f},
                new() {encrypted = true, key = key+17, time = 67.86f},
                new() {encrypted = true, key = key+18, time = 73.5f},
                new() {encrypted = true, key = key+19, time = 74.62f},
                new() {encrypted = true, key = key+20, time = 79.5f},
                new() {encrypted = true, key = key+21, time = 85.6f},
                new() {encrypted = true, key = key+22, time = 89.96f},
                new() {encrypted = true, key = key+23, time = 92.6f},
                new() {encrypted = true, key = key+24, time = 96.8f},
                new() {encrypted = true, key = key+25, time = 100f},
                new() {encrypted = true, key = key+26, time = 104.4f},
                new() {encrypted = true, key = key+2, time = 108.3f},
                new() {encrypted = true, key = key+27, time = 109.7f},
                new() {encrypted = true, key = key+28, time = 116f},
                new() {encrypted = true, key = key+29, time = 117.86f},
                new() {encrypted = true, key = key+30, time = 122.3f},
                new() {encrypted = true, key = key+31, time = 127.3f},
                new() {encrypted = true, key = key+32, time = 132f},
                new() {encrypted = true, key = key+33, time = 134.8f},
                new() {encrypted = true, key = key+34, time = 137.6f},
                new() {encrypted = true, key = key+35, time = 141.8f},
                new() {encrypted = true, key = key+36, time = 148f},
                new() {encrypted = true, key = key+37, time = 152.7f},
                new() {encrypted = true, key = key+38, time = 157.4f},
                new() {encrypted = true, key = key+39, time = 159.4f},
                new() {encrypted = true, key = key+40, time = 162.58f}
            ];
        }

        private void CreatePartyWinSurpriseNpcs()
        {
            Sprite[] vanillaSprites = AssetFinder.FindAllOfType<Sprite>(true);

            // Surprise Baldi
            ClickableSpecialFunctionTrigger tutorialBaldi = AssetFinder.FindOfTypeWithName<ClickableSpecialFunctionTrigger>("Baldi_Tutorial_27", true);
            tutorialBaldi = GameObject.Instantiate(tutorialBaldi, MTM101BaldiDevAPI.prefabTransform);
            tutorialBaldi.name = "SurpriseBaldi";
            GameObject baldiObject = tutorialBaldi.gameObject;
            GameObject.DestroyImmediate(tutorialBaldi);
            GameObject.DestroyImmediate(baldiObject.transform.GetChild(1).gameObject); // Capsule collider
            GameObject.DestroyImmediate(baldiObject.GetComponent<Rigidbody>());
            Entity entity = baldiObject.GetComponent<Entity>();
            GameObject.DestroyImmediate(entity.trigger);
            GameObject.DestroyImmediate(entity.collider);
            GameObject.DestroyImmediate(entity.externalActivity);
            GameObject.DestroyImmediate(entity);
            SpriteRenderer spriteRenderer = baldiObject.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = vanillaSprites.First(x => x.name == "Baldi_Talk_Standing_Sheet_0");
            spriteRenderer.material = AssetMan.Get<Material>("Mat/SpriteNoBillboard");
            GameObject spriteObject = spriteRenderer.gameObject;
            spriteRenderer = GameObject.Instantiate(spriteRenderer, spriteRenderer.transform, false);
            spriteRenderer.name = "PartyHat";
            spriteRenderer.transform.localPosition = Vector3.back * 0.001f;
            spriteRenderer.sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NpcOverlays/Baldi"), 32f);
            spriteObject.AddComponent<BillboardUpdater>();
            SurpriseNpcBase supriseBaldi = baldiObject.AddComponent<SurpriseNpcBase>();
            supriseBaldi.audMan = supriseBaldi.GetComponent<PropagatedAudioManager>();
            supriseBaldi.audSurprise = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NpcSurprises/BAL_Surprise"), "Vfx_BAL_PartySurprise1", SoundType.Voice, Color.green, 6.98f);
            supriseBaldi.audSurprise.additionalKeys = [new() { key = "Vfx_BAL_PartySurprise2", time = 3f }];
            baldiObject.SetActive(false);
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_surprisebaldi", baldiObject);

            // Random Surprise NPC
            GameObject surpriseObject = new("SurpriseNpc_Random", typeof(SurpriseNpc));
            surpriseObject.ConvertToPrefab(true);
            surpriseObject.SetActive(false);
            GameObject rendererBase = new("RendererBase");
            rendererBase.transform.parent = surpriseObject.transform;
            spriteRenderer = ObjectCreation.CreateSpriteBillboard(AssetLoader.SpriteFromMod(BasePlugin, Vector2.one / 2f, 15f, "Textures", "Compat", "LevelStudio", "SupriseNpcPlaceholder.png"), default, rendererBase.transform);
            SurpriseNpc surpriseNpc = surpriseObject.GetComponent<SurpriseNpc>();
            surpriseNpc.audMan = surpriseObject.AddComponent<PropagatedAudioManager>();
            surpriseNpc.audMan.usesVfx = true;
            surpriseNpc.audMan.overrideSubtitleColor = true;
            surpriseNpc.rendererBase = rendererBase;
            surpriseNpc.spriteRenderer = spriteRenderer;
            LevelLoaderPlugin.Instance.basicObjects.Add("recchars_surprisenpc", surpriseObject);

            // Vanilla Surprise NPC variants
            Principal principal = (Principal)NPCMetaStorage.Instance.Get(Character.Principal).value;
            SurpriseNpc.AddVisual(new SurpriseNpcVisualSprite(
                principal,
                AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NpcOverlays/Principal").OverlayTexture(principal.chasingSprite), new Vector2(0.5f, 0.4f), 65f),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NpcSurprises/PRI_NoSurprises"), "Vfx_PRI_NoSurprises", SoundType.Voice, new(0f, 30 / 255f, 123 / 255f), 1.75f)
            ));
            SurpriseNpc.AddVisual(new SurpriseNpcVisualSprite(
                NPCMetaStorage.Instance.Get(Character.Playtime).value,
                AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NpcOverlays/Playtime").OverlayTexture(vanillaSprites.First(x => x.name == "Playtime_6")), 100f),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NpcSurprises/PT_Surprise"), "Vfx_Playtime_Surprise", SoundType.Voice, Color.red, 2.38f)
            ));
            ArtsAndCrafters crafters = (ArtsAndCrafters)NPCMetaStorage.Instance.Get(Character.Crafters).value;
            SurpriseNpc.AddVisual(new SurpriseNpcVisualRenderer(
                crafters, crafters.angrySprite,
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NpcSurprises/CFT_Parrot"), "Vfx_Crafters_Parrot", SoundType.Voice, Color.white)
            ));
            LookAtGuy theTest = (LookAtGuy)NPCMetaStorage.Instance.Get(Character.LookAt).value;
            Vector3 pos = theTest.headTransform.localPosition;
            pos.y = theTest.maxHeadHeight;
            theTest.headTransform.localPosition = pos;
            SurpriseNpc.AddVisual(new SurpriseNpcVisualRenderer(
                theTest, theTest.audActivate
            ));
            NPC npc = NPCMetaStorage.Instance.Get(Character.Bully).value;
            SurpriseNpc.AddVisual(new SurpriseNpcVisualSprite(
                npc,
                AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NpcOverlays/Bully").OverlayTexture(npc.spriteRenderer[0].sprite, Color.green), 26f),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NpcSurprises/BUL_Gift"), "Vfx_Bully_Gift", SoundType.Voice, new(1f, 162 / 255f, 0f), 6.84f)
            ));
            npc = NPCMetaStorage.Instance.Get(Character.Sweep).value;
            SurpriseNpc.AddVisual(new SurpriseNpcVisualSprite(
                npc,
                AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NpcOverlays/GottaSweep").OverlayTexture(npc.spriteRenderer[0].sprite), 26f),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NpcSurprises/GS_Surprise"), "Vfx_Sweep_Surprise", SoundType.Voice, new(0f, 159 / 255f, 16 / 255f), 2.42f)
            ));
            SurpriseNpc.AddVisual(new SurpriseNpcVisualRenderer(
                NPCMetaStorage.Instance.Get(Character.Prize).value,
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NpcSurprises/1PR_Surprise"), "Vfx_Prize_Surprise", SoundType.Voice, new(0f, 223 / 255f, 1f), 2.44f)
            ));

            SurpriseNpc.AddVisual(new SurpriseNpcVisualRenderer(NPCMetaStorage.Instance.Get(Character.Cumulo).value));
            SurpriseNpc.AddVisual(new SurpriseNpcVisualRenderer(NPCMetaStorage.Instance.Get(Character.Pomp).value));
            SurpriseNpc.AddVisual(new SurpriseNpcVisualRenderer(NPCMetaStorage.Instance.Get(Character.DrReflex).value));
            SurpriseNpc.AddVisual(new SurpriseNpcVisualRenderer(NPCMetaStorage.Instance.Get(Character.Beans).value));
        }

        private void LoadPartyWinLevel()
        {
            PlaceholderWinManager placeholderWin = AssetFinder.FindAllOfType<PlaceholderWinManager>(true).First();

            PartyWinManager winManager = new BaseGameManagerBuilder<PartyWinManager>()
                .SetObjectName("PartyWinManager")
                .SetNPCSpawnMode(GameManagerNPCAutomaticSpawn.Never)
                .SetLevelFinishDelay(10)
                .Build();
            winManager.endingEnvironment = AssetFinder.FindAllOfType<EnvironmentController>(true).First();
            winManager.levelLoader = AssetFinder.FindAllOfType<LevelLoader>(true).First();
            winManager.endingLevel = ObjectCreation.LevelAssetFromPath("Secret", "PartyEndingTop.bpl");
            winManager.blackScreen = GameObject.Instantiate(placeholderWin.blackScreen, winManager.transform);

            winManager.promptScreen = GameObject.Instantiate(placeholderWin.endingError, winManager.transform);
            winManager.promptScreen.name = "PromptScreen";
            winManager.promptScreen.GetComponentInChildren<Image>().sprite = AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1, "Textures", "Gui", "BooScaryImage.png");
            winManager.promptText = winManager.promptScreen.GetComponentInChildren<TMP_Text>();
            winManager.promptText.alignment = TextAlignmentOptions.TopLeft;
            winManager.promptText.fontSize = 24;
            winManager.promptText.font = AssetFinder.FindOfTypeWithName<TMP_FontAsset>("SANS_SERIF_24_Pro", true);
            winManager.promptText.color = Color.white;

            winManager.winScreen = GameObject.Instantiate(placeholderWin.endingError, winManager.transform);
            winManager.winScreen.name = "WinScreen";
            winManager.winScreen.GetComponentInChildren<Image>().sprite = AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1, "Textures", "Gui", "WinImage.png");
            TMP_Text winText = winManager.winScreen.GetComponentInChildren<TMP_Text>();
            ((RectTransform)winText.transform).sizeDelta = new(480, 340);
            winText.fontSize = 36;
            winText.font = BaldiFonts.ComicSans36.FontAsset();
            winText.color = Color.white;

            winManager.audBlow = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "BlowCandle.wav"), "", SoundType.Effect, Color.white, 0);
            winManager.audWow = AssetMan.Get<SoundObject>("Sfw/BaldWow");
            winManager.endingLevel.randomGenStructures = [ObjMan.Get<StructureWithParameters>("Strct/PartyWinBaldloonSpawner")];

            SceneObject scene = ObjectCreation.SceneObjectFromPath("Secret", "PartyEnding.bpl");
            scene.manager = winManager;
            winManager.endingLevelExtra = scene.extraAsset;
            winManager.glambience = ScriptableObject.CreateInstance<LoopingSoundObject>();
            winManager.glambience.clips = [AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "Glambience_Siren.ogg")];
            scene.levelNo = 99;
            scene.skippable = false;
            scene.usesMap = false;
            scene.skybox = LevelLoaderPlugin.Instance.skyboxAliases["twilight"];

            scene.AddMeta(BasePlugin, ["ending", "found_on_main"]);
            ObjMan.Add("Scene/PartyWin", scene);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Misc", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Misc", GenerationStageFlags.Addend);
            lvl.MarkAsNeverUnload();

            // Spawn the BEE in literally any floor
            lvl.posters = lvl.posters.AddToArray(ObjMan.Get<PosterObject>("Pst/bee").Weighted(1));
        }

        //[CaudexGenModEvent(GenerationModType.Finalizer)]
        private void FloorFinalizer(string title, int num, SceneObject scene)
        {
            if (RecommendedCharsPlugin.PartyMode && scene.nextLevel?.GetMeta()?.tags.Contains("ending") == true)
                scene.nextLevel = ObjMan.Get<SceneObject>("Scene/PartyWin");
        }

        [CaudexGenModEvent(GenerationModType.Finalizer)]
        private void FloorFinalizerLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Misc", GenerationStageFlags.Finalizer))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Misc", GenerationStageFlags.Finalizer);

            if (!RecommendedCharsPlugin.PartyMode) return;

            // Replace cafeterias with cake variants
            List<WeightedRoomAsset> specialRooms = lvl.potentialSpecialRooms.ToList();

            int totalWeight = 0;
            for (int i = 0; i < specialRooms.Count; i++)
            {
                if (!specialRooms[i].selection.roomFunctionContainer ||
                    !specialRooms[i].selection.roomFunctionContainer.name.StartsWith("Cafeteria"))
                    continue;

                totalWeight += specialRooms[i].weight;
                specialRooms.RemoveAt(i);
                i--;
            }
            if (totalWeight == 0)
                return;

            int weightPerCafe = totalWeight/newCafeterias.Length;
            foreach (WeightedRoomAsset cafeteria in newCafeterias)
                specialRooms.Add(cafeteria.selection.Weighted(weightPerCafe)); // Add the party cafeterias
                
            lvl.potentialSpecialRooms = specialRooms.ToArray();
        }
    }
}
