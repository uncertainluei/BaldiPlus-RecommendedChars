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
    [CaudexModule("Blue, Guy"), CaudexModuleSaveTag("Mdl_BlueGuy")]
    [CaudexModuleConfig("Modules", "BlueGuy",
        "Adds \"Blue, Guy\" from LOLdi's Basics.", true)]
    public sealed partial class Module_BlueGuy : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            ObjectCreation.AddTexturesToAssetMan("BluTex/", ["Textures", "Npc", "BlueGuy"]);
            ObjectCreation.AddAudioToAssetMan("BluAud/", ["Audio", "Npc", "BlueGuy"]);

            AssetMan.Add("StatusSpr/BlueGuyFog", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("BluTex/BlueGuyFogIcon"), 1));
            ObjMan.Add<Fog>("Fog/BlueGuyFog", new() { color = Color.blue, maxDist = 15, startDist = 5, strength = 1, priority = 16});

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "BlueGuy.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
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

            PineDebugNpcIcons.AddIcon([bluGuy], "BorderBlueGuy.png");
            CharacterRadarColorPatch.colors.Add(bluGuy.character, bluGuy.audMan.subtitleColor);

            bluGuy.audIntro = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BluAud/Blu_Intro"), "Vfx_RecChars_BlueGuy_Intro", SoundType.Voice, bluGuy.audMan.subtitleColor);
            bluGuy.audLoop = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("BluAud/Blu_Loop"), "Vfx_RecChars_BlueGuy_Loop", SoundType.Voice, bluGuy.audMan.subtitleColor);

            bluGuy.Navigator.speed = 45;
            bluGuy.Navigator.maxSpeed = 45;

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_blueguy", bluGuy);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_blueguy", bluGuy.Poster);
            ObjMan.Add("Npc/BlueGuy", bluGuy);
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true)
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(ObjMan.Get<BlueGuy>("Npc/BlueGuy"), scene, 90, true);
                return;
            }
            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(ObjMan.Get<BlueGuy>("Npc/BlueGuy"), scene, id > 1 ? 100 : 45, false, 1);
            }
        }

        private void AddToNpcs(NPC npc, SceneObject scene, int weight, bool endless, int firstNo = 0)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                scene.potentialNPCs.Add(npc.Weighted(weight));
            else if (endless || scene.levelNo == firstNo)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(npc);
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs-1, 0);
            }
        }
    }
}
