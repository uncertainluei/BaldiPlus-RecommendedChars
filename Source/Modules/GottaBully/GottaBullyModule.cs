using BepInEx.Configuration;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelLoader;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class Module_GottaBully : Module
    {
        public override string Name => "Gotta Bully";

        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleGottaBully;

        [ModuleLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Room", "SwapCloset"), x => "SwapCloset/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "GottaBully"), x => "GottaBullyTex/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(Plugin), "Audio", "GottaBully"), "GottaBullyAud/");

            CreateSwapClosetBlueprint();
            LoadGottaBully();
        }

        private void LoadGottaBully()
        {
            GottaBully gottaBully = RecommendedCharsPlugin.CloneComponent<GottaSweep, GottaBully>(GameObject.Instantiate((GottaSweep)NPCMetaStorage.Instance.Get(Character.Sweep).value, MTM101BaldiDevAPI.prefabTransform));
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

            AssetMan.Add("GottaBullyNpc", gottaBully);
            NPCMetadata gottaBullyMeta = new(Info, [gottaBully], gottaBully.name, NPCMetaStorage.Instance.Get(Character.Sweep).flags | NPCFlags.MakeNoise, ["adv_first_prize_immunity", "adv_exclusion_hammer_weakness"]);
            NPCMetaStorage.Instance.Add(gottaBullyMeta);
        }

        private void CreateSwapClosetBlueprint()
        {
            RoomBlueprint bullyRoom = new("SwappedCloset", "RecChars_SwappedCloset");

            bullyRoom.texFloor = AssetMan.Get<Texture2D>("SwapCloset/SwappedFloor");
            bullyRoom.texWall = AssetMan.Get<Texture2D>("SwapCloset/SwappedWall");
            bullyRoom.texCeil = Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Carpet" && x.GetInstanceID() >= 0);
            bullyRoom.keepTextures = true;

            bullyRoom.doorMats = ObjectCreators.CreateDoorDataObject("SwapDoor", AssetMan.Get<Texture2D>("SwapCloset/SwappedDoor_Open"), AssetMan.Get<Texture2D>("SwapCloset/SwappedDoor_Closed"));

            bullyRoom.lightObj = ((DrReflex)NPCMetaStorage.Instance.Get(Character.DrReflex).value).potentialRoomAssets[0].selection.lightPre;
            bullyRoom.color = new(198/255f, 136/255f, 91/255f);

            PosterObject tapliasmyChalkboard = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("SwapCloset/SubToTapliasmy"), []);
            tapliasmyChalkboard.name = "NotAChk_SubToTapliasmy";
            List<WeightedPosterObject> bullyRoomPoster =
            [
                new WeightedPosterObject() { weight = 100, selection = tapliasmyChalkboard}
            ];
            bullyRoom.posterChance = 0.25f;
            bullyRoom.posters = bullyRoomPoster;

            AssetMan.Add("SwapClosetBlueprint", bullyRoom);
        }

        private WeightedRoomAsset[] CreateSwapClosetRooms()
        {
            RoomBlueprint blueprint = AssetMan.Get<RoomBlueprint>("SwapClosetBlueprint");

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

        [ModuleCompatLoadEvent(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Pre)]
        private void AnimationsCompat()
        {
            GameObject.DestroyImmediate(AssetMan.Get<GottaBully>("GottaBullyNpc").GetComponent<BBPlusAnimations.Components.GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(AssetMan.Get<GottaBully>("GottaBullyNpc").GetComponent<BBPlusAnimations.Components.GottaSweepComponent>());
        }

        [ModuleCompatLoadEvent(RecommendedCharsPlugin.LevelLoaderGuid, LoadingEventOrder.Pre)]
        private void RegisterToLevelLoader()
        {
            PlusLevelLoaderPlugin.Instance.npcAliases.Add("recchars_gottabully", AssetMan.Get<GottaBully>("GottaBullyNpc"));

            RoomBlueprint blueprint = AssetMan.Get<RoomBlueprint>("SwapClosetBlueprint");
            PlusLevelLoaderPlugin.Instance.textureAliases.Add("recchars_swapwall", blueprint.texWall);
            PlusLevelLoaderPlugin.Instance.textureAliases.Add("recchars_swapceil", blueprint.texCeil);

            PlusLevelLoaderPlugin.Instance.roomSettings.Add("recchars_swapcloset", new(blueprint.category, blueprint.type, blueprint.color, blueprint.doorMats));
        }

        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();
                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                    scene.potentialNPCs.CopyCharacterWeight(Character.LookAt, AssetMan.Get<GottaBully>("GottaBullyNpc"));
                else
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<GottaBully>("GottaBullyNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
                return;
            }

            if (title.StartsWith("F") && id > 1)
            {
                scene.MarkAsNeverUnload();

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                    scene.potentialNPCs.CopyCharacterWeight(Character.LookAt, AssetMan.Get<GottaBully>("GottaBullyNpc"));
                else if (id == 2)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<GottaBully>("GottaBullyNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
            }
        }
    }
}
