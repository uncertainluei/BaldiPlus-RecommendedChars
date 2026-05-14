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
using System.Collections.Generic;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    [CaudexModule("Evil Maintenance Machine"), CaudexModuleSaveTag("Mdl_MaintenanceMachine")]
    [CaudexModuleConfig("Modules", "MaintenanceMachine",
        "It's an evil maintenance machine that's powerful... and evil.", true)]
    public sealed partial class Module_MaintenanceMachine : RecCharsModule
    {
        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddTexturesToAssetMan("MMachineTex/", ["Textures", "Npc", "MaintenanceMachine"]);
            ObjectCreation.AddAudioToAssetMan("MMachineAud/", ["Audio", "Npc", "MaintenanceMachine"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Npc", "MaintenanceMachine.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private void LoadMaintenanceMachine()
        {
            MaintenanceMachine machine = new NPCBuilder<MaintenanceMachine>(Plugin)
                .SetName("EvilMaintenanceMachine")
                .SetEnum("RecChars_MaintMachine")
                .SetPoster(AssetMan.Get<Texture2D>("MMachineTex/pri_emm"), "PST_PRI_RecChars_MaintMachine1", "PST_PRI_RecChars_MaintMachine2")
                .AddMetaFlag(NPCFlags.Standard & ~NPCFlags.CanSee)
                .SetMetaTags(["adv_exclusion_hammer_weakness"])
                .AddTrigger()
                .Build();

            machine.poster.textData[0].position = new(16, 48);
            machine.poster.textData[0].size = new(224, 32);
            PineDebugNpcIcons.AddIcon([machine], "BorderMaintenanceMachine.png");

            Sprite[] sprites = AssetLoader.SpritesFromSpritesheet(2, 1, 50f, new Vector2(0.5f, 0.5f), AssetMan.Get<Texture2D>("LSockTex/LockSockSprites"));

            machine.navigator.SetSpeed(25);
            machine.navigator.accel = 15f;

            machine.spriteRenderer[0].sprite = AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("MMachineTex/EvilMaintenanceMachine"), 30f);
            machine.spriteRenderer[0].transform.localPosition = Vector3.down;

            machine.audMan = machine.GetComponent<AudioManager>();
            ((PropagatedAudioManager)machine.audMan).maxDistance = 300;
            machine.audMan.subtitleColor = new(194/255f, 48/255f, 49/255f);

            CharacterRadarColorPatch.colors.Add(machine.character, machine.audMan.subtitleColor);

            LevelLoaderPlugin.Instance.npcAliases.Add("recchars_maintmachine", machine);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_pri_maintmachine", machine.Poster);
            ObjMan.Add("Npc/MaintMachine", machine);

            // Add spawn tag for the maintenance level type
            LevelType.Maintenance.GetMeta().tags.Add("recchars:spawns_maintenance_machine");
        }

        [CaudexGenModEvent(GenerationModType.Addend)]
        private void FloorAddendLvl(string title, int id, CustomLevelObject lvl)
        {
            if (!title.StartsWith("F") || lvl.IsModifiedByMod(Plugin.Metadata.GUID+"/MaintenanceMachine", GenerationStageFlags.Addend))
                return;
            lvl.MarkAsModifiedByMod(Plugin.Metadata.GUID+"/MaintenanceMachine", GenerationStageFlags.Addend);

            if (lvl.type.GetMeta()?.tags.Contains("recchars:spawns_maintenance_machine") == true)
            {
                lvl.MarkAsNeverUnload();
                if (RecommendedCharsConfig.guaranteeSpawnChar.Value)
                    lvl.GetForcedNpcsInclusive().Add(ObjMan.Get<MaintenanceMachine>("Npc/MaintMachine"));
                else
                    lvl.GetPotentialNpcsInclusive().Add(ObjMan.Get<MaintenanceMachine>("Npc/MaintMachine").Weighted(125));
            }
        }
    }
}
