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
using System.Linq;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Crawlspace"), CaudexModuleSaveTag("Mdl_Crawlspace")]
    [CaudexModuleConfig("Modules.Events", "Crawlspace",
        "falling deep underground", true)]
    public sealed partial class Module_Event_Crawlspace : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load sprite and audio assets
            AssetMan.Add("EvtAud/CrawlspaceAnnouncement", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Event", "Event_Crawlspace.wav"), "Vfx_Baldi_Event_RecChars_Crawlspace", SoundType.Voice, Color.green));

            // Load localization
            //CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Event", "Crawlspace.json5");

            Hooks.PatchAll(typeof(CrawlspaceHeightPatch));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadEvent()
        {
            CrawlspaceEvent crawlspaceEvent = new RandomEventBuilder<CrawlspaceEvent>(Plugin)
            .SetName("Event_Crawlspace")
            .SetEnum("RecChars_Crawlspace")
            .SetSound(AssetMan.Get<SoundObject>("EvtAud/CrawlspaceAnnouncement"))
            .SetMinMaxTime(40f, 80f)
            .SetMeta(RandomEventFlags.AffectsGenerator)
            .Build();

            crawlspaceEvent.ecPrefab = Resources.FindObjectsOfTypeAll<EnvironmentController>().First(x => x.GetInstanceID() >= 0);

            crawlspaceEvent.roomPrefab = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<RoomController>().First(x => x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform);
            crawlspaceEvent.roomPrefab.name = "CrawlspaceRoom";
            crawlspaceEvent.roomPrefab.type = RoomType.Null;
            crawlspaceEvent.roomPrefab.category = RoomCategory.Null;
            crawlspaceEvent.roomPrefab.expandable = true;
            crawlspaceEvent.roomPrefab.acceptsExits = false;
            crawlspaceEvent.roomPrefab.acceptsPosters = false;
            crawlspaceEvent.roomPrefab.florTex = crawlspaceEvent.roomPrefab.wallTex = crawlspaceEvent.roomPrefab.ceilTex
                = Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "ColoredBrickWall" && x.GetInstanceID() >= 0);

            crawlspaceEvent.transparent = Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Transparent" && x.GetInstanceID() >= 0);

            LevelLoaderPlugin.Instance.randomEventAliases.Add("recchars_crawlspace", crawlspaceEvent);
            ObjMan.Add("Evt_Crawlspace", crawlspaceEvent);
        }

        //[CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Events/Crawlspace", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Events/Crawlspace", GenerationStageFlags.Addend);

            lvl.randomEvents.Add(ObjMan.Get<RandomEvent>("Evt_Crawlspace").Weighted(100));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Event_Crawlspace", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Event", "Crawlspace.png"));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            LevelStudioPlugin.Instance.eventSprites.Add("recchars_crawlspace", AssetMan.Get<Sprite>("EditorSpr/Event_Crawlspace"));
            EditorInterfaceModes.AddModeCallback((mode, vanillaCompiant) => mode.availableRandomEvents.Add("recchars_crawlspace"));
        }
    }
}
