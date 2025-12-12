using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_GottaBully : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture asset
            AssetMan.Add("EditorSpr/Npc_GottaBully", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "GottaBully.png"));
            AssetMan.Add("EditorSpr/Room_SwapCloset", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Room", "SwapCloset.png"));
            
            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "SwappedDuo.json5");
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_gottabully", ObjMan.Get<GottaBully>("Npc_GottaBully"));
            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_swapcloset", new("recchars_swapflor", "recchars_swapwall", "BlueCarpet"));
            LevelStudioPlugin.Instance.selectableTextures.AddRange(["recchars_swapflor", "recchars_swapwall"]);

            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_gottabully", AssetMan.Get<Sprite>("EditorSpr/Npc_GottaBully")));

            EditorInterfaceModes.AddToolToCategory(mode, "rooms",
                new RoomTool("recchars_swapcloset", AssetMan.Get<Sprite>("EditorSpr/Room_SwapCloset")));

            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_gbully"),
                new PosterTool("recchars_sub2tapliasmy")
            ]);
        }
    }
}
