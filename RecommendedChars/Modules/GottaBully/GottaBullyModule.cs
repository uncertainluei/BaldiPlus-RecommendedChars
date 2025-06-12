using BepInEx.Configuration;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

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

        public override Action LoadAction => Load;
        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleGottaBully;

        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Room", "SwapCloset"), x => "SwapCloset/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "GottaBully"), x => "GottaBullyTex/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(Plugin), "Audio", "GottaBully"), "GottaBullyAud/");

            GottaBully gottaBully = LoadGottaBully();
            LoadSwapCloset(gottaBully);

            if (RecommendedCharsPlugin.AnimationsCompat)
                AnimationsCompat();
        }

        private GottaBully LoadGottaBully()
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

            gottaBully.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("GottaBullyTex/pri_gbully"), "RecChars_Pst_GottaBully1", "RecChars_Pst_GottaBully2");
            gottaBully.poster.name = "GottaBullyPoster";

            gottaBully.spriteRenderer[0].sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("GottaBullyTex/GottaBully"), 26f);
            gottaBully.audMan.subtitleColor = new Color(198f / 255f, 136f / 255f, 91f / 255f);
            gottaBully.audIntro = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GottaBullyAud/GB_Intro"), "RecChars_GottaBully_Intro", SoundType.Voice, gottaBully.audMan.subtitleColor);
            gottaBully.audSweep = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GottaBullyAud/GB_GottaBully"), "RecChars_GottaBully_Sweep", SoundType.Voice, gottaBully.audMan.subtitleColor);

            AssetMan.Add("GottaBullyNpc", gottaBully);
            NPCMetadata gottaBullyMeta = new NPCMetadata(Info, new NPC[] { gottaBully }, gottaBully.name, NPCMetaStorage.Instance.Get(Character.Sweep).flags | NPCFlags.MakeNoise, new string[0]);
            NPCMetaStorage.Instance.Add(gottaBullyMeta);
            return gottaBully;
        }

        private void LoadSwapCloset(GottaBully gottaBully)
        {
            RoomAsset bullyRoomAsset = RoomAsset.CreateInstance<RoomAsset>();
            ((ScriptableObject)bullyRoomAsset).name = "Room_SwappedCloset_0";
            bullyRoomAsset.name = "SwappedCloset_0";
            bullyRoomAsset.category = EnumExtensions.ExtendEnum<RoomCategory>("RecChars_SwappedCloset");
            bullyRoomAsset.hasActivity = false;
            bullyRoomAsset.posterChance = 0.25f;

            bullyRoomAsset.ceilTex = Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Carpet" && x.GetInstanceID() >= 0);
            bullyRoomAsset.florTex = AssetMan.Get<Texture2D>("SwapCloset/SwappedFloor");
            bullyRoomAsset.wallTex = AssetMan.Get<Texture2D>("SwapCloset/SwappedWall");
            bullyRoomAsset.doorMats = ObjectCreators.CreateDoorDataObject("SwapDoor", AssetMan.Get<Texture2D>("SwapCloset/SwappedDoor_Open"), AssetMan.Get<Texture2D>("SwapCloset/SwappedDoor_Closed"));
            bullyRoomAsset.keepTextures = true;
            bullyRoomAsset.potentialDoorPositions = new List<IntVector2>() { new IntVector2(0, 0) };

            bullyRoomAsset.cells = RoomAssetHelper.CellRect(2, 2);

            bullyRoomAsset.standardLightCells.Add(new IntVector2(0, 0));
            bullyRoomAsset.entitySafeCells.Add(new IntVector2(0, 1));
            bullyRoomAsset.eventSafeCells.Add(new IntVector2(0, 0));
            bullyRoomAsset.lightPre = gottaBully.potentialRoomAssets[0].selection.lightPre;
            bullyRoomAsset.color = gottaBully.audMan.subtitleColor;

            PosterObject tapliasmyChalkboard = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("SwapCloset/SubToTapliasmy"), new PosterTextData[0]);
            tapliasmyChalkboard.name = "NotAChk_SubToTapliasmy";
            List<WeightedPosterObject> bullyRoomPoster = new List<WeightedPosterObject>
            {
                new WeightedPosterObject() { weight = 100, selection = tapliasmyChalkboard}
            };
            bullyRoomAsset.posters = bullyRoomPoster;

            gottaBully.potentialRoomAssets = new WeightedRoomAsset[]
            {
                new WeightedRoomAsset() { weight = 100, selection = bullyRoomAsset },
                new WeightedRoomAsset() { weight = 100 }
            };

            bullyRoomAsset = GameObject.Instantiate(bullyRoomAsset);
            ((ScriptableObject)bullyRoomAsset).name = "Room_SwappedCloset_1";
            bullyRoomAsset.name = "SwappedCloset_1";

            bullyRoomAsset.cells = RoomAssetHelper.CellRect(1, 4);

            bullyRoomAsset.entitySafeCells[0] = new IntVector2(0, 2);
            gottaBully.potentialRoomAssets[1].selection = bullyRoomAsset;
        }

        private void AnimationsCompat()
        {
            GameObject.DestroyImmediate(AssetMan.Get<GottaBully>("GottaBullyNpc").GetComponent<BBPlusAnimations.Components.GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(AssetMan.Get<GottaBully>("GottaBullyNpc").GetComponent<BBPlusAnimations.Components.GottaSweepComponent>());
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
