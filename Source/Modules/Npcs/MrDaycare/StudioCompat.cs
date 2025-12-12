using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;
using PlusStudioLevelLoader;

using UnityEngine;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_MrDaycare : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
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

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_mrdaycare", ObjMan.Get<MrDaycare>("Npc_MrDaycare_Nerfed"));
            LevelStudioPlugin.Instance.npcDisplays.Add("recchars_mrdaycare_og", LevelStudioPlugin.Instance.npcDisplays["recchars_mrdaycare"]);

            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_daycare", new("recchars_daycareflor", "recchars_daycarewall", "recchars_daycareceil"));
            EditorInterface.AddWindow("recchars_daycare", LevelLoaderPlugin.Instance.windowObjects["recchars_daycare"]);
            LevelStudioPlugin.Instance.selectableTextures.AddRange(["recchars_daycareflor", "recchars_daycarewall", "recchars_daycareceil"]);

            EditorInterface.AddDoor<DoorDisplay>("recchars_bookgate", DoorIngameStatus.AlwaysDoor, DaycareDoorAssets.mask, [DaycareDoorAssets.template.shut, DaycareDoorAssets.template.shut]);
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            // In case I add more functional variants
            string[] daycareRoomIds = ["recchars_daycare"];

            EditorInterfaceModes.AddToolsToCategory(mode, "npcs", [
                new ExtRoomNpcTool("recchars_mrdaycare", AssetMan.Get<Sprite>("EditorSpr/Npc_MrDaycare"),
                    daycareRoomIds),
                new ExtRoomNpcTool("recchars_mrdaycare_og", AssetMan.Get<Sprite>("EditorSpr/Npc_MrDaycare_Unnerfed"),
                    "Ed_Tool_npc_recchars_daycare_og_Title", "Ed_Tool_npc_recchars_daycare_og_Desc", daycareRoomIds)
            ]);

            EditorInterfaceModes.AddToolToCategory(mode, "rooms",
                new RoomTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Room_Daycare")));
            EditorInterfaceModes.AddToolsToCategory(mode, "doors", [
                new DoorTool("recchars_bookgate", AssetMan.Get<Sprite>("EditorSpr/Door_BookGate")),
                new WindowTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Window_Daycare"))
            ]);
            EditorInterfaceModes.AddToolToCategory(mode, "lights",
                new LightTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Light_Daycare")));
            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_daycare"),
                new PosterTool("recchars_daycareinfo", AssetMan.Get<Sprite>("EditorSpr/Poster_DaycareInfo")),
                new PosterTool("recchars_daycarerules", AssetMan.Get<Sprite>("EditorSpr/Poster_DaycareRules")),
                new PosterTool("recchars_daycareclock", AssetMan.Get<Sprite>("EditorSpr/Poster_DaycareClock"))
            ]);
        }
    }
}
