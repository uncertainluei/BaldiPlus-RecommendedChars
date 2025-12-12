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
    [CaudexModule("Gotta Bully"), CaudexModuleSaveTag("Mdl_GottaBully")]
    [CaudexModuleConfig("Modules", "GottaBully",
        "Adds Gotta Bully and his closet from Playtime's Swapped Basics.", true)]
    public sealed partial class Module_GottaBully : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            AddTexturesToAssetMan("SwapCloset/", ["Textures", "Environment", "Room", "SwapCloset"]);
            AddTexturesToAssetMan("SwapClosetPoster/", ["Textures", "Environment", "Poster", "SwapCloset"]);
            AddTexturesToAssetMan("GottaBullyTex/", ["Textures", "Npc", "GottaBully"]);
            AddAudioToAssetMan("GottaBullyAud/", ["Audio", "Npc", "GottaBully"]);
            
            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "GottaBully.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            // Tapliasmy Chalkboard
            PosterObject tapliasmyChalkboard = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("SwapClosetPoster/SubToTapliasmy"), []);
            tapliasmyChalkboard.name = "Pst_Sub2Tapliasmy";
            ObjMan.Add("Pst_Sub2Tapliasmy", tapliasmyChalkboard);

            CreateSwapClosetBlueprint();
            LoadGottaBully();
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Pre)]
        private void GottaBullyAnimationsCompat()
        {
            GameObject.DestroyImmediate(ObjMan.Get<GottaBully>("Npc_GottaBully").GetComponent<GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(ObjMan.Get<GottaBully>("Npc_GottaBully").GetComponent<GottaSweepComponent>());
        }

        private void LoadGottaBully()
        {
            GottaBully gottaBully = SwapComponentSimple<GottaSweep, GottaBully>(GameObject.Instantiate((GottaSweep)NPCMetaStorage.Instance.Get(Character.Sweep).value, MTM101BaldiDevAPI.prefabTransform));
            gottaBully.name = "GottaBully";

            gottaBully.character = EnumExtensions.ExtendEnum<Character>("RecChars_GottaBully");
            //PineDebugNpcIconPatch.icons.Add(gottaBully.character, AssetMan.Get<Texture2D>("GottaBullyTex/BorderGottaBully"));

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

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_gottabully", gottaBully);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_gbully", gottaBully.Poster);

            gottaBully.potentialRoomAssets = RoomAssetsFromDirectory(ObjMan.Get<CaudexRoomBlueprint>("Room_SwapCloset"), "SwapCloset");

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
            ObjMan.Add("Room_SwapCloset", bullyRoom);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();

                if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<GottaBully>("Npc_GottaBully"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-1, 0);
                }
                else
                    scene.potentialNPCs.CopyNpcWeight(Character.LookAt, ObjMan.Get<GottaBully>("Npc_GottaBully"));
                return;
            }

            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    if (id > 1)
                        scene.potentialNPCs.CopyNpcWeight(Character.LookAt, ObjMan.Get<GottaBully>("Npc_GottaBully"));
                }
                else if (id == 2)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<GottaBully>("Npc_GottaBully"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-1, 0);
                }
            }
        }
    }
}
