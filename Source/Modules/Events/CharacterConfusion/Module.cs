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
    [CaudexModule("Character Confusion"), CaudexModuleSaveTag("Mdl_CharacterConfusion")]
    [CaudexModuleConfig("Modules.Events", "CharacterConfusion",
        "An event where all characters' visuals get swapped with each other's.", true)]
    public sealed partial class Module_Event_CharacterConfusion : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load audio assets
            AssetMan.Add("EvtAud/CharConfusionAnnouncement", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Event", "Event_CharacterConfusion.wav"), "Vfx_Baldi_Event_RecChars_CharacterConfusion", SoundType.Voice, Color.green));

            // Load localization
            //CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Event", "CharacterConfusion.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadEvent()
        {
            CharacterConfusionEvent stormEvent = new RandomEventBuilder<CharacterConfusionEvent>(Plugin)
            .SetName("Event_CharacterConfusion")
            .SetEnum("RecChars_CharacterConfusion")
            .SetSound(AssetMan.Get<SoundObject>("EvtAud/CharConfusionAnnouncement"))
            .SetMinMaxTime(40f, 80f)
            .SetMeta(RandomEventFlags.None)
            .Build();

            NPCMetaStorage.Instance.Get(Character.Chalkles).tags.Add("recchars:character_confusion_blacklist");

            LevelLoaderPlugin.Instance.randomEventAliases.Add("recchars_characterconfusion", stormEvent);
            ObjMan.Add("Evt_CharacterConfusion", stormEvent);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Events/CharacterConfusion", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Events/CharacterConfusion", GenerationStageFlags.Addend);

            lvl.randomEvents.Add(ObjMan.Get<RandomEvent>("Evt_CharacterConfusion").Weighted(100));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Event_CharacterConfusion", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Event", "CharacterConfusion.png"));

            // Load localization
            //CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "LunchBox.json5");
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.eventSprites.Add("recchars_characterconfusion", AssetMan.Get<Sprite>("EditorSpr/Event_CharacterConfusion"));
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompiant) => mode.availableRandomEvents.Add("recchars_characterconfusion"));
        }
    }
}
