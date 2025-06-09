using BepInEx.Configuration;
using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class Module_Circle : Module
    {
        public override string Name => "TCMGB Circle";

        public override Action LoadAction => Load;
        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;
        public override Action<string, int, CustomLevelObject> LevelAddendAction => FloorAddendLvl;

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleCircle;

        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Item", "NerfGun"), x => "NerfGun/" + x.name);
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "Circle"), x => "CircleTex/" + x.name);

            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(Plugin), "Audio", "Circle"), "CircleAud/");

            LoadNerfGun();
            LoadCircle();
        }
        private void LoadNerfGun()
        {
            ItemMetaData nerfGunMeta = new ItemMetaData(Info, new ItemObject[0])
            {
                flags = ItemFlags.MultipleUse
            };

            Items nerfGunEnum = EnumExtensions.ExtendEnum<Items>("RecChars_NerfGun");

            ItemBuilder nerfGunBuilder = new ItemBuilder(Info)
            .SetNameAndDescription("RecChars_Itm_NerfGun2", "RecChars_Desc_NerfGun")
            .SetEnum(nerfGunEnum)
            .SetMeta(nerfGunMeta)
            .SetSprites(AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NerfGun/NerfGun_Small"), 25f), AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("NerfGun/NerfGun_Large"), 50f))
            .SetShopPrice(750)
            .SetGeneratorCost(75)
            .SetItemComponent<ITM_NerfGun>();

            ItemObject nerfItm = nerfGunBuilder.Build();
            nerfItm.name = "RecChars NerfGun2";
            nerfItm.item.name = "Itm_NerfGun2";
            AssetMan.Add("NerfGunItem", nerfItm);

            nerfGunBuilder.SetNameAndDescription("RecChars_Itm_NerfGun1", "RecChars_Desc_NerfGun");
            ItemObject nerfItm1 = nerfGunBuilder.Build();
            nerfItm1.name = "RecChars NerfGun1";
            nerfItm1.item.name = "Itm_NerfGun1";
            ((ITM_NerfGun)nerfItm.item).nextStage = nerfItm1;

            AssetMan.Add("NerfGunPoster", ObjectCreators.CreatePosterObject(AssetMan.Get<Texture2D>("NerfGun/hnt_nerfgun"), new PosterTextData[0]));
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

            circle.audMan.subtitleColor = new Color(52f / 255f, 182f / 255f, 69f / 255f);
            circle.audCount = new SoundObject[9];
            for (int i = 0; i < 9; i++)
                circle.audCount[i] = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>($"CircleAud/Circle_{i + 1}"), $"Vfx_Playtime_{i + 1}", SoundType.Voice, circle.audMan.subtitleColor);

            circle.audLetsPlay = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_LetsPlay"), "Vfx_Playtime_LetsPlay", SoundType.Voice, circle.audMan.subtitleColor);
            circle.audGo = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_ReadyGo"), "RecChars_Circle_Go", SoundType.Voice, circle.audMan.subtitleColor);
            circle.audOops = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Oops"), "RecChars_Circle_Oops", SoundType.Voice, circle.audMan.subtitleColor);
            circle.audCongrats = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Congrats"), "RecChars_Circle_Congrats", SoundType.Voice, circle.audMan.subtitleColor);
            circle.audSad = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Sad"), "RecChars_Circle_Sad", SoundType.Voice, circle.audMan.subtitleColor);

            circle.audCalls = new SoundObject[]
            {
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Random1"), "RecChars_Circle_Random", SoundType.Voice, circle.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Random2"), "RecChars_Circle_Random", SoundType.Voice, circle.audMan.subtitleColor)
            };

            PropagatedAudioManager music = circle.GetComponents<PropagatedAudioManager>()[1];
            music.soundOnStart[0] = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("CircleAud/Circle_Music"), "RecChars_Circle_Music", SoundType.Effect, circle.audMan.subtitleColor);
            music.subtitleColor = circle.audMan.subtitleColor = new Color(52f / 255f, 182f / 255f, 69f / 255f);

            // The default speed was 500 but this should flow better in-game
            circle.normSpeed = 90f;
            circle.runSpeed = 90f;

            circle.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("CircleTex/pri_circle"), "RecChars_Pst_Circle1", "RecChars_Pst_Circle2");
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
            jumprope.penaltyVal = 10;

            AssetMan.Add("CircleNpc", circle);
            NPCMetadata circleMeta = new NPCMetadata(Info, new NPC[] { circle }, circle.name, NPCMetaStorage.Instance.Get(Character.Playtime).flags | NPCFlags.MakeNoise, new string[] { "student" });
            NPCMetaStorage.Instance.Add(circleMeta);
        }

        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();
                scene.shopItems = scene.shopItems.AddToArray(new WeightedItemObject() { selection = AssetMan.Get<ItemObject>("NerfGunItem"), weight = 25 });
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

                scene.shopItems = scene.shopItems.AddToArray(new WeightedItemObject() { selection = AssetMan.Get<ItemObject>("NerfGunItem"), weight = 25 });
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

        private void FloorAddendLvl(string title, int id, LevelObject lvl)
        {
            if (title == "END")
            {
                lvl.posters = lvl.posters.AddToArray(new WeightedPosterObject() { selection = AssetMan.Get<PosterObject>("NerfGunPoster"), weight = 100 });
                lvl.potentialItems = lvl.potentialItems.AddToArray(new WeightedItemObject() { selection = AssetMan.Get<ItemObject>("NerfGunItem"), weight = 50 });
                return;
            }

            if (title.StartsWith("F") && id > 0)
            {
                lvl.posters = lvl.posters.AddToArray(new WeightedPosterObject() { selection = AssetMan.Get<PosterObject>("NerfGunPoster"), weight = 75 });
                lvl.potentialItems = lvl.potentialItems.AddToArray(new WeightedItemObject() { selection = AssetMan.Get<ItemObject>("NerfGunItem"), weight = 25 });
            }
        }
    }
}
