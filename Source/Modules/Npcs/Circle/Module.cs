using AsmResolver.DotNet;
using BaldisBasicsPlusAdvanced.API;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components.Animation;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;
using PlusStudioLevelLoader;
using System;
using System.Linq;
using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("TCMGBiMaE Circle"), CaudexModuleSaveTag("Mdl_Circle")]
    [CaudexModuleConfig("Modules", "Circle",
        "Adds Circle from TCMG's Basics in Mods and Edits.", true)]
    public sealed partial class Module_Npc_Circle : RecCharsModule
    {
        internal override byte IconId => 0;

        protected override void Initialized()
        {
            // Load texture and audio assets
            ObjectCreation.AddTexturesToAssetManWLegacy("CircleTex/", ["Textures", "Npc", "Circle"]);
            ObjectCreation.AddAudioToAssetMan("CircleAud/", ["Audio", "Npc", "Circle"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "Circle.json5");

            // Load patches
            Hooks.PatchAll(typeof(CirclePatches));
            //RecommendedCharsPlugin.PatchCompat(typeof(CircleMusicCompatPatch), RecommendedCharsPlugin.CustomMusicsGuid);
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadCircle()
        {
            CircleNpc circle = GameObject.Instantiate((Playtime)NPCMetaStorage.Instance.Get(Character.Playtime).value, MTM101BaldiDevAPI.prefabTransform).SwapComponentSimple<Playtime, CircleNpc>();
            circle.name = "RecChars ShapeWorld Circle";

            CircleNpc.charEnum = EnumExtensions.ExtendEnum<Character>("RecChars_Circle");
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

            circle.normSpeed = 30f;
            circle.runSpeed = 50f;
            circle.initialCooldown = 25f;

            circle.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("CircleTex/pri_circle"), "PST_PRI_RecChars_Circle1", "PST_PRI_RecChars_Circle2");
            circle.poster.textData[1].font = BaldiFonts.ComicSans18.FontAsset();
            circle.poster.textData[1].fontSize = 18;
            circle.poster.name = "CirclePoster";

            circle.sprite = circle.spriteRenderer[0];

            String spritePath = "CircleTex/Circle";
            DateTime today = DateTime.Today;
            if (RecommendedCharsPlugin.PartyMode)
                spritePath = "CircleTex/Circle_Party";
            else if (today.Month == 12 && today.Day >= 20)
                spritePath = "CircleTex/Circle_Xmas";

            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 100f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>(spritePath));
             
            circle.sprNormal = sprites[0];
            circle.sprSad = sprites[1];

            circle.sprite.sprite = circle.sprNormal;

            CircleJumprope jumprope = GameObject.Instantiate(circle.jumpropePre, MTM101BaldiDevAPI.prefabTransform).SwapComponentSimple<Jumprope, CircleJumprope>();
            circle.jumpropePre = jumprope;

            jumprope.name = "ShapeWorld Circle_Jumprope";
            jumprope.ropeAnimator = jumprope.animator.gameObject.AddComponent<CustomSpriteRendererAnimator>();
            jumprope.ropeAnimator.renderer = jumprope.ropeAnimator.GetComponentInChildren<SpriteRenderer>();
            jumprope.ropeAnimator.AddAnimation("JumpRope", new(15, AssetLoader.SpritesFromSpritesheet(4, 4, 1f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("CircleTex/CircleRainbow"))));

            jumprope.ropeDelay = 0f;
            jumprope.ropeTime = 1f;
            jumprope.maxJumps = 8;
            jumprope.startVal = 64;
            jumprope.penaltyVal = -8;

            ObjMan.Add("Npc/Circle_Nerfed", circle);
            ObjMan.Add("Comp/CircleRope_Nerfed", jumprope);
            
            CircleNpc unnerfedCircle = GameObject.Instantiate(circle, MTM101BaldiDevAPI.prefabTransform);
            unnerfedCircle.name = "ShapeWorld Circle Unnerfed";

            // The original speed in TCMG's Basics was 500 but this should flow better in-game
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

            ObjMan.Add("Npc/Circle_Unnerfed", unnerfedCircle);
            ObjMan.Add("Comp/CircleRope_Unnferfed", unnerfedJumprope);

            PineDebugNpcIcons.AddIcon([circle, unnerfedCircle], "BorderCircle.png");
            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_circle", circle);
            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_circle_og", unnerfedCircle);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_circle", circle.Poster);
            NPCMetadata circleMeta = new(Plugin, [circle, unnerfedCircle], circle.name, NPCMetaStorage.Instance.Get(Character.Playtime).flags | NPCFlags.MakeNoise, ["student", "adv_exclusion_hammer_weakness"]);
            NPCMetaStorage.Instance.Add(circleMeta);
            SetCirclePrefabs();
            RecommendedCharsConfig.nerfCircle.SettingChanged += (x, y) =>
            {
                SetCirclePrefabs();
                UpdateCircleInstances();
            };
        }

        /*[CaudexLoadEventMod(RecommendedCharsPlugin.AnimationsGuid, LoadingEventOrder.Pre)]
        private void AnimationsCompat()
        {
            GameObject.DestroyImmediate(ObjMan.Get<CircleNpc>("Npc/Circle_Nerfed").GetComponent<GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(ObjMan.Get<CircleNpc>("Npc/Circle_Unnerfed").GetComponent<GenericAnimationExtraComponent>());
            
            GameObject.DestroyImmediate(ObjMan.Get<CircleJumprope>("Comp/CircleRope_Nerfed").GetComponent<GenericAnimationExtraComponent>());
            GameObject.DestroyImmediate(ObjMan.Get<CircleJumprope>("Comp/CircleRope_Unnerfed").GetComponent<GenericAnimationExtraComponent>());
        }*/

        [CaudexLoadEventMod(RecommendedCharsPlugin.AdvancedGuid, LoadingEventOrder.Pre)]
        private void AdvancedCompat()
        {
            ApiManager.AddNewSymbolMachineWords(Plugin, "TCMG", "edits", "round", "John", "shape");
            ApiManager.AddNewTips(Plugin, "Adv_Elv_Tip_RecChars_Circle");
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (scene.GetMeta()?.tags.Contains("endless") == true)
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
                        if (!RecommendedCharsConfig.guaranteeSpawnChar)
                            scene.potentialNPCs.Add(ObjMan.Get<CircleNpc>("Npc/Circle").Weighted(3));
                        return;
                    case 1:
                        AddToNpcs(scene, 75);
                        break;
                    default:
                        AddToNpcs(scene, 100);
                        break;
                }
            }
        }

        private void SetCirclePrefabs()
        {
            ObjMan.Add("Npc/Circle", ObjMan.Get<CircleNpc>(RecommendedCharsConfig.nerfCircle.Value ? "Npc/Circle_Nerfed" : "Npc/Circle_Unnerfed"));
            ObjMan.Add("Comp/CircleRope", ObjMan.Get<CircleJumprope>(RecommendedCharsConfig.nerfCircle.Value ? "Comp/CircleRope_Nerfed" : "Comp/CircleRope_Unnerfed"));
            NPCMetaStorage.Instance.Get(CircleNpc.charEnum).ReflectionSetVariable("defaultKey", ObjMan.Get<CircleNpc>("Npc/Circle").name);
        }

        private void UpdateCircleInstances()
        {
            SceneObject scene;
            int i, c;
            foreach (SceneObjectMetadata sceneMeta in SceneObjectMetaStorage.Instance.All().Where(x => x.tags.Contains("endless") == true || x.title.StartsWith("F")))
            {
                scene = sceneMeta.value;
                if (RecommendedCharsConfig.guaranteeSpawnChar)
                {
                    for (i = 0, c = scene.forcedNpcs.Length; i < c; i++)
                    {
                        if (scene.forcedNpcs[i].character == CircleNpc.charEnum)
                            scene.forcedNpcs[i] = ObjMan.Get<CircleNpc>("Npc/Circle");
                    }
                    continue;
                }
                for (i = 0, c = scene.potentialNPCs.Count; i < c; i++)
                {
                    if (scene.potentialNPCs[i].selection?.character == CircleNpc.charEnum)
                        scene.potentialNPCs[i].selection = ObjMan.Get<CircleNpc>("Npc/Circle");
                }
            }
        }

        private void AddToNpcs(SceneObject scene, int weight, bool endless = false)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar)
                scene.potentialNPCs.Add(ObjMan.Get<CircleNpc>("Npc/Circle").Weighted(weight));
            else if (endless || scene.levelNo == 1)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<CircleNpc>("Npc/Circle"));
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            }
        }
    }
}
