using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using PlusStudioLevelLoader;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Noongus"), CaudexModuleSaveTag("Mdl_Noongus")]
    [CaudexModuleConfig("Modules", "Noongus",
        "An unknown creature who loves bricks!", false)]
    public sealed class Module_Noongus : RecCharsModule
    {
        internal override byte IconId => 11;

        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddTexturesToAssetManWLegacy("NoonTex/", ["Textures", "Npc", "Noongus"]);
            ObjectCreation.AddAudioToAssetMan("NoonAud/", ["Audio", "Npc", "Noongus"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "Noongus.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadNoongus()
        {
            Noongus noongus = new NPCBuilder<Noongus>(Plugin)
                .SetName("Noongus")
                .SetEnum("RecChars_Noongus")
                .SetPoster(AssetMan.Get<Texture2D>("NoonTex/pri_noongus"), "PST_PRI_RecChars_Noongus1", "PST_PRI_RecChars_Noongus2")
                .AddMetaFlag(NPCFlags.Standard)
                .SetMetaTags(["adv_exclusion_hammer_weakness"])
                .AddTrigger()
                .AddLooker()
                .Build();

            PineDebugNpcIcons.AddIcon([noongus], "BorderNoongus.png");

            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(3, 1, 35f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>(RecommendedCharsPlugin.PartyMode ? "NoonTex/Noongus_Party" : "NoonTex/Noongus"));

            noongus.navigator.SetSpeed(20f);

            noongus.sprite = noongus.spriteRenderer[0];
            noongus.sprite.transform.localPosition = Vector3.up * -1.8f;

            noongus.sprite.sprite = sprites[0];
            noongus.sprIdle = sprites[0];
            noongus.sprThrow = sprites[1];
            noongus.sprSpot = sprites[2];

            noongus.audMan = noongus.GetComponent<AudioManager>();
            noongus.audMan.subtitleColor = new(1f, 143/255f, 0f);
            noongus.audMan.usesSfx = true;
            noongus.audMan.usesVfx = true;

            noongus.audIdle = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NoonAud/Noon_Idle"), "Vfx_RecChars_Noongus_Idle", SoundType.Voice, noongus.audMan.subtitleColor);
            noongus.audSpotted = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NoonAud/Noon_Spotted"), "Vfx_RecChars_Noongus_Spotted", SoundType.Voice, noongus.audMan.subtitleColor);
            noongus.audThrow = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("NoonAud/BricksLanding"), "Sfx_RecChars_BricksLand", SoundType.Effect, Color.white);

            LoadBricks(noongus);

            CharacterRadarColorPatch.colors.Add(noongus.character, noongus.audMan.subtitleColor);

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_noongus", noongus);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_noongus", noongus.Poster);

            SurpriseNpc.possibleVisuals.Add(new SurpriseNpcVisualSprite(noongus, sprites[2], noongus.audSpotted));
            ObjMan.Add("Npc/Noongus", noongus);
        }

        private void LoadBricks(Noongus prefab)
        {
            prefab.brickPre = new ITM_NanaPeel[2];

            ITM_NanaPeel nana = (ITM_NanaPeel)ItemMetaStorage.Instance.FindByEnum(Items.NanaPeel).value.item;
            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 35f, new Vector2(0.5f, 0f), AssetMan.Get<Texture2D>("NoonTex/NoongusBricks"));

            Entity entity = new EntityBuilder()
                .SetName("NoongusBrick_Red")
                .SetBaseRadius(2f)
                .SetLayerCollisionMask(nana.entity.collisionLayerMask)
                .AddTrigger(2f)
                .AddDefaultRenderBaseFunction(sprites[0])
                .Build();
            prefab.brickPre[0] = entity.gameObject.AddComponent<ITM_NanaPeel>();
            prefab.brickPre[0].entity = entity;
            prefab.brickPre[0].endAngle = 181f;
            prefab.brickPre[0].gravity = 25f;
            prefab.brickPre[0].startHeight = 4f;
            prefab.brickPre[0].endHeight = 0f;
            prefab.brickPre[0].maxTime = 5f;
            prefab.brickPre[0].speed = 26f;

            PropagatedAudioManager audMan = entity.gameObject.AddComponent<PropagatedAudioManager>();
            audMan.maxDistance = 80f;
            prefab.brickPre[0].audioManager = audMan;
            prefab.brickPre[0].audEnd = nana.audEnd;
            prefab.brickPre[0].audSlipping = nana.audSlipping;
            prefab.brickPre[0].audSplat = AssetMan.Get<SoundObject>("Sfx/Silence");

            prefab.brickPre[1] = GameObject.Instantiate(prefab.brickPre[0], MTM101BaldiDevAPI.prefabTransform);
            prefab.brickPre[1].name = "NoongusBrick_Blue";
            prefab.brickPre[1].GetComponentInChildren<SpriteRenderer>().sprite = sprites[1];
        }

        //[CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true)
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(scene, 125, true);
                return;
            }
            if (title.StartsWith("F") && id > 0 && id < 3)
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(scene, 150);
            }
        }

        private void AddToNpcs(SceneObject scene, int weight, bool endless = false)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar)
                scene.potentialNPCs.Add(ObjMan.Get<Noongus>("Npc/Noongus").Weighted(weight));
            else if (endless || scene.levelNo == 1)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<Noongus>("Npc/Noongus"));
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            }
        }
    }
}
