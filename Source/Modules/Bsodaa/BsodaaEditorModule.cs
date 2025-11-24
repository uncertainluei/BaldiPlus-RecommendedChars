using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Util;
using System.Linq;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat
{
    [CaudexModule("Eveyone's Bsodaa (Editor)")]
    public sealed class EditorCompat_Bsodaa : RecCharsEditorSubModule<Module_Bsodaa>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AddTexturesToAssetMan("EditorTex/Bsodaa/", ["Textures", "Editor", "Bsodaa"]);
            
            AssetMan.Add("EditorSpr/Npc_Bsodaa", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Bsodaa/npc_bsodaa"), 1f));

            AssetMan.Add("EditorSpr/Npc_BsodaaHelper", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Bsodaa/npc_bsodaahelper"), 1f));
            AssetMan.Add("EditorSpr/Npc_BsodaaHelper_Diet", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Bsodaa/npc_bsodaahelper_diet"), 1f));

            AssetMan.Add("EditorSpr/Room_BsodaaRoom", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Bsodaa/room_bsodaaroom"), 1f));
            AssetMan.Add("EditorSpr/Light_Bsodaa", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Bsodaa/light_bsodaa"), 1f));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "Bsodaa.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_bsodaa", ObjMan.Get<EveyBsodaa>("Npc_Bsodaa"));
            EditorBasicObject helperVisual = EditorInterface.AddObjectVisual("recchars_bsodaahelper", ObjMan.Get<BsodaaHelper>("Npc_BsodaaHelper").gameObject, true);
            LevelStudioPlugin.Instance.basicObjectDisplays.Add("recchars_bsodaahelper_diet", helperVisual);

            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_bsodaaroom", new("recchars_bsodaaflor", "recchars_bsodaawall", "recchars_bsodaaceil"));
            LevelStudioPlugin.Instance.selectableTextures.AddRange(["recchars_bsodaaflor", "recchars_bsodaawall", "recchars_bsodaaceil"]);

            LevelStudioPlugin.Instance.selectableShopItems.AddRange(["recchars_smallbsoda", "recchars_smalldietbsoda"]);
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_bsodaa", AssetMan.Get<Sprite>("EditorSpr/Npc_Bsodaa")));
            EditorInterfaceModes.AddToolToCategory(mode, mode.id == "rooms" ? "objects" : "npcs",
                new BsodaaHelperObjTool("recchars_bsodaahelper", AssetMan.Get<Sprite>("EditorSpr/Npc_BsodaaHelper"), "recchars_bsodaaroom"));
            EditorInterfaceModes.AddToolToCategory(mode, "npcs", 
                new BsodaaHelperObjTool("recchars_bsodaahelper_diet", AssetMan.Get<Sprite>("EditorSpr/Npc_BsodaaHelper_Diet"), "recchars_bsodaaroom"));

            EditorInterfaceModes.AddToolsToCategory(mode, "items", [
                new ItemTool("recchars_smallbsoda"),
                new ItemTool("recchars_smalldietbsoda")
            ]);

            EditorInterfaceModes.AddToolToCategory(mode, "rooms",
                new RoomTool("recchars_bsodaaroom", AssetMan.Get<Sprite>("EditorSpr/Room_BsodaaRoom")));
            EditorInterfaceModes.AddToolToCategory(mode, "lights",
                new LightTool("recchars_bsodaa", AssetMan.Get<Sprite>("EditorSpr/Light_Bsodaa")));

            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_bsodaa"),
                new PosterTool("recchars_pri_bsodaahelper")
            ]);
        }

        private class BsodaaHelperObjTool(string id, Sprite spr, params string[] rooms) : ObjectToolNoRotation(id, spr, 5f)
        {
            private string[] allowedRoomIds = rooms;

            public override bool ValidLocation(IntVector2 pos)
            {
                if (!base.ValidLocation(pos)) return false;
                return allowedRoomIds.Contains(EditorController.Instance.levelData.RoomFromPos(pos, forEditor: true).roomType);
            }
        }
    }
}
