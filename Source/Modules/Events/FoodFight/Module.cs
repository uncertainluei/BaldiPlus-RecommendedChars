using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;

using PlusStudioLevelLoader;
using PlusLevelStudio;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Food Fight"), CaudexModuleSaveTag("Mdl_FoodFight")]
    [CaudexModuleConfig("Modules.Events", "FoodFight",
        "\"Foodfight? More like Gayfight! This movie is the worst movie on the planet!\" ~ Michael the GoAnimate Guy, 2015", true)]
    public sealed partial class Module_Event_FoodFight : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load sprite and audio assets
            AssetMan.Add("EvtAud/FoodFightAnnouncement", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Event", "Event_FoodFight.wav"), "Vfx_Baldi_Event_RecChars_FoodFight", SoundType.Voice, Color.green));

            // Load localization
            //CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Event", "FoodFight.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadEvent()
        {
            FoodFightEvent foodFightEvent = new RandomEventBuilder<FoodFightEvent>(Plugin)
            .SetName("Event_FoodFight")
            .SetEnum("RecChars_FoodFight")
            .SetSound(AssetMan.Get<SoundObject>("EvtAud/FoodFightAnnouncement"))
            .SetMinMaxTime(40f, 80f)
            .SetMeta(RandomEventFlags.None)
            .Build();

            LevelLoaderPlugin.Instance.randomEventAliases.Add("recchars_foodfight", foodFightEvent);
            ObjMan.Add("Evt_FoodFight", foodFightEvent);
        }

        //[CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Events/FoodFight", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Events/FoodFight", GenerationStageFlags.Addend);

            lvl.randomEvents.Add(ObjMan.Get<RandomEvent>("Evt_FoodFight").Weighted(100));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Event_FoodFight", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Event", "FoodFight.png"));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.eventSprites.Add("recchars_foodfight", AssetMan.Get<Sprite>("EditorSpr/Event_FoodFight"));
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompiant) => mode.availableRandomEvents.Add("recchars_foodfight"));
        }
    }
}
