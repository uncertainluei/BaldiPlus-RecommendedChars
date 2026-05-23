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

            CharacterRadarColorPatch.colors.Add(noongus.character, noongus.audMan.subtitleColor);

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_noongus", noongus);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_noongus", noongus.Poster);
            ObjMan.Add("Npc/Noongus", noongus);
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
