using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

using UnityEngine;
using TMPro;

using System.IO;
using System.Collections.Generic;


namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("TCMGBiMaE Circle (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_Circle : RecCharsEditorSubModule<Module_Circle>
    {
        protected override void Initialized()
        {
            // Load texture assets
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_Circle", ["Textures", "Compat", "LevelStudio", "Npc", "Circle.png"]);
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_Circle_Og", ["Textures", "Compat", "LevelStudio", "Npc", "Circle_Unnerfed.png"]);

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Compat", "LevelStudio", "Circle.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_circle", ObjMan.Get<CircleNpc>("Npc/Circle_Nerfed"));
            LevelStudioPlugin.Instance.npcDisplays.Add("recchars_circle_og", LevelStudioPlugin.Instance.npcDisplays["recchars_circle"]);
            LevelStudioPlugin.Instance.npcPropertyTypes.Add("recchars_circle", new()
            {
                npcPropertiesType = typeof(CircleProperties),
                pageType = typeof(CirclePropExchangeHandler), 
                pagePath = Path.Combine(AssetLoader.GetModPath(BasePlugin), "Data", "UI", "LevelStudio", "Settings_Circle.json")
            });
            LevelStudioPlugin.Instance.editorLevelPreLoadCallbacks.Add(LevelPreload);
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            List<EditorTool> npcTools = [new ExtNpcTool("recchars_circle", AssetMan.Get<Sprite>("EditorSpr/Npc_Circle"),
                    "Ed_Tool_npc_recchars_circle_Desc").SetModdedFrame()];
            if (vanillaCompliant)
                npcTools.Add(new ExtNpcTool("recchars_circle_og", AssetMan.Get<Sprite>("EditorSpr/Npc_Circle_Og"),
                    "Ed_Tool_npc_recchars_circle_og_Title", "Ed_Tool_npc_recchars_circle_og_Desc").SetModdedFrame());

            EditorInterfaceModes.InsertToolsInCategory(mode, "npcs", "npc_playtime", npcTools);
            EditorInterfaceModes.AddToolToCategory(mode, "posters",
                new PosterTool("recchars_pri_circle").SetModdedFrame());
        }

        private static void LevelPreload(EditorMode mode, EditorLevelData data)
        {
            if (mode.vanillaComplaint) return;
            for (int i = 0; i < data.npcs.Count; i++)
            {
                if (data.npcs[i]?.npc == "recchars_circle_og")
                {
                    data.npcs[i].npc = "recchars_circle";
                    data.npcs[i].properties = CircleProperties.GetUnnerfed();
                }
            }
        }

        internal class CircleProperties : PlaytimeProperties
        {
            public bool originalSpeed = false;
            public short pointGain = 64, pointPenalty = 8;

            private new const byte version = 0;

            public CircleProperties() : base()
            {
                jumps = 8;
                jumpDelay = 25f; // Standard cooldown
                ropeSpeed = 50f; // Success cooldown
            }

            public static CircleProperties GetUnnerfed()
            {
                CircleProperties properties = new();
                properties.jumps = 10;
                properties.originalSpeed = true;
                properties.jumpDelay = 15f;
                properties.ropeSpeed = 15f;
                properties.pointGain = 43;
                properties.pointPenalty = 5;
                return properties;
            }

            public override void Write(BinaryWriter writer)
            {
                writer.Write(version);
                writer.Write(jumps);
                writer.Write(jumpDelay);
                writer.Write(ropeSpeed);
                writer.Write(originalSpeed);
                writer.Write(pointGain);
                writer.Write(pointPenalty);
            }

            public override void ReadInto(BinaryReader reader)
            {
                byte ver = reader.ReadByte();
                jumps = reader.ReadByte();
                jumpDelay = reader.ReadSingle();
                ropeSpeed = reader.ReadSingle();
                originalSpeed = reader.ReadBoolean();
                pointGain = reader.ReadInt16();
                pointPenalty = reader.ReadInt16();
            }

            public override bool IsAtDefaultSettings()
            =>
                jumps == 8       &&
                jumpDelay == 25f &&
                ropeSpeed == 50f &&
                !originalSpeed   &&
                pointGain == 64  &&
                pointPenalty == 8;

            public override GameObject[] GeneratePrefabs(NPC baseNpc)
            {
                CircleNpc circle = GameObject.Instantiate(ObjMan.Get<CircleNpc>("Npc/Circle_Unnerfed"), MTM101BaldiDevAPI.prefabTransform);
                CircleJumprope jumprope = GameObject.Instantiate((CircleJumprope)circle.jumpropePre, MTM101BaldiDevAPI.prefabTransform);
                circle.SetNerfedPreset(!originalSpeed);
                circle.name = "ShapeWorld Circle_Customized";
                jumprope.name = "ShapeWorld Circle_Jumprope_Customized";
                circle.initialCooldown = jumpDelay; // Standard cooldown
                circle.successCooldown = ropeSpeed; // Success cooldown
                jumprope.maxJumps = jumps;
                jumprope.totalPoints = pointGain;
                jumprope.penaltyVal = -pointPenalty;
                circle.jumpropePre = jumprope;
                return [circle.gameObject, jumprope.gameObject];
            }
        }

        internal class CirclePropExchangeHandler : NPCPropertyExchangeHandler
        {
            public TextMeshProUGUI jumpBox, cooldownBox, successCooldownBox, pointBox, pointPenaltyBox;
            public MenuToggle toggle;
            public CircleProperties Properties => (CircleProperties)properties;

            public override void OnElementsCreated()
            {
                base.OnElementsCreated();
                jumpBox = transform.Find("JumpBox").GetComponent<TextMeshProUGUI>();
                cooldownBox = transform.Find("CooldownBox").GetComponent<TextMeshProUGUI>();
                successCooldownBox = transform.Find("SuccessCooldownBox").GetComponent<TextMeshProUGUI>();
                pointBox = transform.Find("PointBox").GetComponent<TextMeshProUGUI>();
                pointPenaltyBox = transform.Find("PointPenaltyBox").GetComponent<TextMeshProUGUI>();

                toggle = transform.Find("OriginalSpeedToggle").GetComponent<MenuToggle>();
            }

            public override void SendInteractionMessage(string message, object data)
            {
                switch (message)
                {
                    case "setJumps":
                        if (byte.TryParse((string)data, out var jumps))
                        {
                            Properties.jumps = (byte)Mathf.Clamp(jumps, 1, 10);
                            propertiesChanged = true;
                        }
                        OnPropertiesAssigned();
                        break;
                    case "setCooldown":
                        if (float.TryParse((string)data, out var cool))
                        {
                            Properties.jumpDelay = Mathf.Clamp(cool, 5f, 999999f);
                            if (Properties.jumpDelay > Properties.ropeSpeed)
                                Properties.ropeSpeed = Properties.jumpDelay;
                            propertiesChanged = true;
                        }
                        OnPropertiesAssigned();
                        break;
                    case "setSuccessCooldown":
                        if (float.TryParse((string)data, out var successCool))
                        {
                            Properties.ropeSpeed = Mathf.Clamp(successCool, Properties.jumpDelay, 999999f);
                            propertiesChanged = true;
                        }
                        OnPropertiesAssigned();
                        break;
                    case "toggleOriginalSpeed":
                        Properties.originalSpeed = !Properties.originalSpeed;
                        propertiesChanged = true;
                        OnPropertiesAssigned();
                        break;
                    case "setPointGain":
                        if (short.TryParse((string)data, out var pointGain))
                        {
                            Properties.pointGain = (short)Mathf.Max(pointGain, 0);
                            if (Properties.pointPenalty > Properties.pointGain)
                                Properties.pointPenalty = Properties.pointGain;
                            propertiesChanged = true;
                        }
                        OnPropertiesAssigned();
                        break;
                    case "setPointPenalty":
                        if (short.TryParse((string)data, out var pointPenalty))
                        {
                            Properties.pointPenalty = (short)Mathf.Clamp(pointPenalty, 0, Properties.pointGain);
                            propertiesChanged = true;
                        }
                        OnPropertiesAssigned();
                        break;
                    default:
                        base.SendInteractionMessage(message, data);
                        break;
                }
            }

            public override void OnPropertiesAssigned()
            {
                jumpBox.text = Properties.jumps.ToString();
                cooldownBox.text = Properties.jumpDelay.ToString();
                successCooldownBox.text = Properties.ropeSpeed.ToString();
                pointBox.text = Properties.pointGain.ToString();
                pointPenaltyBox.text = Properties.pointPenalty.ToString();

                toggle.Set(Properties.originalSpeed);
            }
        }
    }

}
