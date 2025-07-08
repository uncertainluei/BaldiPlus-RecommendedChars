using BepInEx.Configuration;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;

using BBPlusAnimations.Components;

using BaldiLevelEditor;
using PlusLevelLoader;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LegacyEditor;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;
using BaldisBasicsPlusAdvanced.API;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class Module_Circle : Module
    {
        public override string Name => "TCMGB Circle";

        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;
        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleCircle;


        [ModuleLoadEvent(LoadingEventOrder.Pre)]
        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Item", "NerfGun"), x => "NerfGun/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "Circle"), x => "CircleTex/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(Plugin), "Audio", "Circle"), "CircleAud/");

            LoadNerfGun();
            LoadCircle();

            LevelGeneratorEventPatch.OnNpcAdd += AddItemsToLevel;
        }

        private void LoadNerfGun()
        {
            ItemMetaData nerfGunMeta = new(Info, [])
            {
                flags = ItemFlags.MultipleUse
            };
            nerfGunMeta.tags.AddRange(["adv_normal", "adv_sm_potential_reward"]);

            Items nerfGunEnum = EnumExtensions.ExtendEnum<Items>("RecChars_NerfGun");

            ItemBuilder nerfGunBuilder = new ItemBuilder(Info)
            .SetNameAndDescription("Itm_RecChars_NerfGun2", "Desc_RecChars_NerfGun")
            .SetEnum(nerfGunEnum)
            .SetMeta(nerfGunMeta)
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NerfGun/NerfGun_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NerfGun/NerfGun_Large"), 50f))
            .SetShopPrice(450)
            .SetGeneratorCost(45)
            .SetItemComponent<ITM_NerfGun>();

            ItemObject nerfItm = nerfGunBuilder.Build();
            nerfItm.name = "RecChars NerfGun2";
            nerfItm.item.name = "Itm_NerfGun2";
            AssetMan.Add("NerfGunItem", nerfItm);

            nerfGunBuilder.SetNameAndDescription("Itm_RecChars_NerfGun1", "Desc_RecChars_NerfGun");
            ItemObject nerfItm1 = nerfGunBuilder.Build();
            nerfItm1.name = "RecChars NerfGun1";
            nerfItm1.item.name = "Itm_NerfGun1";
            ((ITM_NerfGun)nerfItm.item).nextStage = nerfItm1;

            AssetMan.Add("NerfGunPoster", ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("NerfGun/hnt_nerfgun"), []));
            AssetMan.Get<PosterObject>("NerfGunPoster").name = "NerfGunPoster";

            // Reverse itemObject list so the (2) variant is always selected first
            nerfGunMeta.itemObjects = nerfGunMeta.itemObjects.Reverse().ToArray();
        }

        private void LoadCircle()
        {
            CircleNpc circle = RecommendedCharsPlugin.CloneComponent<Playtime, CircleNpc>(GameObject.Instantiate((Playtime)NPCMetaStorage.Instance.Get(Character.Playtime).value, MTM101BaldiDevAPI.prefabTransform));
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
            circle.normSpeed = 65f;
            circle.runSpeed = 75f;

            circle.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("CircleTex/pri_circle"), "PST_PRI_RecChars_Circle1", "PST_PRI_RecChars_Circle2");
            circle.poster.textData[1].font = BaldiFonts.ComicSans18.FontAsset();
            circle.poster.textData[1].fontSize = 18;
            circle.poster.name = "CirclePoster";

            circle.sprite = circle.spriteRenderer[0];
            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 100f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("CircleTex/CircleSprites"));
            circle.sprNormal = sprites[0];
            circle.sprite.sprite = circle.sprNormal;

            circle.sprSad = sprites[1];

            CircleJumprope jumprope = RecommendedCharsPlugin.CloneComponent<Jumprope, CircleJumprope>(GameObject.Instantiate(circle.jumpropePre, MTM101BaldiDevAPI.prefabTransform));
            circle.jumpropePre = jumprope;

            jumprope.name = "ShapeWorld Circle_Jumprope";
            jumprope.ropeAnimator = jumprope.animator.gameObject.AddComponent<CustomSpriteAnimator>();
            jumprope.ropeAnimator.spriteRenderer = jumprope.ropeAnimator.GetComponentInChildren<SpriteRenderer>();
            CircleJumprope.ropeAnimation = new Dictionary<string, Sprite[]> { { "JumpRope", AssetLoader.SpritesFromSpritesheet(4, 4, 1f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("CircleTex/CircleRainbow")) } };
            jumprope.ropeDelay = 0f;
            jumprope.ropeTime = 1f;
            jumprope.maxJumps = 10;
            jumprope.startVal = 100;
            jumprope.penaltyVal = -10;

            AssetMan.Add("CircleJumprope", jumprope);
            AssetMan.Add("CircleNpc", circle);
            NPCMetadata circleMeta = new(Info, [circle], circle.name, NPCMetaStorage.Instance.Get(Character.Playtime).flags | NPCFlags.MakeNoise, ["student", "adv_exclusion_hammer_weakness"]);
            NPCMetaStorage.Instance.Add(circleMeta);
        }

        [ModuleCompatLoadEvent(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Pre)]
        private void AnimationsCompat()
        {
            GameObject.DestroyImmediate(AssetMan.Get<CircleJumprope>("CircleJumprope").GetComponent<GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(AssetMan.Get<CircleNpc>("CircleNpc").GetComponent<GenericAnimationExtraComponent>());
        }

        [ModuleCompatLoadEvent(RecommendedCharsPlugin.LevelLoaderGuid, LoadingEventOrder.Pre)]
        private void RegisterToLevelLoader()
        {
            PlusLevelLoaderPlugin.Instance.npcAliases.Add("recchars_circle", RecommendedCharsPlugin.AssetMan.Get<CircleNpc>("CircleNpc"));
            PlusLevelLoaderPlugin.Instance.itemObjects.Add("recchars_nerfgun", RecommendedCharsPlugin.AssetMan.Get<ItemObject>("NerfGunItem"));
            PlusLevelLoaderPlugin.Instance.posters.Add("recchars_nerfgunposter", RecommendedCharsPlugin.AssetMan.Get<PosterObject>("NerfGunPoster"));
        }

        [ModuleCompatLoadEvent(RecommendedCharsPlugin.LegacyEditorGuid, LoadingEventOrder.Pre)]
        private void RegisterToLegacyEditor()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Editor", "Circle"), x => "CircleEditor/" + x.name);

            LegacyEditorCompatHelper.AddCharacterObject("recchars_circle", AssetMan.Get<CircleNpc>("CircleNpc"));
            BaldiLevelEditorPlugin.itemObjects.Add("recchars_nerfgun", AssetMan.Get<ItemObject>("NerfGunItem"));

            new ExtendedNpcTool("recchars_circle", "CircleEditor/Npc_circle").AddToEditor("characters");
            new ExtendedItemTool("recchars_nerfgun", "CircleEditor/Itm_nerfgun").AddToEditor("items");
        }

        [ModuleCompatLoadEvent(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Pre)]
        private void AdvancedCompat()
        {
            ApiManager.AddNewSymbolMachineWords(Info, "TCMG", "Edits", "Round", "John", "Shape", "World");
            ApiManager.AddNewTips(Info, "Adv_Elv_Tip_RecChars_Circle");
        }

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
                            scene.potentialNPCs.Add(AssetMan.Get<CircleNpc>("CircleNpc").Weighted(3));
                        return;
                    case 1:
                        AddToNpcs(scene, 75);
                        break;
                    default:
                        AddToNpcs(scene, 100);
                        break;
                }
                scene.shopItems = scene.shopItems.AddToArray(new WeightedItemObject() { selection = AssetMan.Get<ItemObject>("NerfGunItem"), weight = 50 });
            }
        }

        private void AddToNpcs(SceneObject scene, int weight, bool endless = false)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                scene.potentialNPCs.Add(AssetMan.Get<CircleNpc>("CircleNpc").Weighted(weight));
            else if (endless || scene.levelNo == 1)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<CircleNpc>("CircleNpc"));
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
                gen.ld.posters = gen.ld.posters.AddToArray(AssetMan.Get<PosterObject>("NerfGunPoster").Weighted(75));
                gen.ld.potentialItems = gen.ld.potentialItems.AddToArray(AssetMan.Get<ItemObject>("NerfGunItem").Weighted(25));
                return;
            }

            gen.ld.posters = gen.ld.posters.AddToArray(AssetMan.Get<PosterObject>("NerfGunPoster").Weighted(100));
            gen.ld.potentialItems = gen.ld.potentialItems.AddToArray(AssetMan.Get<ItemObject>("NerfGunItem").Weighted(50));
            gen.ld.shopItems = gen.ld.shopItems.AddToArray(new WeightedItemObject() { selection = AssetMan.Get<ItemObject>("NerfGunItem"), weight = 50 });
        }
    }
}
