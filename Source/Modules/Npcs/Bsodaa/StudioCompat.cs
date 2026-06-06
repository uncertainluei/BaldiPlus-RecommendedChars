using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Eveyone's Bsodaa (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_Bsodaa : RecCharsEditorSubModule<Module_Bsodaa>
    {
        protected override void Initialized()
        {
            // Load icon assets            
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_Bsodaa", ["Textures", "Compat", "LevelStudio", "Npc", "Bsodaa.png"]);
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_BsodaaHelper", ["Textures", "Compat", "LevelStudio", "Npc", "BsodaaHelper.png"]);
            ObjectCreation.AddSpriteToAssetManWLegacy("EditorSpr/Npc_BsodaaHelper_Diet", ["Textures", "Compat", "LevelStudio", "Npc", "BsodaaHelper_Diet.png"]);

            AssetMan.Add("EditorSpr/Room_BsodaaRoom", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Room", "BsodaaRoom.png"));
            AssetMan.Add("EditorSpr/Light_Bsodaa", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Light", "Bsodaa.png"));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Compat", "LevelStudio", "Bsodaa.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_bsodaa", ObjMan.Get<EveyBsodaa>("Npc/Bsodaa"));
            EditorBasicObject helperVisual = EditorInterface.AddObjectVisual("recchars_bsodaahelper", ObjMan.Get<BsodaaHelper>("Npc/BsodaaHelper").gameObject, true);
            LevelStudioPlugin.Instance.basicObjectDisplays.Add("recchars_bsodaahelper_diet", helperVisual);

            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_bsodaaroom", new("recchars_bsodaaflor", "recchars_bsodaawall", "recchars_bsodaaceil"));
            LevelStudioPlugin.Instance.selectableTextures.AddRange(["recchars_bsodaaflor", "recchars_bsodaawall", "recchars_bsodaaceil"]);

            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "npcs",
                new NPCTool("recchars_bsodaa", AssetMan.Get<Sprite>("EditorSpr/Npc_Bsodaa")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, mode.id == "rooms" ? "objects" : "npcs",
                new ExtRoomObjectTool("recchars_bsodaahelper", AssetMan.Get<Sprite>("EditorSpr/Npc_BsodaaHelper"), "recchars_bsodaaroom").SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "npcs", 
                new ExtRoomObjectTool("recchars_bsodaahelper_diet", AssetMan.Get<Sprite>("EditorSpr/Npc_BsodaaHelper_Diet"), "recchars_bsodaaroom").SetModdedFrame());

            EditorInterfaceModes.AddToolToCategory(mode, "rooms",
                new RoomTool("recchars_bsodaaroom", AssetMan.Get<Sprite>("EditorSpr/Room_BsodaaRoom")).SetModdedFrame());
            EditorInterfaceModes.AddToolToCategory(mode, "lights",
                new LightTool("recchars_bsodaa", AssetMan.Get<Sprite>("EditorSpr/Light_Bsodaa")).SetModdedFrame());

            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_bsodaa").SetModdedFrame(),
                new PosterTool("recchars_pri_bsodaahelper").SetModdedFrame()
            ]);
        }
    }
}