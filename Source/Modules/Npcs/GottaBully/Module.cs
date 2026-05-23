using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusStudioLevelLoader;

using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelLoader;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;

using System.Collections.Generic;


namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Gotta Bully"), CaudexModuleSaveTag("Mdl_GottaBully")]
    [CaudexModuleConfig("Modules", "GottaBully",
        "Adds Gotta Bully and his closet from Playtime's Swapped Basics.", true)]
    public sealed class Module_GottaBully : RecCharsModule
    {
        internal override byte IconId => 1;

        protected override void Initialized()
        {
            // Load texture and audio assets
            ObjectCreation.AddTexturesToAssetMan("SwapCloset/", ["Textures", "Environment", "Room", "SwapCloset"]);
            ObjectCreation.AddTexturesToAssetMan("GottaBullyTex/", ["Textures", "Npc", "GottaBully"]);
            ObjectCreation.AddAudioToAssetMan("GottaBullyAud/", ["Audio", "Npc", "GottaBully"]);
            
            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "GottaBully.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            // Tapliasmy Chalkboard
            ObjectCreation.CreatePoster(AssetLoader.TextureFromMod(BasePlugin, "Textures", "Environment", "Poster", "SubToTapliasmy"), "Sub2Tapliasmy");
            CreateSwapClosetBlueprint();
            LoadGottaBully();
        }

        /*[CaudexLoadEventMod(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Pre)]
        private void GottaBullyAnimationsCompat()
        {
            GameObject.DestroyImmediate(ObjMan.Get<GottaBully>("Npc/GottaBully").GetComponent<GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(ObjMan.Get<GottaBully>("Npc/GottaBully").GetComponent<GottaSweepComponent>());
        }*/

        private void LoadGottaBully()
        {
            GottaBully gottaBully = GameObject.Instantiate((GottaSweep)NPCMetaStorage.Instance.Get(Character.Sweep).value, MTM101BaldiDevAPI.prefabTransform).SwapComponentSimple<GottaSweep, GottaBully>();
            gottaBully.name = "GottaBully";

            gottaBully.character = EnumExtensions.ExtendEnum<Character>("RecChars_GottaBully");
            PineDebugNpcIcons.AddIcon([gottaBully], "BorderGottaBully.png");

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

            gottaBully.spriteRenderer[0].sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>(RecommendedCharsPlugin.PartyMode ? "GottaBullyTex/GottaBully_Party" : "GottaBullyTex/GottaBully"), 26f);
            gottaBully.audMan.subtitleColor = new(198/255f, 136/255f, 91/255f);
            CharacterRadarColorPatch.colors.Add(gottaBully.character, gottaBully.audMan.subtitleColor);

            gottaBully.audIntro = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GottaBullyAud/GB_Intro"), "Vfx_RecChars_GBully_Intro", SoundType.Voice, gottaBully.audMan.subtitleColor);
            gottaBully.audSweep = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GottaBullyAud/GB_GottaBully"), "Vfx_RecChars_GBully_Sweep", SoundType.Voice, gottaBully.audMan.subtitleColor);

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_gottabully", gottaBully);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_gbully", gottaBully.Poster);

            gottaBully.potentialRoomAssets = ObjectCreation.RoomAssetsFromDirectory(ObjMan.Get<CaudexRoomBlueprint>("Room/SwapCloset"), "SwapCloset");

            ObjMan.Add("Npc/GottaBully", gottaBully);
            NPCMetadata gottaBullyMeta = new(Plugin, [gottaBully], gottaBully.name, NPCMetaStorage.Instance.Get(Character.Sweep).flags | NPCFlags.MakeNoise, ["adv_first_prize_immunity", "adv_exclusion_hammer_weakness"]);
            NPCMetaStorage.Instance.Add(gottaBullyMeta);
        }

        private void CreateSwapClosetBlueprint()
        {
            CaudexRoomBlueprint bullyRoom = new(Plugin, "SwappedCloset", "RecChars_SwappedCloset");

            bullyRoom.texFloor = AssetMan.Get<Texture2D>("SwapCloset/SwappedFloor");
            bullyRoom.texWall = AssetMan.Get<Texture2D>("SwapCloset/SwappedWall");
            bullyRoom.texCeil = AssetFinder.FindOfTypeWithName<Texture2D>("Carpet", true);
            bullyRoom.keepTextures = true;

            bullyRoom.doorMats = ObjectCreators.CreateDoorDataObject("SwapDoor", AssetMan.Get<Texture2D>("SwapCloset/SwappedDoor_Open"), AssetMan.Get<Texture2D>("SwapCloset/SwappedDoor_Closed"));

            bullyRoom.lightObj = ((DrReflex)NPCMetaStorage.Instance.Get(Character.DrReflex).value).potentialRoomAssets[0].selection.lightPre;
            bullyRoom.color = new(198/255f, 136/255f, 91/255f);

            List<WeightedPosterObject> bullyRoomPoster =
            [ObjMan.Get<PosterObject>("Pst/Sub2Tapliasmy").Weighted(100)];
            bullyRoom.posterChance = 0.25f;
            bullyRoom.posters = bullyRoomPoster;

            LevelLoaderCompatHelper.AddRoom(bullyRoom, "recchars_swapcloset");
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_swapflor", bullyRoom.texFloor);
            LevelLoaderPlugin.Instance.roomTextureAliases.Add("recchars_swapwall", bullyRoom.texWall);
            ObjMan.Add("Room/SwapCloset", bullyRoom);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true)
            {
                scene.MarkAsNeverUnload();

                if (RecommendedCharsConfig.guaranteeSpawnChar)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<GottaBully>("Npc/GottaBully"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-1, 0);
                }
                else
                    scene.potentialNPCs.CopyNpcWeight(Character.LookAt, ObjMan.Get<GottaBully>("Npc/GottaBully"));
                return;
            }

            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();

                if (!RecommendedCharsConfig.guaranteeSpawnChar)
                {
                    if (id > 1)
                        scene.potentialNPCs.CopyNpcWeight(Character.LookAt, ObjMan.Get<GottaBully>("Npc/GottaBully"));
                }
                else if (id == 2)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<GottaBully>("Npc/GottaBully"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-1, 0);
                }
            }
        }
    }
}
