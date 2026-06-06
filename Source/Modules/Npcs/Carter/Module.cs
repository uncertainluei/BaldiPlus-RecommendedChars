using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;

using PlusStudioLevelLoader;

using TMPro;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;
using UnityEngine.UI;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Carter"), CaudexModuleSaveTag("Mdl_Carter")]
    [CaudexModuleConfig("Modules", "Carter",
        "Adds Carter, an Arts and Crafters OC originating from a scratch project shared in 2022.", true)]
    public sealed class Module_Carter : RecCharsModule
    {
        internal override byte IconId => 9;

        protected override void Initialized()
        {
            // Load texture and audio assets
            ObjectCreation.AddTexturesToAssetMan("CarterTex/", ["Textures", "Npc", "Carter"]);
            ObjectCreation.AddAudioToAssetMan("CarterAud/", ["Audio", "Npc", "Carter"]);

            AssetMan.Add("CarterPst/ClassicCarterMissing", AssetLoader.TextureFromMod(BasePlugin, "Textures", "Environment", "Poster", "ClassicCarterMissing.png"));

            AssetMan.Add("Sfx/MapZoom", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "MapPage_ZoomIn.ogg"), "", SoundType.Effect, Color.white, 0));
            AssetMan.Add("Sfx/MapWhoosh", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "MapPage_Whoosh.ogg"), "", SoundType.Effect, Color.white, 0));
            AssetMan.Add("Sfx/MapThump", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "MapPage_Thump.ogg"), "", SoundType.Effect, Color.white, 0));
            AssetMan.Add("Sfx/MapTurn", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "MapPage_Turn.ogg"), "", SoundType.Effect, Color.white, 0));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "Carter.json5");

            // Load patches
            Hooks.PatchAll(typeof(CarterPatches));
        }

        private Items _carterItmEnum;

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            // Create poster
            ObjectCreation.CreatePoster("CarterPst/ClassicCarterMissing", "ClassicCarterMissing", new PosterTextData() {
                color = Color.black,
                textKey = "PST_RecChars_ClassicCarterMissing",
                font = BaldiFonts.BoldComicSans24.FontAsset(),
                fontSize = 24,
                alignment = TextAlignmentOptions.Center,
                position = new(48,48),
                size = new(160,32)
            });

            // Carter's items
            _carterItmEnum = EnumExtensions.ExtendEnum<Items>("RecChars_CarterItem");
            CarterItemObject[] carterItems = [
                CreateCarterItem(Items.Bsoda),
                CreateCarterItem(Items.Scissors),
                CreateCarterItem(Items.Tape, false),
                CreateCarterItem(Items.PrincipalWhistle, false),
                CreateCarterItem(Items.ZestyBar),
                CreateCarterItem(Items.DoorLock),
                CreateCarterItem(Items.Wd40)
            ];

            // Carter's paper
            GameObject paperObject = new("CarterPaper", typeof(Image));
            Image paperImage = paperObject.GetComponent<Image>();
            paperImage.sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("CarterTex/carterpaper"), 100);
            paperImage.rectTransform.sizeDelta = new(128,128);
            paperObject.SetActive(false);
            CarterPaper paper = paperObject.AddComponent<CarterPaper>();
            paper.audZoom = AssetMan.Get<SoundObject>("Sfx/MapZoom");
            paper.audWhoosh = AssetMan.Get<SoundObject>("Sfx/MapWhoosh");
            paper.audThump = AssetMan.Get<SoundObject>("Sfx/MapThump");
            paperObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform, false);
            GameObject paperText = new("PaperText", typeof(TextMeshProUGUI));
            paper.text = paperText.GetComponent<TMP_Text>();
            paperText.transform.SetParent(paperObject.transform, false);
            paper.text.font = BaldiFonts.ComicSans12.FontAsset();
            paper.text.fontSize = 12;
            paper.text.alignment = TextAlignmentOptions.Center;
            paper.text.lineSpacing = -40;
            paper.text.color = Color.black;
            paper.text.rectTransform.anchorMin = Vector2.up; 
            paper.text.rectTransform.anchorMax = Vector2.up; 
            paper.text.rectTransform.pivot = Vector2.up; 
            paper.text.rectTransform.anchoredPosition = new(16, -44);
            paper.text.rectTransform.sizeDelta = new(96, 50);
            paper.gameObject.SetActive(true);
            ObjMan.Add("Obj_CarterPaper", paper);

            // Carter himself
            Carter carter = new NPCBuilder<Carter>(Plugin)
                .SetName("Carter")
                .SetEnum("RecChars_Carter")
                .SetPoster(AssetMan.Get<Texture2D>("CarterTex/pri_cartre"), "PST_PRI_RecChars_Carter1", "PST_PRI_RecChars_Carter2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(["student"])
                .AddLooker()
                .AddTrigger()
                .SetWanderEnterRooms()
                .Build();


            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(3, 1, 28f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("CarterTex/cartre_sheet"));

            carter.sprite = carter.spriteRenderer[0];
            carter.sprite.transform.localPosition = Vector3.zero;

            carter.sprite.sprite = sprites[0];
            carter.sprNormal = sprites[0];
            carter.sprAngry = sprites[1];
            carter.sprScreech = sprites[2];

            carter.possibleItems = carterItems;

            carter.audMan = carter.GetComponent<AudioManager>();
            carter.audMan.subtitleColor = new(19/255f, 158/255f, 140/255f);

            PineDebugNpcIcons.AddIcon([carter], "BorderCarter.png");
            CharacterRadarColorPatch.colors.Add(carter.character, carter.audMan.subtitleColor);

            carter.audLost = [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Lost1"), "Vfx_RecChars_Carter_Lost1", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Lost2"), "Vfx_RecChars_Carter_Lost2", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Lost3"), "Vfx_RecChars_Carter_Lost3", SoundType.Voice, carter.audMan.subtitleColor),
            ];
            carter.audItms = [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Item_Bsoda"), "Vfx_RecChars_Carter_Lost_Bsoda", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Item_Scissors"), "Vfx_RecChars_Carter_Lost_Scissors", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Item_Tape"), "Vfx_RecChars_Carter_Lost_Tape", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Item_Whistle"), "Vfx_RecChars_Carter_Lost_PrincipalWhistle", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Item_ZestyBar"), "Vfx_RecChars_Carter_Lost_ZestyBar", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Item_DoorLock"), "Vfx_RecChars_Carter_Lost_DoorLock", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Item_NoSquee"), "Vfx_RecChars_Carter_Lost_Wd40", SoundType.Voice, carter.audMan.subtitleColor),
            ];
            carter.audHelp = [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Help1"), "Vfx_RecChars_Carter_Help1", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Help2"), "Vfx_RecChars_Carter_Help2", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Help3"), "Vfx_RecChars_Carter_Help3", SoundType.Voice, carter.audMan.subtitleColor),
            ];
            carter.audLeave = [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Leave1"), "Vfx_RecChars_Carter_Leave1", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Leave2"), "Vfx_RecChars_Carter_Leave2", SoundType.Voice, carter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Leave3"), "Vfx_RecChars_Carter_Leave3", SoundType.Voice, carter.audMan.subtitleColor),
            ];

            carter.audCoords = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_MapCoords"), "Vfx_RecChars_Carter_MapCoords", SoundType.Voice, carter.audMan.subtitleColor);
            carter.audThanks = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_Thanks"), "Vfx_RecChars_Carter_Thanks", SoundType.Voice, carter.audMan.subtitleColor);
            carter.audThanks.additionalKeys = [new() {key = "Vfx_RecChars_Carter_Thanks_1", time = 2.641f}];

            carter.audIntro = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_ScreamIntro"), "Vfx_RecChars_Carter_ScreamIntro", SoundType.Effect, carter.audMan.subtitleColor);
            carter.audLoop = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CarterAud/Ctr_ScreamLoop"), "Vfx_RecChars_Carter_ScreamLoop", SoundType.Effect, carter.audMan.subtitleColor);

            carter.Navigator.speed = 30;
            carter.Navigator.maxSpeed = 30;

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_carter", carter);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_carter", carter.Poster);
            SurpriseNpc.possibleVisuals.Add(new SurpriseNpcVisualSprite(carter));
            ObjMan.Add("Npc/Carter", carter);
        }

        private ItemMetaData _carterItmMeta;
        private CarterItemObject CreateCarterItem(Items baseItm, bool suffix = true)
        {
            ItemBuilder builder = new ItemBuilder(Plugin)
                .SetNameAndDescription("Itm_RecChars_Carter"+baseItm.ToStringExtended(), "")
                .SetEnum(_carterItmEnum);
                
            if (_carterItmMeta == null)
                builder.SetMeta(ItemFlags.None, ["recchars:gifter_blacklist", "adv_forbidden_present", "presents_nopresent"]);
            else
                builder.SetMeta(_carterItmMeta);

            CarterItemObject itm = builder.Build<CarterItemObject>();
            _carterItmMeta = itm.GetMeta();
            itm.Setup(baseItm, suffix);
            return itm;
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true)
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(ObjMan.Get<Carter>("Npc/Carter"), scene, 90, true);
                return;
            }
            if (title.StartsWith("F") && id > 1)
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(ObjMan.Get<Carter>("Npc/Carter"), scene, id > 1 ? 100 : 45, false, 2);
            }
        }

        private void AddToNpcs(NPC npc, SceneObject scene, int weight, bool endless, int firstNo = 0)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar)
                scene.potentialNPCs.Add(npc.Weighted(weight));
            else if (endless || scene.levelNo == firstNo)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(npc);
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-1, 0);
            }
        }
    }
}