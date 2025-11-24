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
    //[CaudexModule("LOLdi Exchanges"), CaudexModuleSaveTag("Mdl_Loldi")]
    [CaudexModuleConfig("Modules", "Loldi",
        "Adds \"Blue, Guy\" and Gifter from LOLdi's Basics.", true)]
    public sealed class Module_Loldi : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            AddTexturesToAssetMan("BluTex/", ["Textures", "Npc", "BlueGuy"]);
            AddTexturesToAssetMan("GifterTex/", ["Textures", "Npc", "Gifter"]);

            AddAudioToAssetMan("BluAud/", ["Audio", "BlueGuy"]);
            AddAudioToAssetMan("GifterAud/", ["Audio", "Gifter"]);

            AssetMan.Add("StatusSpr/BlueGuyFog", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BluTex/BlueGuyFogIcon"), 1));
            ObjMan.Add<Fog>("Fog/BlueGuyFog", new() { color = Color.blue, maxDist = 15, startDist = 5, strength = 1, priority = 16});

            AssetMan.Add("Sfx/GiftUnwrap", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(BasePlugin, "Audio", "Sfx", "GiftUnwrap.wav"), "Sfx_RecChars_GiftUnwrap", SoundType.Effect, Color.white));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Loldi.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            LoadBlueGuy();
            LoadGifter();
        }

        private void LoadBlueGuy()
        {
            BlueGuy bluGuy = new NPCBuilder<BlueGuy>(Plugin)
                .SetName("BlueGuy")
                .SetEnum("RecChars_BlueGuy")
                .SetPoster(AssetMan.Get<Texture2D>("BluTex/pri_blue"), "PST_PRI_RecChars_BlueGuy1", "PST_PRI_RecChars_BlueGuy2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(["student"])
                .AddLooker()
                .AddTrigger()
                .SetWanderEnterRooms()
                .Build();


            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 50f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("BluTex/BlueGuySprites"));

            bluGuy.sprite = bluGuy.spriteRenderer[0];
            bluGuy.sprite.transform.localPosition = Vector3.zero;

            bluGuy.sprite.sprite = sprites[0];
            bluGuy.sprNormal = sprites[0];
            bluGuy.sprAngry = sprites[1];

            bluGuy.audMan = bluGuy.GetComponent<AudioManager>();
            bluGuy.audMan.subtitleColor = new(36/255f, 72/255f, 145/255f);

            PineDebugNpcIconPatch.icons.Add(bluGuy.character, AssetMan.Get<Texture2D>("BluTex/BorderBlueGuy"));
            CharacterRadarColorPatch.colors.Add(bluGuy.character, bluGuy.audMan.subtitleColor);

            bluGuy.audIntro = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BluAud/Blu_Intro"), "Vfx_RecChars_BlueGuy_Intro", SoundType.Effect, bluGuy.audMan.subtitleColor);
            bluGuy.audLoop = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BluAud/Blu_Loop"), "Vfx_RecChars_BlueGuy_Loop", SoundType.Effect, bluGuy.audMan.subtitleColor);

            bluGuy.Navigator.speed = 45;
            bluGuy.Navigator.maxSpeed = 45;

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_blueguy", bluGuy);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_blueguy", bluGuy.Poster);
            ObjMan.Add("Npc_BlueGuy", bluGuy);
        }

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

            PineDebugNpcIconPatch.icons.Add(gifter.character, AssetMan.Get<Texture2D>("GifterTex/BorderGifter"));

            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 2, 42f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("GifterTex/Gifter_Sheet"));

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
            ObjMan.Add("Npc_Gifter", gifter);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(ObjMan.Get<BlueGuy>("Npc_BlueGuy"), scene, 90, true);
                AddToNpcs(ObjMan.Get<Gifter>("Npc_Gifter"), scene, 125, true);
                return;
            }

            if (title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();

                AddToNpcs(ObjMan.Get<Gifter>("Npc_Gifter"), scene, 125, false);
                if (id > 0)
                    AddToNpcs(ObjMan.Get<BlueGuy>("Npc_BlueGuy"), scene, id > 1 ? 100 : 45, false, 1);
            }
        }

        private void AddToNpcs(NPC npc, SceneObject scene, int weight, bool endless, int firstNo = 0)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                scene.potentialNPCs.Add(npc.Weighted(weight));
            else if (endless || scene.levelNo == firstNo)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(npc);
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            }
        }
    }
}
