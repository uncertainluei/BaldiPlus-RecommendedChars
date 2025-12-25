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
using System.Reflection;
using HarmonyLib;
using brobowindowsmod;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Crawlspace"), CaudexModuleSaveTag("Mdl_Crawlspace")]
    [CaudexModuleConfig("Modules.Events", "Crawlspace",
        "the backrooms", true)]
    public sealed partial class Module_Event_Crawlspace : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            AddTexturesToAssetMan("CrawlspaceTex/", ["Textures", "Environment", "Room", "Crawlspace"]);

            AssetMan.Add("EvtAud/CrawlspaceAnnouncement", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Event", "Event_Crawlspace.wav"), "Vfx_Baldi_Event_RecChars_Crawlspace", SoundType.Voice, Color.green));

            // Load localization
            //CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Event", "Crawlspace.json5");

            // Load patches
            Hooks.PatchAll(typeof(CrawlspaceEntityPatches));
            Hooks.Patch(typeof(Looker).GetRuntimeMethods().First(x => x.GetParameters().Length == 5), new HarmonyMethod(AccessTools.Method(typeof(CrawlspaceEntityPatches), "LookerRaycast")));
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
            crawlspaceEvent.lvlLoaderPre = Resources.FindObjectsOfTypeAll<LevelLoader>().First(x => x.GetInstanceID() >= 0);

            crawlspaceEvent.lvlData = crawlspaceEvent.gameObject.AddComponent<LevelDataContainer>();
            crawlspaceEvent.lvlData.rooms = [new() {
                name = "Crawlspace",
                type = RoomType.Room,
                category = RoomCategory.Null,
                wallTex = AssetMan.Get<Texture2D>("CrawlspaceTex/CrawlspaceWall"),
                florTex = AssetMan.Get<Texture2D>("CrawlspaceTex/CrawlspaceFloor"),
                ceilTex = AssetMan.Get<Texture2D>("CrawlspaceTex/CrawlspaceCeiling")
            }];

            crawlspaceEvent.tilePrefab = GameObject.Instantiate(crawlspaceEvent.ecPrefab.TilePre, MTM101BaldiDevAPI.prefabTransform).GetComponent<MeshRenderer>();
            GameObject.DestroyImmediate(crawlspaceEvent.tilePrefab.GetComponent<Tile>());
            GameObject.DestroyImmediate(crawlspaceEvent.tilePrefab.transform.GetChild(0).gameObject);
            GameObject.DestroyImmediate(crawlspaceEvent.tilePrefab.transform.GetChild(0).gameObject);
            GameObject.DestroyImmediate(crawlspaceEvent.tilePrefab.transform.GetChild(0).gameObject);
            GameObject.DestroyImmediate(crawlspaceEvent.tilePrefab.transform.GetChild(0).gameObject);
            crawlspaceEvent.tilePrefab.name = "Tile_Stripped";

            crawlspaceEvent.darkLightmap = new(1,1);
            crawlspaceEvent.darkLightmap.name = "Dark_Lightmap";
            crawlspaceEvent.darkLightmap.SetPixel(0,0,Color.gray);
            crawlspaceEvent.darkLightmap.Apply();

            crawlspaceEvent.transparentTexture = Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.name == "Transparent" && x.isReadable && x.GetInstanceID() >= 0);

            LevelLoaderPlugin.Instance.randomEventAliases.Add("recchars_crawlspace", crawlspaceEvent);
            ObjMan.Add("Evt_Crawlspace", crawlspaceEvent);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int num, CustomLevelObject lvl)
        {
            if (title == "END" || lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/Events/Crawlspace", GenerationStageFlags.Addend))
                return;

            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/Events/Crawlspace", GenerationStageFlags.Addend);

            // Exclude F1-F2 / Small/Medium Schoolhouse (unless you get Maintenance with Level Typed)
            if (num < 2 && lvl.type != LevelType.Maintenance) return;
            // Exclude Fragile Windows Window World level type (because of THOSE DREADED FLOOR WINDOWS)
            if (lvl.type.ToStringExtended() == "WindowWorld") return;

            lvl.randomEvents.Add(ObjMan.Get<RandomEvent>("Evt_Crawlspace").Weighted(lvl.type == LevelType.Maintenance ? 150 : 80));
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
