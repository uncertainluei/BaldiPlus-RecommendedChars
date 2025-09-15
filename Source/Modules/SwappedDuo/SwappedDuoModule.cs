using BepInEx.Configuration;

using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;

using System;
using System.IO;
using System.Linq;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;
using UnityEngine.UI;
using UncertainLuei.CaudexLib.Util.Extensions;
using PlusStudioLevelLoader;
using UncertainLuei.CaudexLib.Objects;
using System.Collections.Generic;
using UncertainLuei.CaudexLib.Util;
using BBPlusAnimations.Components;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelLoader;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Swapped Duo"), CaudexModuleSaveTag("Mdl_SwappedDuo")]
    [CaudexModuleConfig("Modules", "SwappedDuo",
        "Adds Gotta Bully and Arts with Wires from Playtime's Swapped Basics (and 1st Prize's Mania respectively).", true)]
    public sealed class Module_SwappedDuo : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Room", "SwapCloset"), x => "SwapCloset/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Npc", "GottaBully"), x => "GottaBullyTex/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Npc", "ArtsWWires"), x => "WiresTex/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(BasePlugin), "Audio", "ArtsWWires"), "WiresAud/");
            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(BasePlugin), "Audio", "GottaBully"), "GottaBullyAud/");
            
            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "SwappedDuo.json5");

            // Load patches
            Hooks.PatchAll(typeof(SwappedDuoPatches));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            // Tapliasmy Chalkboard
            PosterObject tapliasmyChalkboard = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("SwapCloset/SubToTapliasmy"), []);
            tapliasmyChalkboard.name = "Pst_Sub2Tapliasmy";
            ObjMan.Add("Pst_Sub2Tapliasmy", tapliasmyChalkboard);

            CreateSwapClosetBlueprint();
            LoadGottaBully();
            LoadArtsWithWires();
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Pre)]
        private void GottaBullyAnimationsCompat()
        {
            GameObject.DestroyImmediate(ObjMan.Get<GottaBully>("Npc_GottaBully").GetComponent<GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(ObjMan.Get<GottaBully>("Npc_GottaBully").GetComponent<GottaSweepComponent>());
        }

        private void LoadGottaBully()
        {
            GottaBully gottaBully = RecommendedCharsPlugin.SwapComponentSimple<GottaSweep, GottaBully>(GameObject.Instantiate((GottaSweep)NPCMetaStorage.Instance.Get(Character.Sweep).value, MTM101BaldiDevAPI.prefabTransform));
            gottaBully.name = "GottaBully";

            gottaBully.character = EnumExtensions.ExtendEnum<Character>("RecChars_GottaBully");
            PineDebugNpcIconPatch.icons.Add(gottaBully.character, AssetMan.Get<Texture2D>("GottaBullyTex/BorderGottaBully"));

            gottaBully.ignorePlayerOnSpawn = true;

            gottaBully.looker.npc = gottaBully;
            gottaBully.navigator.npc = gottaBully;

            gottaBully.speed *= 0.9f;
            gottaBully.minDelay = 150f;
            gottaBully.maxDelay = 250f;

            // Reference for item rejection (yeah that's pretty much it)
            gottaBully.bullyReference = (Bully)NPCMetaStorage.Instance.Get(Character.Bully).value;

            gottaBully.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("GottaBullyTex/pri_gbully"), "PST_PRI_RecChars_GBully1", "PST_PRI_RecChars_GBully2");
            gottaBully.poster.name = "GottaBullyPoster";

            gottaBully.spriteRenderer[0].sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("GottaBullyTex/GottaBully"), 26f);
            gottaBully.audMan.subtitleColor = new(198/255f, 136/255f, 91/255f);
            CharacterRadarColorPatch.colors.Add(gottaBully.character, gottaBully.audMan.subtitleColor);

            gottaBully.audIntro = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GottaBullyAud/GB_Intro"), "Vfx_RecChars_GBully_Intro", SoundType.Voice, gottaBully.audMan.subtitleColor);
            gottaBully.audSweep = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GottaBullyAud/GB_GottaBully"), "Vfx_RecChars_GBully_Sweep", SoundType.Voice, gottaBully.audMan.subtitleColor);

            gottaBully.potentialRoomAssets = CreateSwapClosetRooms();

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_gottabully", gottaBully);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_gbully", gottaBully.Poster);
            ObjMan.Add("Npc_GottaBully", gottaBully);
            NPCMetadata gottaBullyMeta = new(Plugin, [gottaBully], gottaBully.name, NPCMetaStorage.Instance.Get(Character.Sweep).flags | NPCFlags.MakeNoise, ["adv_first_prize_immunity", "adv_exclusion_hammer_weakness"]);
            NPCMetaStorage.Instance.Add(gottaBullyMeta);
        }

        private void CreateSwapClosetBlueprint()
        {
            CaudexRoomBlueprint bullyRoom = new(Plugin, "SwappedCloset", "RecChars_SwappedCloset");

            bullyRoom.texFloor = AssetMan.Get<Texture2D>("SwapCloset/SwappedFloor");
            bullyRoom.texWall = AssetMan.Get<Texture2D>("SwapCloset/SwappedWall");
            bullyRoom.texCeil = Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Carpet" && x.GetInstanceID() >= 0);
            bullyRoom.keepTextures = true;

            bullyRoom.doorMats = ObjectCreators.CreateDoorDataObject("SwapDoor", AssetMan.Get<Texture2D>("SwapCloset/SwappedDoor_Open"), AssetMan.Get<Texture2D>("SwapCloset/SwappedDoor_Closed"));

            bullyRoom.lightObj = ((DrReflex)NPCMetaStorage.Instance.Get(Character.DrReflex).value).potentialRoomAssets[0].selection.lightPre;
            bullyRoom.color = new(198/255f, 136/255f, 91/255f);

            List<WeightedPosterObject> bullyRoomPoster =
            [ObjMan.Get<PosterObject>("Pst_Sub2Tapliasmy").Weighted(100)];
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_sub2tapliasmy", bullyRoomPoster[0].selection);
            bullyRoom.posterChance = 0.25f;
            bullyRoom.posters = bullyRoomPoster;

            LevelLoaderCompatHelper.AddRoom(bullyRoom, "recchars_swapcloset");
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_swapflor", bullyRoom.texFloor);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_swapwall", bullyRoom.texWall);
            AssetMan.Add("SwapClosetBlueprint", bullyRoom);
        }

        private WeightedRoomAsset[] CreateSwapClosetRooms()
        {
            CaudexRoomBlueprint blueprint = AssetMan.Get<CaudexRoomBlueprint>("SwapClosetBlueprint");

            List<WeightedRoomAsset> rooms = [];

            RoomAsset newRoom = blueprint.CreateAsset("Sugary0");
            rooms.Add(newRoom.Weighted(100));
            newRoom.cells = RoomAssetHelper.CellRect(2, 2);
            newRoom.standardLightCells = [new(0, 0)];
            newRoom.potentialDoorPositions =
            [
                new(0, 0),
                new(1, 0)
            ];
            newRoom.entitySafeCells =
            [
                new(0, 1),
                new(1, 1)
            ];

            newRoom = blueprint.CreateAsset("Sugary1");
            rooms.Add(newRoom.Weighted(100));
            newRoom.cells = RoomAssetHelper.CellRect(1, 4);
            newRoom.standardLightCells = [new(0, 1)];
            newRoom.potentialDoorPositions =
            [
                new(0, 0),
                new(0, 3)
            ];
            newRoom.entitySafeCells =
            [
                new(0, 1),
                new(0, 2)
            ];

            // Turn
            newRoom = blueprint.CreateAsset("Luei0");
            rooms.Add(newRoom.Weighted(100));
            newRoom.cells =
            [
                RoomAssetHelper.Cell(0, 0, 13),
                RoomAssetHelper.Cell(1, 0, 6),
                RoomAssetHelper.Cell(1, 1, 11),
            ];
            newRoom.standardLightCells = [new(1, 0)];
            newRoom.potentialDoorPositions =
            [
                new(0, 0),
                new(1, 1)
            ];
            newRoom.entitySafeCells = [new(1, 0)];

            // "Plus" shape
            newRoom = blueprint.CreateAsset("Luei1");
            rooms.Add(newRoom.Weighted(100));
            newRoom.cells =
            [
                RoomAssetHelper.Cell(1, 0, 14),

                RoomAssetHelper.Cell(0, 1, 13),
                RoomAssetHelper.Cell(1, 1, 0),
                RoomAssetHelper.Cell(2, 1, 7),

                RoomAssetHelper.Cell(1, 2, 11),

            ];
            newRoom.standardLightCells = [new(1, 1)];
            newRoom.potentialDoorPositions =
            [
                new(1, 0),
                new(0, 1),
                new(2, 1),
                new(1, 2)
            ];
            newRoom.entitySafeCells = [new(1, 1)];

            return [.. rooms];
        }

        private void LoadArtsWithWires()
        {
            ArtsWithWires artsWithWires = new NPCBuilder<ArtsWithWires>(Plugin)
                .SetName("ArtsWithWires")
                .SetEnum("RecChars_ArtsWithWires")
                .SetPoster(AssetMan.Get<Texture2D>("WiresTex/pri_wires"), "PST_PRI_RecChars_Wires1", "PST_PRI_RecChars_Wires2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(["adv_exclusion_hammer_weakness"])
                .AddLooker()
                .AddTrigger()
                .Build();

            PineDebugNpcIconPatch.icons.Add(artsWithWires.character, AssetMan.Get<Texture2D>("WiresTex/BorderWires"));

            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 50f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("WiresTex/WiresSprites"));

            artsWithWires.sprite = artsWithWires.spriteRenderer[0];
            artsWithWires.sprite.transform.localPosition = Vector3.zero;

            artsWithWires.sprite.sprite = sprites[0];
            artsWithWires.sprNormal = sprites[0];
            artsWithWires.sprAngry = sprites[1];

            artsWithWires.audMan = artsWithWires.GetComponent<AudioManager>();
            artsWithWires.audMan.subtitleColor = new(138/255f, 22/255f, 15/255f);

            CharacterRadarColorPatch.colors.Add(artsWithWires.character, artsWithWires.audMan.subtitleColor);

            artsWithWires.audIntro = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("WiresAud/AWW_Intro"), "Sfx_RecChars_Wires_Intro", SoundType.Effect, artsWithWires.audMan.subtitleColor);
            artsWithWires.audLoop = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("WiresAud/AWW_Loop"), "Sfx_RecChars_Wires_Loop", SoundType.Effect, artsWithWires.audMan.subtitleColor);

            artsWithWires.stareStacks = RecommendedCharsConfig.intendedWiresBehavior.Value;

            Jumprope jumpropeCopy = GameObject.Instantiate(((Playtime)NPCMetaStorage.Instance.Get(Character.Playtime).value).jumpropePre, MTM101BaldiDevAPI.prefabTransform);
            jumpropeCopy.name = "ArtsWithWires GrabbingGame";
            jumpropeCopy.enabled = false;

            artsWithWires.gamePrefab = jumpropeCopy.gameObject.AddComponent<GrabbingGame>();
            artsWithWires.gamePrefab.textCanvas = jumpropeCopy.textCanvas;
            artsWithWires.gamePrefab.textScaler = jumpropeCopy.textScaler;
            artsWithWires.gamePrefab.instructionsTmp = jumpropeCopy.instructionsTmp;
            artsWithWires.gamePrefab.instructionsTmp.rectTransform.anchoredPosition = Vector2.up * 32f;

            GameObject.DestroyImmediate(jumpropeCopy.countTmp.gameObject);
            GameObject.DestroyImmediate(jumpropeCopy.ropeCanvas.gameObject);
            GameObject.DestroyImmediate(jumpropeCopy);

            Sprite background = Sprite.Create(AssetMan.Get<Texture2D>("WiresTex/WiresGrabMeter"), new Rect(0, 0, 136, 24), Vector2.one / 2f);
            background.name = "WiresGrabMeter_Background";

            artsWithWires.gamePrefab.needle = UIHelpers.CreateImage(background, artsWithWires.gamePrefab.textCanvas.transform, default, false).rectTransform;
            artsWithWires.gamePrefab.needle.anchorMin = Vector2.one / 2f;
            artsWithWires.gamePrefab.needle.anchorMax = artsWithWires.gamePrefab.needle.anchorMin;
            artsWithWires.gamePrefab.needle.pivot = artsWithWires.gamePrefab.needle.anchorMin;
            artsWithWires.gamePrefab.needle.anchoredPosition = Vector2.up * -32f;
            artsWithWires.gamePrefab.needle.name = "Meter_BG";

            background = Sprite.Create(AssetMan.Get<Texture2D>("WiresTex/WiresGrabMeter"), new Rect(136, 0, 8, 24), Vector2.one / 2f);
            background.name = "WiresGrabMeter_Needle";

            artsWithWires.gamePrefab.needle = GameObject.Instantiate(artsWithWires.gamePrefab.needle, artsWithWires.gamePrefab.needle.parent, false);
            artsWithWires.gamePrefab.needle.GetComponent<Image>().sprite = background;
            artsWithWires.gamePrefab.needle.sizeDelta = new Vector2(8f, 24f);
            artsWithWires.gamePrefab.needle.name = "Meter_Needle";

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_artswithwires", artsWithWires);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_wires", artsWithWires.Poster);
            ObjMan.Add("Npc_ArtsWithWires", artsWithWires);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();

                if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<ArtsWithWires>("Npc_ArtsWithWires"));
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<GottaBully>("Npc_GottaBully"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-2, 0);
                }
                else
                {
                    scene.potentialNPCs.CopyNpcWeight(Character.LookAt, ObjMan.Get<GottaBully>("Npc_GottaBully"));
                    scene.potentialNPCs.CopyNpcWeight(Character.DrReflex, ObjMan.Get<ArtsWithWires>("Npc_ArtsWithWires"));
                }
                return;
            }

            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    if (id > 1)
                        scene.potentialNPCs.CopyNpcWeight(Character.LookAt, ObjMan.Get<GottaBully>("Npc_GottaBully"));

                    scene.potentialNPCs.CopyNpcWeight(Character.DrReflex, ObjMan.Get<ArtsWithWires>("Npc_ArtsWithWires"));
                    return;
                }
                if (id == 1)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<ArtsWithWires>("Npc_ArtsWithWires"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-1, 0);
                    return;
                }
                if (id == 2)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<GottaBully>("Npc_GottaBully"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-1, 0);
                    return;
                }
            }
        }
    }
}
