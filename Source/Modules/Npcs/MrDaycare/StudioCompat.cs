using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;
using PlusStudioLevelLoader;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using TMPro;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Mr. Daycare (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_MrDaycare : RecCharsEditorSubModule<Module_MrDaycare>
    {
        protected override void Initialized()
        {
            // Load texture assets            
            AssetMan.Add("EditorSpr/Npc_MrDaycare", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "MrDaycare.png"));
            AssetMan.Add("EditorSpr/Npc_MrDaycare_Unnerfed", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "MrDaycare_Unnerfed.png"));

            AssetMan.Add("EditorSpr/Room_Daycare", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Room", "Daycare.png"));
            AssetMan.Add("EditorSpr/Door_BookGate", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Door", "BookGate.png")); 
            AssetMan.Add("EditorSpr/Window_Daycare", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Door", "Window_Daycare.png"));
            AssetMan.Add("EditorSpr/Light_Daycare", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Light", "Daycare.png"));

            AssetMan.Add("EditorSpr/Poster_DaycareInfo", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Poster", "DaycareInfo.png"));
            AssetMan.Add("EditorSpr/Poster_DaycareRules", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Poster", "DaycareRules.png"));
            AssetMan.Add("EditorSpr/Poster_DaycareClock", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Poster", "DaycareClock.png"));
            
            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Compat", "LevelStudio", "MrDaycare.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            MrDaycare mrDaycare = ObjMan.Get<MrDaycare>("Npc/MrDaycare_Nerfed");
            EditorInterface.AddNPCVisual("recchars_mrdaycare", mrDaycare);
            LevelStudioPlugin.Instance.npcDisplays.Add("recchars_mrdaycare_og", LevelStudioPlugin.Instance.npcDisplays["recchars_mrdaycare"]);
            MrDaycareProperties.timeSounds = new()
            {
                {30, mrDaycare.audTimes[0]},
                {60, mrDaycare.audTimes[1]},
                {100, mrDaycare.audTimes[2]},
                {200, mrDaycare.audTimes[3]},
                {500, mrDaycare.audTimes[4]},
                {3161, mrDaycare.audTimes[5]}
            };
            LevelStudioPlugin.Instance.npcPropertyTypes.Add("recchars_mrdaycare", new()
            {
                npcPropertiesType = typeof(MrDaycareProperties),
                pageType = typeof(MrDaycarePropExchangeHandler), 
                pagePath = Path.Combine(AssetLoader.GetModPath(BasePlugin), "Data", "UI", "LevelStudio", "Settings_MrDaycare.json")
            });

            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_daycare", new("recchars_daycareflor", "recchars_daycarewall", "recchars_daycareceil"));
            EditorInterface.AddWindow("recchars_daycare", LevelLoaderPlugin.Instance.windowObjects["recchars_daycare"]);
            LevelStudioPlugin.Instance.selectableTextures.AddRange(["recchars_daycareflor", "recchars_daycarewall", "recchars_daycareceil"]);
            EditorInterface.AddDoor<DoorDisplay>("recchars_bookgate", DoorIngameStatus.AlwaysDoor, DaycareDoorAssets.mask, [DaycareDoorAssets.template.shut, DaycareDoorAssets.template.shut]);


            LevelStudioPlugin.Instance.editorLevelPreLoadCallbacks.Add(LevelPreload);
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            // In case I add more functional variants
            string[] daycareRoomIds = ["recchars_daycare"];

            List<NPCTool> npcs = [new ExtRoomNpcTool("recchars_mrdaycare", AssetMan.Get<Sprite>("EditorSpr/Npc_MrDaycare"), daycareRoomIds).SetModdedFrame()];
            if (vanillaCompliant)
                npcs.Add(new ExtRoomNpcTool("recchars_mrdaycare_og", AssetMan.Get<Sprite>("EditorSpr/Npc_MrDaycare_Unnerfed"),
                    "Ed_Tool_npc_recchars_daycare_og_Title", "Ed_Tool_npc_recchars_daycare_og_Desc", daycareRoomIds).SetModdedFrame());
            EditorInterfaceModes.InsertToolsInCategory(mode, "npcs", "npc_principal", npcs);
            
            EditorInterfaceModes.InsertToolInCategory(mode, "rooms", "room_office",
                new RoomTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Room_Daycare")).SetModdedFrame());
            EditorInterfaceModes.InsertToolInCategory(mode, "doors", "door_standard",
                new DoorTool("recchars_bookgate", AssetMan.Get<Sprite>("EditorSpr/Door_BookGate")).SetModdedFrame());
            EditorInterfaceModes.InsertToolInCategory(mode, "doors", "window_green",
                new WindowTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Window_Daycare")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "lights",
                new LightTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Light_Daycare")).SetModdedFrame());
            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_daycare").SetModdedFrame(),
                new PosterTool("recchars_daycareinfo", AssetMan.Get<Sprite>("EditorSpr/Poster_DaycareInfo")).SetModdedFrame(),
                new PosterTool("recchars_daycarerules", AssetMan.Get<Sprite>("EditorSpr/Poster_DaycareRules")).SetModdedFrame(),
                new PosterTool("recchars_daycareclock", AssetMan.Get<Sprite>("EditorSpr/Poster_DaycareClock")).SetModdedFrame()
            ]);
        }

        private static void LevelPreload(EditorMode mode, EditorLevelData data)
        {
            if (mode.vanillaComplaint) return;
            for (int i = 0; i < data.npcs.Count; i++)
            {
                if (data.npcs[i]?.npc == "recchars_mrdaycare_og")
                {
                    data.npcs[i].npc = "recchars_mrdaycare";
                    data.npcs[i].properties = MrDaycareProperties.GetUnnerfed();
                }
            }
        }

        internal class MrDaycareProperties : PrincipalProperties
        {
            public static new Dictionary<int, SoundObject> timeSounds = [];
            protected override SoundObject GetSoundFromTime(int time) => timeSounds[time];

            private static readonly int[] defaultTimes = [30, 60, 100, 200, 500, 3161];
            public float moveSpeed = 30f, ruleSensitivityMul = 1f;

            private new const byte version = 0;

            public MrDaycareProperties() : base()
            {
                times = [30, 60];
            }

            public static MrDaycareProperties GetUnnerfed()
            {
                MrDaycareProperties properties = new();
                properties.moveSpeed = 36f;
                properties.ruleSensitivityMul = 1.5f;
                properties.times = new(defaultTimes);
                return properties;
            }

            public override GameObject[] GeneratePrefabs(NPC baseNpc)
            {
                MrDaycare mrDaycare = Object.Instantiate((MrDaycare)baseNpc, MTM101BaldiDevAPI.prefabTransform);
                mrDaycare.name = "MrDaycare_Customized";
                mrDaycare.allKnowing = allKnowing;
                mrDaycare.Navigator.speed = moveSpeed;
                mrDaycare.Navigator.maxSpeed = moveSpeed;
                mrDaycare.ruleSensitivityMul = ruleSensitivityMul;
                mrDaycare.lockTimes = times.ToArray();
                mrDaycare.maxTimeoutLevel = times.Count-1;
                mrDaycare.audTimes = new SoundObject[times.Count];
                for (int i = 0; i < times.Count; i++)
                    mrDaycare.audTimes[i] = GetSoundFromTime(times[i]);

                return [mrDaycare.gameObject];
            }

            public override bool TimesAreDefault()
            {
                if (times.Count != 2) return false;
                for (int i = 0; i < times.Count; i++)
                {
                    if (times[i] != defaultTimes[i])
                        return false;
                }
                return true;
            }

            public override int SnapValue(int time)
            {
                int num = -1;
                int num2 = int.MaxValue;
                for (int i = 0; i < defaultTimes.Length; i++)
                {
                    if (Mathf.Abs(defaultTimes[i] - time) < num2)
                    {
                        num = i;
                        num2 = Mathf.Abs(defaultTimes[i] - time);
                    }
                }
                return defaultTimes[num];
            }

            public override bool IsAtDefaultSettings()
                => base.IsAtDefaultSettings() && moveSpeed == 30f && ruleSensitivityMul == 1f;

            public override void Write(BinaryWriter writer)
            {
                writer.Write(version);
                writer.Write(moveSpeed);
                writer.Write(ruleSensitivityMul);
                writer.Write(allKnowing);
                writer.Write(times.Count);
                for (int i = 0; i < times.Count; i++)
                    writer.Write((short)times[i]);
            }

            public override void ReadInto(BinaryReader reader)
            {
                byte ver = reader.ReadByte();
                moveSpeed = reader.ReadSingle();
                ruleSensitivityMul = reader.ReadSingle();
                allKnowing = reader.ReadBoolean();
                int count = reader.ReadInt32();
                times.Clear();
                for (int i = 0; i < count; i++)
                    times.Add(reader.ReadInt16());
            }
        }

        internal class MrDaycarePropExchangeHandler : PrincipalPropExchangeHandler
        {
            // I'd be better off using a publiciser for this, but that might screw up my current Studio impls so I probably won't
            private static readonly FieldInfo _texts = AccessTools.Field(typeof(PrincipalPropExchangeHandler), "texts");

            public TextMeshProUGUI moveSpeedBox, sensitivityBox;
            public MrDaycareProperties Properties => (MrDaycareProperties)properties;

            public override void OnElementsCreated()
            {
                _texts.SetValue(this, new TextMeshProUGUI[6]); // We only have six boxes
                base.OnElementsCreated();

                moveSpeedBox = transform.Find("MoveSpeedBox").GetComponent<TextMeshProUGUI>();
                sensitivityBox = transform.Find("SensitivityBox").GetComponent<TextMeshProUGUI>();
            }

            public override void SendInteractionMessage(string message, object data)
            {
                switch (message)
                {
                    case "setMoveSpeed":
                        if (float.TryParse((string)data, out var moveSpeed))
                        {
                            Properties.moveSpeed = (float)Mathf.Clamp(moveSpeed, 0f, 90f);
                            propertiesChanged = true;
                        }
                        OnPropertiesAssigned();
                        break;
                    case "setSensitivity":
                        if (float.TryParse((string)data, out var sensitivity))
                        {
                            Properties.ruleSensitivityMul = Mathf.Clamp(sensitivity, 0.1f, 5f);
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
                base.OnPropertiesAssigned();
                moveSpeedBox.text = Properties.moveSpeed.ToString();
                sensitivityBox.text = Properties.ruleSensitivityMul.ToString();
            }
        }
    }
}
