using BepInEx.Configuration;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;

using BBPlusAnimations.Components;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;
using BaldisBasicsPlusAdvanced.API;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util.Extensions;
using UncertainLuei.CaudexLib.Registers;
using PlusStudioLevelLoader;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("TCMGBiMaE Circle"), CaudexModuleSaveTag("Mdl_Circle")]
    [CaudexModuleConfig("Modules", "Circle",
        "Adds Circle and Nerf Gun from TCMG's Basics in Mods and Edits.", true)]
    public sealed class Module_Circle : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            AddTexturesToAssetMan("NerfGun/", ["Textures", "Item", "NerfGun"]);
            AddTexturesToAssetMan("CircleTex/", ["Textures", "Npc", "Circle"]);

            AddAudioToAssetMan("CircleAud/", ["Audio", "Circle"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Circle.json5");

            // Load patches
            Hooks.PatchAll(typeof(CirclePatches));
            RecommendedCharsPlugin.PatchCompat(typeof(CircleMusicCompatPatch), RecommendedCharsPlugin.CustomMusicsGuid);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            LoadNerfGun();
            LoadCircle();

            CaudexGeneratorEvents.AddAction(CaudexGeneratorEventType.NpcPrep, AddItemsToLevel);
        }

        private void LoadNerfGun()
        {
            ItemBuilder nerfGunBuilder = new ItemBuilder(Plugin)
            .SetNameAndDescription("Itm_RecChars_NerfGun", "Desc_RecChars_NerfGun")
            .SetEnum("RecChars_NerfGun")
            .SetMeta(ItemFlags.MultipleUse, ["adv_normal", "adv_sm_potential_reward"])
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NerfGun/NerfGun_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NerfGun/NerfGun_Large"), 50f))
            .SetShopPrice(450)
            .SetGeneratorCost(45)
            .SetItemComponent<ITM_NerfGun>();

            if (!RecommendedCharsConfig.nerfCircle.Value)
                nerfGunBuilder.SetShopPrice(500).SetGeneratorCost(75);

            ItemObject nerfGun = nerfGunBuilder.BuildAsMulti(2);
            LevelLoaderPlugin.Instance.itemObjects.Add("recchars_nerfgun", nerfGun);
            ObjMan.Add("Itm_NerfGun", nerfGun);

            PosterObject nerfGunHint = ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("NerfGun/hnt_nerfgun"), []);
            nerfGunHint.name = "NerfGunPoster";
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_nerfgun_hint", nerfGunHint);
            ObjMan.Add("Pst_NerfGunHint", nerfGunHint);
        }

        private void LoadCircle()
        {
            CircleNpc circle = RecommendedCharsPlugin.SwapComponentSimple<Playtime, CircleNpc>(GameObject.Instantiate((Playtime)NPCMetaStorage.Instance.Get(Character.Playtime).value, MTM101BaldiDevAPI.prefabTransform));
            circle.name = "ShapeWorld Circle";

            CircleNpc.charEnum = EnumExtensions.ExtendEnum<Character>("RecChars_Circle");
            PineDebugNpcIconPatch.icons.Add(CircleNpc.charEnum, AssetMan.Get<Texture2D>("CircleTex/BorderCircle"));

            circle.character = CircleNpc.charEnum;
            circle.looker.npc = circle;
            circle.navigator.npc = circle;

            circle.animator.enabled = false;

            PropagatedAudioManager music = circle.GetComponents<PropagatedAudioManager>()[1];
            music.soundOnStart[0] = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Music"), "Mfx_RecChars_JingleBells", SoundType.Effect, circle.audMan.subtitleColor);
            circle.audMan.subtitleColor = music.subtitleColor = new(52/255f, 182/255f, 69/255f);
            CharacterRadarColorPatch.colors.Add(CircleNpc.charEnum, circle.audMan.subtitleColor);

            circle.audCount = new SoundObject[9];
            for (int i = 0; i < 9; i++)
                circle.audCount[i] = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>($"CircleAud/Circle_{i+1}"), $"Vfx_Playtime_{i+1}", SoundType.Voice, circle.audMan.subtitleColor);

            circle.audLetsPlay = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_LetsPlay"), "Vfx_RecChars_Circle_LetsPlay", SoundType.Voice, circle.audMan.subtitleColor);
            circle.audGo = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_ReadyGo"), "Vfx_RecChars_Circle_Go", SoundType.Voice, circle.audMan.subtitleColor);
            circle.audOops = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Oops"), "Vfx_RecChars_Circle_Oops", SoundType.Voice, circle.audMan.subtitleColor);
            circle.audCongrats = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Congrats"), "Vfx_RecChars_Circle_Congrats", SoundType.Voice, circle.audMan.subtitleColor);
            circle.audSad = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Sad"), "Vfx_RecChars_Circle_Sad", SoundType.Voice, circle.audMan.subtitleColor);

            circle.audCalls =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Random1"), "Vfx_RecChars_Circle_Random", SoundType.Voice, circle.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Random2"), "Vfx_RecChars_Circle_Random", SoundType.Voice, circle.audMan.subtitleColor)
            ];

            // The default speed was 500 but this should flow better in-game
            circle.normSpeed = 30f;
            circle.runSpeed = 50f;
            circle.initialCooldown = 25f;

            circle.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("CircleTex/pri_circle"), "PST_PRI_RecChars_Circle1", "PST_PRI_RecChars_Circle2");
            circle.poster.textData[1].font = BaldiFonts.ComicSans18.FontAsset();
            circle.poster.textData[1].fontSize = 18;
            circle.poster.name = "CirclePoster";

            circle.sprite = circle.spriteRenderer[0];
            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 100f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("CircleTex/CircleSprites"));
            circle.sprNormal = sprites[0];
            circle.sprite.sprite = circle.sprNormal;

            circle.sprSad = sprites[1];

            CircleJumprope jumprope = RecommendedCharsPlugin.SwapComponentSimple<Jumprope, CircleJumprope>(GameObject.Instantiate(circle.jumpropePre, MTM101BaldiDevAPI.prefabTransform));
            circle.jumpropePre = jumprope;

            jumprope.name = "ShapeWorld Circle_Jumprope";
            jumprope.ropeAnimator = jumprope.animator.gameObject.AddComponent<CustomSpriteAnimator>();
            jumprope.ropeAnimator.spriteRenderer = jumprope.ropeAnimator.GetComponentInChildren<SpriteRenderer>();
            CircleJumprope.ropeAnimation = new Dictionary<string, Sprite[]> { { "JumpRope", AssetLoader.SpritesFromSpritesheet(4, 4, 1f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("CircleTex/CircleRainbow")) } };
            jumprope.ropeDelay = 0f;
            jumprope.ropeTime = 1f;
            jumprope.maxJumps = 8;
            jumprope.startVal = 64;
            jumprope.penaltyVal = -8;

            ObjMan.Add("Npc_Circle_Nerfed", circle);
            ObjMan.Add("Comp_CircleJumprope_Nerfed", jumprope);
            
            CircleNpc unnerfedCircle = GameObject.Instantiate(circle, MTM101BaldiDevAPI.prefabTransform);
            unnerfedCircle.name = "ShapeWorld Circle Unnerfed";

            unnerfedCircle.normSpeed = 90f;
            unnerfedCircle.runSpeed = 90f;
            unnerfedCircle.sadSpeed = 90f;
            unnerfedCircle.initialCooldown = 15f;
            unnerfedCircle.successCooldown = 15f;

            CircleJumprope unnerfedJumprope = GameObject.Instantiate(jumprope, MTM101BaldiDevAPI.prefabTransform);
            unnerfedJumprope.name = "ShapeWorld Circle_Jumprope Unnerfed";

            unnerfedCircle.jumpropePre = unnerfedJumprope;
            unnerfedJumprope.maxJumps = 10;
            unnerfedJumprope.startVal = 43;
            unnerfedJumprope.penaltyVal = -5;

            ObjMan.Add("Npc_Circle_Unnerfed", unnerfedCircle);
            ObjMan.Add("Comp_CircleJumprope_Unnerfed", unnerfedJumprope);

            if (!RecommendedCharsConfig.nerfCircle.Value)
            {
                ObjMan.Add("Npc_Circle", unnerfedCircle);
                ObjMan.Add("Comp_CircleJumprope", unnerfedJumprope);
            }
            else
            {
                ObjMan.Add("Npc_Circle", circle);
                ObjMan.Add("Comp_CircleJumprope", jumprope);
            }

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_circle", circle);
            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_circle_og", unnerfedCircle);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_circle", circle.Poster);
            NPCMetadata circleMeta = new(Plugin, [circle, unnerfedCircle], circle.name, NPCMetaStorage.Instance.Get(Character.Playtime).flags | NPCFlags.MakeNoise, ["student", "adv_exclusion_hammer_weakness"]);
            NPCMetaStorage.Instance.Add(circleMeta);
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Pre)]
        private void AnimationsCompat()
        {
            GameObject.DestroyImmediate(ObjMan.Get<CircleNpc>("Npc_Circle_Nerfed").GetComponent<GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(ObjMan.Get<CircleNpc>("Npc_Circle_Unnerfed").GetComponent<GenericAnimationExtraComponent>());
            
            GameObject.DestroyImmediate(ObjMan.Get<CircleJumprope>("Comp_CircleJumprope_Nerfed").GetComponent<GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(ObjMan.Get<CircleJumprope>("Comp_CircleJumprope_Unnerfed").GetComponent<GenericAnimationExtraComponent>());
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Pre)]
        private void AdvancedCompat()
        {
            ApiManager.AddNewSymbolMachineWords(Plugin, "TCMG", "edits", "round", "John", "shape");
            ApiManager.AddNewTips(Plugin, "Adv_Elv_Tip_RecChars_Circle");
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(scene, 100, true);
                return;
            }

            if (title.StartsWith("F"))
            {
                scene.MarkAsNeverUnload();

                switch (id)
                {
                    case 0:
                        // A 1 in 1000 chance is kinda impossible to predict so instead it's pretty low weight, also if you have guaranteed spawns it only spawns on F2
                        if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                            scene.potentialNPCs.Add(ObjMan.Get<CircleNpc>("Npc_Circle").Weighted(3));
                        return;
                    case 1:
                        AddToNpcs(scene, 75);
                        break;
                    default:
                        AddToNpcs(scene, 100);
                        break;
                }
                scene.shopItems = scene.shopItems.AddToArray(new WeightedItemObject() { selection = ObjMan.Get<ItemObject>("Itm_NerfGun"), weight = 50 });
            }
        }

        private void AddToNpcs(SceneObject scene, int weight, bool endless = false)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                scene.potentialNPCs.Add(ObjMan.Get<CircleNpc>("Npc_Circle").Weighted(weight));
            else if (endless || scene.levelNo == 1)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<CircleNpc>("Npc_Circle"));
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            }
        }

        private void AddItemsToLevel(LevelGenerator gen)
        {
            if (gen.scene == null) return;
            if (gen.Ec.npcsToSpawn.FirstOrDefault(x => x != null && x.Character == CircleNpc.charEnum) == null) return;

            SceneObjectMetadata meta = SceneObjectMetaStorage.Instance.Get(gen.scene);
            if (meta == null || !meta.tags.Contains("endless") || gen.scene.levelTitle != "END")
            {
                gen.ld.posters = gen.ld.posters.AddToArray(ObjMan.Get<PosterObject>("Pst_NerfGunHint").Weighted(75));
                gen.ld.potentialItems = gen.ld.potentialItems.AddToArray(ObjMan.Get<ItemObject>("Itm_NerfGun").Weighted(25));
                return;
            }

            gen.ld.posters = gen.ld.posters.AddToArray(ObjMan.Get<PosterObject>("Pst_NerfGunHint").Weighted(100));
            gen.ld.potentialItems = gen.ld.potentialItems.AddToArray(ObjMan.Get<ItemObject>("Itm_NerfGun").Weighted(50));
            gen.ld.shopItems = gen.ld.shopItems.AddToArray(new WeightedItemObject() { selection = ObjMan.Get<ItemObject>("Itm_NerfGun"), weight = 50 });
        }
    }
}
