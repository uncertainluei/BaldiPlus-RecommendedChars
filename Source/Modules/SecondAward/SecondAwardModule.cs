using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusStudioLevelLoader;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    //[CaudexModule("2nd Award"), CaudexModuleSaveTag("Mdl_SecondAward")]
    [CaudexModuleConfig("Modules", "SecondAward",
        "Adds 2nd Award. It's 1st Prize, but slow and stuns characters.", true)]
    public sealed class Module_SecondAward : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture and audio assets
            AddTexturesToAssetMan("AwaTex/", ["Textures", "Npc", "SecondAward"]);
            AddAudioToAssetMan("AwaAud/", ["Audio", "SecondAward"]);

            AssetMan.Add("StatusSpr/ElectricalStun", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("AwaTex/2AwStunIcon"), 25));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "SecondAward.json5");

            // Load patches
            Hooks.PatchAll(typeof(SecondAwardPatches));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadSecondAward()
        {
            SecondAward secondAward = SwapComponentSimple<FirstPrize, SecondAward>(GameObject.Instantiate((FirstPrize)NPCMetaStorage.Instance.Get(Character.Prize).value, MTM101BaldiDevAPI.prefabTransform));
            secondAward.name = "Second Award";

            SecondAward.charEnum = EnumExtensions.ExtendEnum<Character>("RecChars_SecondAward");
            secondAward.character = SecondAward.charEnum;
            secondAward.looker.npc = secondAward;
            secondAward.navigator.npc = secondAward;

            // Setting sprites
            SpriteRotationMap rotationMap = new();
            rotationMap.angleCount = 16;
            rotationMap.spriteSheet = AssetLoader.SpritesFromSpriteSheetCount(AssetMan.Get<Texture2D>("AwaTex/2AwSprites"), 256, 256, 26);

            secondAward.spriteRenderer[0].sprite = rotationMap.spriteSheet[0];
            secondAward.spriteRenderer[0].GetComponent<AnimatedSpriteRotator>().spriteMap = [rotationMap];

            PineDebugNpcIconPatch.icons.Add(SecondAward.charEnum, AssetMan.Get<Texture2D>("AwaTex/BorderSecondAward"));

            // Subtitle/Radar color
            secondAward.audMan.subtitleColor = secondAward.motorAudMan.subtitleColor = new(219/255f, 159/255f, 86/255f);
            secondAward.audMan.overrideSubtitleColor = false;
            CharacterRadarColorPatch.colors.Add(SecondAward.charEnum, secondAward.audMan.subtitleColor);

            secondAward.wanderSpeed = 24;
            secondAward.chaseSpeed = 24;
            secondAward.minPushSpeed = 24;
            secondAward.slamSpeed = float.MaxValue;

            // Voicelines
            secondAward.audSee =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("AwaAud/2A_Spot1"), "Vfx_RecChars_SecAward_Spot1", SoundType.Voice, secondAward.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("AwaAud/2A_Spot2"), "Vfx_RecChars_SecAward_Spot2", SoundType.Voice, secondAward.audMan.subtitleColor)
            ];
            secondAward.audLose =
            [
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("AwaAud/2A_Lose1"), "Vfx_RecChars_SecAward_Lose1", SoundType.Voice, secondAward.audMan.subtitleColor),
                ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("AwaAud/2A_Lose2"), "Vfx_RecChars_SecAward_Lose2", SoundType.Voice, secondAward.audMan.subtitleColor)
            ];
            secondAward.audHug = secondAward.audRand = [AssetMan.Get<SoundObject>("Sfx/Silence")];
            secondAward.audBang = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("AwaAud/2A_Stun"), "Sfx_RecChars_SecAward_Stun", SoundType.Voice, Color.white);
            secondAward.audBroken = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("AwaAud/2A_Broken"), "Vfx_RecChars_SecAward_Broken", SoundType.Voice, secondAward.audMan.subtitleColor);

            // Office Poster
            secondAward.poster = ObjectCreators.CreateCharacterPoster(AssetMan.Get<Texture2D>("AwaTex/pri_second"), "PST_PRI_RecChars_SecAward1", "PST_PRI_RecChars_SecAward2");
            secondAward.poster.name = "SecondAwardPoster";

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_secondaward", secondAward);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_secaward", secondAward.Poster);
            ObjMan.Add("Npc_SecondAward", secondAward);
            NPCMetaStorage.Instance.Add(new(Plugin, [secondAward], secondAward.name, NPCMetaStorage.Instance.Get(Character.Prize).flags, []));
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
            if (title.StartsWith("F") && id % 2 == 1)
            {
                scene.MarkAsNeverUnload();
                AddToNpcs(scene, 100);
            }
        }

        private void AddToNpcs(SceneObject scene, int weight, bool endless = false)
        {
            if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                scene.potentialNPCs.Add(ObjMan.Get<SecondAward>("Npc_SecondAward").Weighted(weight));
            else if (endless || scene.levelNo == 1)
            {
                scene.forcedNpcs = scene.forcedNpcs.AddToArray(ObjMan.Get<SecondAward>("Npc_SecondAward"));
                scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
            }
        }
    }
}
