using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;

using System.IO;
using TMPro;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Evil Maintenance Machine (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_MaintenanceMachine : RecCharsEditorSubModule<Module_MaintenanceMachine>
    {
        protected override void Initialized()
        {
            // Load icon asset
            AssetMan.Add("EditorSpr/Npc_MaintenanceMachine", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "MaintenanceMachine.png"));
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_maintmachine", ObjMan.Get<MaintenanceMachine>("Npc/MaintMachine"));
            LevelStudioPlugin.Instance.npcPropertyTypes.Add("recchars_maintmachine", new()
            {
                npcPropertiesType = typeof(MaintenanceMachineProperties),
                pageType = typeof(MaintenanceMachinePropExchangeHandler), 
                pagePath = Path.Combine(AssetLoader.GetModPath(BasePlugin), "Data", "UI", "LevelStudio", "Settings_MaintenanceMachine.json")
            });
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.InsertToolInCategory(mode, "npcs", "npc_sweep",
                new NPCTool("recchars_maintmachine", AssetMan.Get<Sprite>("EditorSpr/Npc_MaintenanceMachine")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_maintmachine").SetModdedFrame());
        }

        internal class MaintenanceMachineProperties : GottaSweepProperties
        {
            public float dropChance = 10f;
            private new const byte version = 0;


            public MaintenanceMachineProperties() : base()
            {
                minDelay = 30f;
                maxDelay = 60f;
                minActive = 30f;
                maxActive = 60f;
            }

            public override GameObject[] GeneratePrefabs(NPC baseNpc)
            {
                MaintenanceMachine machine = GameObject.Instantiate((MaintenanceMachine)baseNpc, MTM101BaldiDevAPI.prefabTransform);
                machine.name = "EvilMaintenanceMachine_Customized";
                machine.delayTimeRange = new(minDelay, maxDelay);
                machine.activeTimeRange = new(minActive, maxActive);
                machine.dropChance = dropChance*0.01f;
                return [machine.gameObject];
            }

            public override bool IsAtDefaultSettings()
            =>
                minDelay == 30f &&
                maxDelay == 60f &&
                minActive == 30f &&
                maxActive == 60f &&
                dropChance == 10f;

            public override void Write(BinaryWriter writer)
            {
                writer.Write(version);
                writer.Write(minDelay);
                writer.Write(maxDelay);
                writer.Write(minActive);
                writer.Write(maxActive);
                writer.Write(dropChance);
            }

            public override void ReadInto(BinaryReader reader)
            {
                byte ver = reader.ReadByte();
                minDelay = reader.ReadSingle();
                maxDelay = reader.ReadSingle();
                minActive = reader.ReadSingle();
                maxActive = reader.ReadSingle();
                dropChance = reader.ReadSingle();
            }
        }

        internal class MaintenanceMachinePropExchangeHandler : GottaSweepPropExchangeHandler
        {
            public TextMeshProUGUI dropChanceBox;
            public MaintenanceMachineProperties Properties => (MaintenanceMachineProperties)properties;

            public override void OnElementsCreated()
            {
                base.OnElementsCreated();
                dropChanceBox = transform.Find("DropChanceBox").GetComponent<TextMeshProUGUI>();
            }

            public override void SendInteractionMessage(string message, object data)
            {
                if (message == "setDropChance")
                {
                    if (float.TryParse((string)data, out var dropChance))
                    {
                        Properties.dropChance = Mathf.Clamp(dropChance, 0f, 100f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                }
                base.SendInteractionMessage(message, data);
            }

            public override void OnPropertiesAssigned()
            {
                base.OnPropertiesAssigned();
                dropChanceBox.text = Properties.dropChance.ToString();
            }
        }
    }
}
