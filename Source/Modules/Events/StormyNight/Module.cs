using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

using PlusStudioLevelLoader;
using PlusLevelStudio;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Stormy Night"), CaudexModuleSaveTag("Mdl_StormyNight")]
    [CaudexModuleConfig("Modules.Events", "StormyNight",
        "An event where the Super Schoolhouse goes dark and only gets illuminated by occasional thunder.", true)]
    public sealed partial class Module_Event_StormyNight : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load audio assets
            AddAudioToAssetMan("EvtAud/StormyNight/", ["Audio", "Event", "StormyNight"]);
            AssetMan.Add("EvtAud/StormyNightAnnouncement", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Event", "Event_StormyNight.wav"), "Vfx_Baldi_Event_StormyNight", SoundType.Voice, Color.green));
            AssetMan.Add("Skybox/NightStandard", AssetLoader.CubemapFromMod(BasePlugin, "Textures", "Environment", "Skybox", "Cubemap_NightStandard.png"));

            // Load localization
            //CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "LunchBox.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadEvent()
        {
            StormyNightEvent stormEvent = new RandomEventBuilder<StormyNightEvent>(Plugin)
            .SetName("Event_StormyNight")
            .SetEnum("RecChars_StormyNight")
            .SetSound(AssetMan.Get<SoundObject>("EvtAud/StormyNightAnnouncement"))
            .SetMinMaxTime(40f, 80f)
            .SetMeta(RandomEventFlags.None)
            .Build();

            stormEvent.audMan = stormEvent.gameObject.AddComponent<AudioManager>();
            stormEvent.audMan.audioDevice = stormEvent.gameObject.AddComponent<AudioSource>();
            stormEvent.audMan.loop = true;
            stormEvent.audMan.loopOnStart = true;
            stormEvent.audMan.maintainLoop = true;

            stormEvent.audRain = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("EvtAud/StormyNight/StormLoop"), "Sfx_RecChars_StormyNight_RainLoop", SoundType.Effect, Color.white, 0);
            stormEvent.audThunder =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("EvtAud/StormyNight/Thunderclap1"), "Sfx_RecChars_StormyNight_Thunder", SoundType.Effect, Color.white, 0),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("EvtAud/StormyNight/Thunderclap2"), "Sfx_RecChars_StormyNight_Thunder", SoundType.Effect, Color.white, 0),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("EvtAud/StormyNight/Thunderclap3"), "Sfx_RecChars_StormyNight_Thunder", SoundType.Effect, Color.white, 0),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("EvtAud/StormyNight/Thunderclap4"), "Sfx_RecChars_StormyNight_Thunder", SoundType.Effect, Color.white, 0)
            ];

            stormEvent.nightSkybox = AssetMan.Get<Cubemap>("Skybox/NightStandard");

            LevelLoaderPlugin.Instance.randomEventAliases.Add("recchars_stormynight", stormEvent);
            ObjMan.Add("Evt_StormyNight", stormEvent);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Events/StormyNight", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Events/StormyNight", GenerationStageFlags.Addend);

            lvl.randomEvents.Add(ObjMan.Get<RandomEvent>("Evt_StormyNight").Weighted(100));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Event_StormyNight", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Event", "StormyNight.png"));

            // Load localization
            //CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "LunchBox.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.eventSprites.Add("recchars_stormynight", AssetMan.Get<Sprite>("EditorSpr/Event_StormyNight"));
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompiant) => mode.availableRandomEvents.Add("recchars_stormynight"));
        }
    }
}
