using BepInEx.Configuration;

using HarmonyLib;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;

using System;
using System.IO;

using UncertainLuei.BaldiPlus.RecommendedChars.Patches;

using UnityEngine;
using UnityEngine.UI;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public sealed class Module_ArtsWithWires : Module
    {
        public override string Name => "Arts with Wires";
        public override string SaveTag => Name + (RecommendedCharsConfig.intendedWiresBehavior.Value ? " (v1.1.1+)" : "");

        public override Action LoadAction => Load;
        public override Action<string, int, SceneObject> FloorAddendAction => FloorAddend;

        protected override ConfigEntry<bool> ConfigEntry => RecommendedCharsConfig.moduleArtsWWires;

        private void Load()
        {
            AssetMan.AddRange(AssetLoader.TexturesFromMod(Plugin, "*.png", "Textures", "Npc", "ArtsWWires"), x => "WiresTex/" + x.name);
            RecommendedCharsPlugin.AddAudioClipsToAssetMan(Path.Combine(AssetLoader.GetModPath(Plugin), "Audio", "ArtsWWires"), "WiresAud/");

            ArtsWithWires artsWithWires = new NPCBuilder<ArtsWithWires>(Info)
                .SetName("ArtsWithWires")
                .SetEnum("RecChars_ArtsWithWires")
                .SetPoster(AssetMan.Get<Texture2D>("WiresTex/pri_wires"), "RecChars_Pst_Wires1", "RecChars_Pst_Wires2")
                .AddMetaFlag(NPCFlags.Standard)
                .AddLooker()
                .AddTrigger()
                .Build();

            PineDebugNpcIconPatch.icons.Add(artsWithWires.character, AssetMan.Get<Texture2D>("WiresTex/BorderWires"));

            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 50f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("WiresTex/WiresSprites"));

            artsWithWires.sprite = artsWithWires.spriteRenderer[0];
            artsWithWires.sprite.transform.localPosition = Vector3.zero;

            artsWithWires.sprite.sprite = sprites[0];
            artsWithWires.sprNormal = sprites[0];
            artsWithWires.sprAngry = sprites[1];

            artsWithWires.audMan = artsWithWires.GetComponent<AudioManager>();
            artsWithWires.audMan.subtitleColor = new Color(138f / 255f, 22f / 255f, 15f / 255f);

            artsWithWires.audIntro = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("WiresAud/AWW_Intro"), "RecChars_Wires_Intro", SoundType.Effect, artsWithWires.audMan.subtitleColor);
            artsWithWires.audLoop = ObjectCreators.CreateSoundObject(AssetMan.Get<AudioClip>("WiresAud/AWW_Loop"), "RecChars_Wires_Intro", SoundType.Effect, artsWithWires.audMan.subtitleColor);

            artsWithWires.stareStacks = RecommendedCharsConfig.intendedWiresBehavior.Value;

            Jumprope jumpropeCopy = GameObject.Instantiate(((Playtime)NPCMetaStorage.Instance.Get(Character.Playtime).value).jumpropePre, MTM101BaldiDevAPI.prefabTransform);
            jumpropeCopy.name = "ArtsWithWires GrabbingGame";
            jumpropeCopy.enabled = false;

            artsWithWires.gamePrefab = jumpropeCopy.gameObject.AddComponent<GrabbingGame>();
            artsWithWires.gamePrefab.textCanvas = jumpropeCopy.textCanvas;
            artsWithWires.gamePrefab.textScaler = jumpropeCopy.textScaler;
            artsWithWires.gamePrefab.instructionsTmp = jumpropeCopy.instructionsTmp;
            artsWithWires.gamePrefab.instructionsTmp.rectTransform.anchoredPosition = Vector2.up * 32f;

            GameObject.DestroyImmediate(jumpropeCopy.countTmp.gameObject);
            GameObject.DestroyImmediate(jumpropeCopy.ropeCanvas.gameObject);
            GameObject.DestroyImmediate(jumpropeCopy);

            Sprite background = Sprite.Create(AssetMan.Get<Texture2D>("WiresTex/WiresGrabMeter"), new Rect(0, 0, 136, 24), Vector2.one / 2f);
            background.name = "WiresGrabMeter_Background";

            artsWithWires.gamePrefab.needle = UIHelpers.CreateImage(background, artsWithWires.gamePrefab.textCanvas.transform, default, false).rectTransform;
            artsWithWires.gamePrefab.needle.anchorMin = Vector2.one / 2f;
            artsWithWires.gamePrefab.needle.anchorMax = artsWithWires.gamePrefab.needle.anchorMin;
            artsWithWires.gamePrefab.needle.pivot = artsWithWires.gamePrefab.needle.anchorMin;
            artsWithWires.gamePrefab.needle.anchoredPosition = Vector2.up * -32f;
            artsWithWires.gamePrefab.needle.name = "Meter_BG";

            background = Sprite.Create(AssetMan.Get<Texture2D>("WiresTex/WiresGrabMeter"), new Rect(136, 0, 8, 24), Vector2.one / 2f);
            background.name = "WiresGrabMeter_Needle";

            artsWithWires.gamePrefab.needle = GameObject.Instantiate(artsWithWires.gamePrefab.needle, artsWithWires.gamePrefab.needle.parent, false);
            artsWithWires.gamePrefab.needle.GetComponent<Image>().sprite = background;
            artsWithWires.gamePrefab.needle.sizeDelta = new Vector2(8f, 24f);
            artsWithWires.gamePrefab.needle.name = "Meter_Needle";

            AssetMan.Add("ArtsWithWiresNpc", artsWithWires);
        }

        private void FloorAddend(string title, int id, SceneObject scene)
        {
            if (title == "END")
            {
                scene.MarkAsNeverUnload();

                if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<ArtsWithWires>("ArtsWithWiresNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
                else
                    scene.potentialNPCs.CopyCharacterWeight(Character.DrReflex, AssetMan.Get<ArtsWithWires>("ArtsWithWiresNpc"));
                return;
            }

            if (title.StartsWith("F") && id > 0)
            {
                scene.MarkAsNeverUnload();

                if (!RecommendedCharsConfig.guaranteeSpawnChar.Value)
                {
                    scene.potentialNPCs.CopyCharacterWeight(Character.DrReflex, AssetMan.Get<ArtsWithWires>("ArtsWithWiresNpc"));
                }
                else if (id == 1)
                {
                    scene.forcedNpcs = scene.forcedNpcs.AddToArray(AssetMan.Get<ArtsWithWires>("ArtsWithWiresNpc"));
                    scene.additionalNPCs = Mathf.Max(scene.additionalNPCs - 1, 0);
                }
            }
        }
    }
}
