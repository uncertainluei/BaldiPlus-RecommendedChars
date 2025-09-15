using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat
{
    [CaudexModule("Swapped Duo (Editor)")]
    public sealed class EditorCompat_SwappedDuo : RecCharsEditorSubModule<Module_SwappedDuo>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Editor", "SwappedDuo"), x => "EditorTex/SwappedDuo/" + x.name);
            
            AssetMan.Add("EditorSpr/Npc_GottaBully", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/SwappedDuo/npc_gottabully"), 1f));
            AssetMan.Add("EditorSpr/Npc_ArtsWithWires", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/SwappedDuo/npc_artswithwires"), 1f));
            AssetMan.Add("EditorSpr/Room_SwapCloset", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/SwappedDuo/room_swapcloset"), 1f));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "SwappedDuo.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_gottabully", ObjMan.Get<GottaBully>("Npc_GottaBully"));
            EditorInterface.AddNPCVisual("recchars_artswithwires", ObjMan.Get<ArtsWithWires>("Npc_ArtsWithWires"));
            LevelStudioCompatHelper.AddRoomDefaultTextures("recchars_swapcloset", "recchars_swapflor", "recchars_swapwall", "BlueCarpet");
            LevelStudioPlugin.Instance.selectableTextures.AddRange(["recchars_swapflor", "recchars_swapwall"]);

            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolsToCategory(mode, "npcs", [
                new NPCTool("recchars_gottabully", AssetMan.Get<Sprite>("EditorSpr/Npc_GottaBully")),
                new NPCTool("recchars_artswithwires", AssetMan.Get<Sprite>("EditorSpr/Npc_ArtsWithWires"))
            ]);

            EditorInterfaceModes.AddToolToCategory(mode, "rooms",
            new RoomTool("recchars_swapcloset", AssetMan.Get<Sprite>("EditorSpr/Room_SwapCloset")));

            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_gbully"),
                new PosterTool("recchars_pri_wires"),
                new PosterTool("recchars_sub2tapliasmy")
            ]);
        }
    }
}
