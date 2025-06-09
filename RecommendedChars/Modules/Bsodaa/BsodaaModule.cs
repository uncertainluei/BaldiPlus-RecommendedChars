using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UnityEngine;
using MTM101BaldAPI.Components;
using System.Linq;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class Module_Bsodaa : Module
    {
        public override string Name => "Eveyone's Bsodaa";

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleExp;

        public override Action LoadAction => Load;
        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;

        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Room", "Bsodaa"), x => "BsodaaRoom/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "Bsodaa"), x => "BsodaaTex/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(Plugin), "Audio", "Bsodaa"), "BsodaaAud/");

            LoadBsodaaHelper();
            LoadEveyBsodaa();
        }

        private void LoadBsodaaHelper()
        {
            // Essentially this other guy will not be like the below guy, as in she's a glorified structure rather than an
            // NPC.
        }

        private void LoadEveyBsodaa()
        {
            EveyBsodaa bsodaaGuy = new NPCBuilder<EveyBsodaa>(Info)
                .SetName("Bsodaa")
                .SetEnum("RecChars_Bsodaa")
                .SetPoster(AssetMan.Get<Texture2D>("BsodaaTex/pri_bsodaa"), "RecChars_Pst_Bsodaa1", "RecChars_Pst_Bsodaa2")
                .AddMetaFlag(NPCFlags.Standard)
                .AddPotentialRoomAsset(LoadBsodaaRoom(), 100)
                .AddLooker()
                .AddTrigger()
                .IgnorePlayerOnSpawn()
                .Build();

            Sprite[] sprites = RecommendedCharsPlugin.SplitSpriteSheet(AssetMan.Get<Texture2D>("BsodaaTex/Bsodaa_Idle"), 106, 256, 3, 32f);

            bsodaaGuy.spriteRenderer[0].transform.localPosition = Vector3.up * -1.08f;
            bsodaaGuy.spriteRenderer[0].sprite = sprites[0];

            EveyBsodaa.animations = new Dictionary<string, Sprite[]>()
            {
                {"Idle", new Sprite[] { sprites[0] }},
                {"Happy", new Sprite[] { sprites[1] }},
                {"Upset", new Sprite[] { sprites[2] }},
            };

            sprites = RecommendedCharsPlugin.SplitSpriteSheet(AssetMan.Get<Texture2D>("BsodaaTex/Bsodaa_Shoot"), 106, 256, 6, 32f);

            EveyBsodaa.animations.Add("Charge", new Sprite[]
            {
                sprites[4],
                sprites[4],
                sprites[4],
                sprites[4],
                sprites[3],
                sprites[2],
                sprites[1],
                sprites[0]
            });
            EveyBsodaa.animations.Add("Shoot", new Sprite[]
            {
                sprites[5],
                sprites[5],
                sprites[5],
                bsodaaGuy.spriteRenderer[0].sprite
            });

            bsodaaGuy.animator = bsodaaGuy.gameObject.AddComponent<CustomSpriteAnimator>();
            bsodaaGuy.animator.spriteRenderer = bsodaaGuy.spriteRenderer[0];

            bsodaaGuy.audMan = bsodaaGuy.GetComponent<AudioManager>();
            bsodaaGuy.audMan.subtitleColor = new Color(3f/255f, 36f/255f, 1f);

            bsodaaGuy.audCharging = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/Evey_Charging"), "RecChars_Bsodaa_Charging", SoundType.Effect, bsodaaGuy.audMan.subtitleColor);
            bsodaaGuy.audReloaded = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/Evey_Thanks"), "RecChars_Bsodaa_Thanks", SoundType.Voice, bsodaaGuy.audMan.subtitleColor);

            bsodaaGuy.audSuccess = new SoundObject[]
            {
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/Evey_Happy1"), "RecChars_Bsodaa_Happy1", SoundType.Voice, bsodaaGuy.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BsodaaAud/Evey_Happy2"), "RecChars_Bsodaa_Happy2", SoundType.Voice, bsodaaGuy.audMan.subtitleColor)
            };

            bsodaaGuy.projectilePre = RecommendedCharsPlugin.CloneComponent<ITM_BSODA, EveyBsodaaSpray>(GameObject.Instantiate((ITM_BSODA)ItemMetaStorage.Instance.FindByEnum(Items.Bsoda).value.item, MTM101BaldiDevAPI.prefabTransform));
            bsodaaGuy.projectilePre.name = "Bsodaa_Spray";

            PineDebugNpcIconPatch.icons.Add(bsodaaGuy.Character, AssetMan.Get<Texture2D>("BsodaaTex/BorderBsodaa"));

            AssetMan.Add("BsodaaNpc", bsodaaGuy);
        }

        private RoomAsset LoadBsodaaRoom()
        {
            RoomAsset bsodaaRoomAsset = RoomAsset.CreateInstance<RoomAsset>();
            ((ScriptableObject)bsodaaRoomAsset).name = "Room_Bsodaa_0";
            bsodaaRoomAsset.name = "Bsodaa_0";

            bsodaaRoomAsset.type = RoomType.Room;
            bsodaaRoomAsset.category = EnumExtensions.ExtendEnum<RoomCategory>("RecChars_Bsodaa");

            bsodaaRoomAsset.florTex = AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaCarpet");
            bsodaaRoomAsset.wallTex = AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaWall");
            bsodaaRoomAsset.ceilTex = AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaCeiling");

            bsodaaRoomAsset.doorMats = ObjectCreators.CreateDoorDataObject("BsodaaDoor", AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaDoor_Open"), AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaDoor_Closed"));
            bsodaaRoomAsset.keepTextures = true;

            bsodaaRoomAsset.lightPre = GameObject.Instantiate(((DrReflex)NPCMetaStorage.Instance.Get(Character.DrReflex).value).potentialRoomAssets[0].selection.lightPre, MTM101BaldiDevAPI.prefabTransform);
            bsodaaRoomAsset.lightPre.name = "BsodaaLight";

            MeshRenderer light = bsodaaRoomAsset.lightPre.GetComponentInChildren<MeshRenderer>();
            light.sharedMaterial = new Material(light.sharedMaterial)
            {
                name = "BsodaaRoom_Light",
                mainTexture = AssetMan.Get<Texture2D>("BsodaaRoom/BsodaaLight")
            };

            bsodaaRoomAsset.mapMaterial = ObjectCreators.CreateMapTileShader(AssetMan.Get<Texture2D>("BsodaaRoom/Map_Bsodaa"));
            bsodaaRoomAsset.color = new Color(57f/255f, 87f/255f, 159f/255f);

            bsodaaRoomAsset.cells = new List<CellData>()
            {
                RoomAssetHelper.Cell(0,0,12),
                RoomAssetHelper.Cell(1,0,4),
                RoomAssetHelper.Cell(2,0,6),
                RoomAssetHelper.Cell(0,1,9),
                RoomAssetHelper.Cell(1,1,0),
                RoomAssetHelper.Cell(2,1,3),
                RoomAssetHelper.Cell(1,2,11)
            };

            bsodaaRoomAsset.posterChance = 0.1f;
            bsodaaRoomAsset.posters = new List<WeightedPosterObject>();

            bsodaaRoomAsset.standardLightCells = new List<IntVector2>() { new IntVector2(1, 1) };
            bsodaaRoomAsset.potentialDoorPositions = new List<IntVector2>()
            {
                new IntVector2(0,0),
                new IntVector2(1,0),
                new IntVector2(2,0)
            };

            bsodaaRoomAsset.entitySafeCells = new List<IntVector2>()
            {
                new IntVector2(1,0),
                new IntVector2(1,1)
            };

            bsodaaRoomAsset.blockedWallCells = new List<IntVector2>()
            {
                new IntVector2(0,1),
                new IntVector2(2,1)
            };

            SodaMachine dBsodaMachine = Resources.FindObjectsOfTypeAll<SodaMachine>().First(x => x.item.itemType == Items.DietBsoda && x.GetInstanceID() >= 0);
            bsodaaRoomAsset.basicObjects = new List<BasicObjectData>()
            {
                RoomAssetHelper.ObjectPlacement(dBsodaMachine, new Vector3(5f,0f,15f), 0f),
                RoomAssetHelper.ObjectPlacement(dBsodaMachine, new Vector3(25f,0f,15f), 0f)
            };

            //DetentionRoomFunction detention = Resources.FindObjectsOfTypeAll<DetentionRoomFunction>().First(x => x.name == "OfficeRoomFunction" && x.GetInstanceID() >= 0);
            //DaycareRoomFunction roomFunction = RecommendedCharsPlugin.CloneComponent<DetentionRoomFunction, DaycareRoomFunction>(GameObject.Instantiate(detention, MTM101BaldiDevAPI.prefabTransform));
            //roomFunction.name = "DaycareRoomFunction";

            //roomFunction.gaugeSprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("DaycareTex/TimeoutIcon"), 25f);

            //bsodaaRoomAsset.roomFunctionContainer = roomFunction.GetComponent<RoomFunctionContainer>();
            //GameObject.DestroyImmediate(bsodaaRoomAsset.roomFunctionContainer.GetComponent<CharacterPostersRoomFunction>());
            //bsodaaRoomAsset.roomFunctionContainer.functions = new List<RoomFunction>()
            //{
            //    bsodaaRoomAsset.roomFunctionContainer .GetComponent<RuleFreeZone>(),
            //    roomFunction,
            //    bsodaaRoomAsset.roomFunctionContainer .GetComponent<CoverRoomFunction>()
            //};

            return bsodaaRoomAsset;
        }

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
                    scene.potentialNPCs.CopyCharacterWeight(Character.DrReflex, AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
                return;
            }

            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.potentialNPCs.CopyCharacterWeight(Character.DrReflex, AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
                }
                else if (id == 1)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<EveyBsodaa>("BsodaaNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
            }
        }
    }
}
