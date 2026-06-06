using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusStudioLevelLoader;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Gifter"), CaudexModuleSaveTag("Mdl_Gifter")]
    [CaudexModuleConfig("Modules", "Gifter",
        "Adds Gifter from LOLdi's Basics Public Alpha.", true)]
    public sealed class Module_Gifter : RecCharsModule
    {
        internal override byte IconId => 5;

        protected override void Initialized()
        {
            // Load texture and audio assets
            ObjectCreation.AddTexturesToAssetManWLegacy("GifterTex/", ["Textures", "Npc", "Gifter"]);
            ObjectCreation.AddAudioToAssetMan("GifterAud/", ["Audio", "Npc", "Gifter"]);

            AssetMan.Add("Sfx/GiftUnwrap", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "GiftUnwrap.wav"), "Sfx_RecChars_GiftUnwrap", SoundType.Effect, Color.white));
            AssetMan.Add("Sfx/BombFuse", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "BombFuse.wav"), "Sfx_RecChars_BombFuse", SoundType.Effect, Color.white));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "Gifter.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadGifter()
        {
            Gifter gifter = new NPCBuilder<Gifter>(Plugin)
                .SetName("Gifter")
                .SetEnum("RecChars_Gifter")
                .SetPoster(AssetMan.Get<Texture2D>("GifterTex/pri_gifter"), "PST_PRI_RecChars_Gifter1", "PST_PRI_RecChars_Gifter2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(["student"])
                .AddLooker()
                .AddTrigger()
                .SetWanderEnterRooms()
                .Build();

            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 2, 42f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>(RecommendedCharsPlugin.PartyMode ? "GifterTex/Gifter_Party" : "GifterTex/Gifter_Sheet"));

            gifter.sprite = gifter.spriteRenderer[0];
            gifter.sprite.transform.localPosition = Vector3.up * -1.32f;

            gifter.sprite.sprite = gifter.sprGift = sprites[1];
            gifter.sprNoGift = sprites[0];
            gifter.sprThrow = sprites[2];
            gifter.sprSad = sprites[3];

            gifter.audMan = gifter.GetComponent<AudioManager>();
            gifter.audMan.subtitleColor = new(80/255f, 154/255f, 205/255f);

            CharacterRadarColorPatch.colors.Add(gifter.character, gifter.audMan.subtitleColor);

            LookAtGuy theTest = (LookAtGuy)NPCMetaStorage.Instance.Get(Character.LookAt).value;
            gifter.explosionPre = theTest.explosionPrefab;

            gifter.audHumming = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_Idle"), "Vfx_RecChars_Gifter_Idle", SoundType.Voice, gifter.audMan.subtitleColor);
            gifter.audShocked = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_Shocked"), "Vfx_RecChars_Gifter_Shocked", SoundType.Voice, gifter.audMan.subtitleColor);
            gifter.audSorry = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_Sorry"), "Vfx_RecChars_Gifter_Sorry", SoundType.Voice, gifter.audMan.subtitleColor);

            gifter.audGift =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_Gift1"), "Vfx_RecChars_Gifter_Gift1", SoundType.Voice, gifter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_Gift2"), "Vfx_RecChars_Gifter_Gift2", SoundType.Voice, gifter.audMan.subtitleColor)
            ];
            gifter.audOpened =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_GiftOpened1"), "Vfx_RecChars_Gifter_GiftOpened1", SoundType.Voice, gifter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_GiftOpened2"), "Vfx_RecChars_Gifter_GiftOpened2", SoundType.Voice, gifter.audMan.subtitleColor)
            ];
            gifter.audLeft =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_GiftLeft1"), "Vfx_RecChars_Gifter_GiftLeft1", SoundType.Voice, gifter.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("GifterAud/Gft_GiftLeft2"), "Vfx_RecChars_Gifter_GiftLeft2", SoundType.Voice, gifter.audMan.subtitleColor)
            ];
            gifter.audLeft[1].additionalKeys =
            [
                new()
                {
                    key = "Vfx_RecChars_Gifter_GiftLeft2_1",
                    time = 1.393f
                }
            ];

            gifter.Navigator.speed = 22;
            gifter.Navigator.maxSpeed = 22;

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_gifter", gifter);
            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_gifttanynt", gifter);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_gifter", gifter.Poster);

            PineDebugNpcIcons.AddIcon([gifter], "BorderGifter.png");
            SurpriseNpc.possibleVisuals.Add(new SurpriseNpcVisualSprite(gifter, gifter.audGift[1]));
            ObjMan.Add("Npc/Gifter", gifter);
        }

        [CaudexLoadEvent(LoadingEventOrder.Post)]
        private void SetGifterTags()
        {
            ItemMetaStorage.Instance.FindByEnum(Items.BusPass).tags.Add("recchars:gifter_blacklist");
            ItemMetaStorage.Instance.FindAllWithTags(false, ["lost_item", "shape_key", "shop_dummy"]).Do(x => { x.tags.Add("recchars:gifter_blacklist"); });
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true || title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(ObjMan.Get<Gifter>("Npc/Gifter"), scene, 125, title == "END");
            }
        }

        private void AddToNpcs(NPC npc, SceneObject scene, int weight, bool endless, int firstNo = 0)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar)
                scene.potentialNPCs.Add(npc.Weighted(weight));
            else if (endless || scene.levelNo == firstNo)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(npc);
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            }
        }
    }
}
