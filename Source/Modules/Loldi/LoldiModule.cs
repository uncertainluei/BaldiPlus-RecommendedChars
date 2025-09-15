using BaldisBasicsPlusAdvanced.API;

using BepInEx.Configuration;
using BepInEx.Bootstrap;

using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;
using UncertainLuei.CaudexLib.Objects;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelLoader;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.FragileWindows;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;

using APIConnector;
using UncertainLuei.CaudexLib.Registers;
using PlusStudioLevelLoader;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("LOLdi Exchanges"), CaudexModuleSaveTag("Mdl_Loldi")]
    [CaudexModuleConfig("Modules", "Loldi",
        "Adds \"Blue, Guy\" and Gifter from LOLdi's Basics.", true)]
    public sealed class Module_Loldi : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Npc", "BlueGuy"), x => "BluTex/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Npc", "Gifter"), x => "GifterTex/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(BasePlugin), "Audio", "BlueGuy"), "BluAud/");
            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(BasePlugin), "Audio", "Gifter"), "GifterAud/");

            AssetMan.Add("Sfx/GiftUnwrap", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "GiftUnwrap.wav"), "", SoundType.Effect, Color.white, 0f));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Loldi.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
        }

        /*private void LoadMrDaycare()
        {
            MrDaycare daycare = new NPCBuilder<MrDaycare>(Plugin)
                .SetName("MrDaycare")
                .SetEnum("RecChars_MrDaycare")
                .SetPoster(AssetMan.Get<Texture2D>("DaycareTex/pri_daycare"), "PST_PRI_RecChars_Daycare1", "PST_PRI_RecChars_Daycare2")
                .AddMetaFlag(NPCFlags.Standard | NPCFlags.MakeNoise)
                .SetMetaTags(["faculty", "no_balloon_frenzy"])
                .AddPotentialRoomAssets(CreateDaycareRooms())
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
            daycare.audComing = Resources.FindObjectsOfTypeAll<SoundObject>().First(x => x.name == "Silence" && x.GetInstanceID() >= 0);
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

            MrDaycare unnerfedDaycare = GameObject.Instantiate(daycare, MTM101BaldiDevAPI.prefabTransform);
            unnerfedDaycare.name = "MrDaycare Unnerfed";
            ObjMan.Add("Npc_MrDaycare_Unnerfed", unnerfedDaycare);

            daycare.Navigator.speed = 30f;
            daycare.Navigator.maxSpeed = 30f;
            daycare.maxTimeoutLevel = 1;
            daycare.ruleSensitivityMul = 1;
            ObjMan.Add("Npc_MrDaycare_Nerfed", daycare);

            PineDebugNpcIconPatch.icons.Add(daycare.Character, AssetMan.Get<Texture2D>("DaycareTex/BorderDaycare"));
            CharacterRadarColorPatch.colors.Add(daycare.Character, daycare.audMan.subtitleColor);

            ObjMan.Add("Npc_MrDaycare", RecommendedCharsConfig.nerfMrDaycare.Value ? daycare : unnerfedDaycare);
        }*/

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelLoaderGuid, LoadingEventOrder.Pre)]
        private void RegisterToLevelLoader()
        {
            // LevelLoaderPlugin.Instance.npcAliases.Add("recchars_blueguy", ObjMan.Get<BlueGuy>("Npc_BlueGuy"));
            // LevelLoaderPlugin.Instance.npcAliases.Add("recchars_gifter", ObjMan.Get<Gifter>("Npc_Gifter_Thrower"));
            // LevelLoaderPlugin.Instance.npcAliases.Add("recchars_gifttanynt", ObjMan.Get<Gifter>("Npc_Gifter_Gifttanny"));

            // LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_blueguy", ObjMan.Get<MrDaycare>("Npc_MrDaycare").Poster);
            // LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_gifter", ObjMan.Get<Gifter>("Npc_Gifter_Thrower").Poster);
        }

/*
        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Pre)]
        private void AdvancedCompat()
        {
            BepInEx.PluginInfo advInfo = Chainloader.PluginInfos[RecommendedCharsPlugin.AdvancedGuid];

            // Add new words and tips
            ApiManager.AddNewSymbolMachineWords(Plugin, "Moldy", "Dave", "house");
            ApiManager.AddNewTips(Plugin, "Adv_Elv_Tip_RecChars_Pie", "Adv_Elv_Tip_RecChars_DoorKey",
                "Adv_Elv_Tip_RecChars_MrDaycareExceptions", "Adv_Elv_Tip_RecChars_MrDaycareEarly");
        }*/

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            // if (title == "END")
            // {
            //     scene.MarkAsNeverUnload();
            //     scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("PieItem").Weighted(50));
            //     scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("DoorKeyItem").Weighted(25));

            //     if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
            //     {
            //         scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<MrDaycare>("Npc_MrDaycare"));
            //         scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            //     }
            //     else
            //         scene.potentialNPCs.CopyNpcWeight(Character.Beans, ObjMan.Get<MrDaycare>("Npc_MrDaycare"));
            //     return;
            // }

            // if (title.StartsWith("F"))
            // {
            //     scene.MarkAsNeverUnload();
            //     scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("PieItem").Weighted(50));
            //     if (id > 0)
            //         scene.shopItems = scene.shopItems.AddToArray(AssetMan.Get<ItemObject>("DoorKeyItem").Weighted(25));

            //     if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
            //     {
            //         scene.potentialNPCs.CopyNpcWeight(Character.Beans, ObjMan.Get<MrDaycare>("Npc_MrDaycare"));
            //     }
            //     else if (id == 0)
            //     {
            //         scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<MrDaycare>("Npc_MrDaycare"));
            //         scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            //     }
            // }
        }
    }
}
