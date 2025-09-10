using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Util;
using PlusStudioLevelLoader;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat
{
    [CaudexModule("Mr. Daycare (Editor)")]
    public sealed class EditorCompat_MrDaycare : RecCharsEditorSubModule<Module_MrDaycare>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AssetMan.AddRange(AssetLoader.TexturesFromMod(BasePlugin, "*.png", "Textures", "Editor", "Daycare"), x => "EditorTex/Daycare/" + x.name);
            
            AssetMan.Add("EditorSpr/Npc_MrDaycare", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Daycare/npc_mrdaycare"), 1f));
            AssetMan.Add("EditorSpr/Npc_MrDaycare_Og", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Daycare/npc_mrdaycare_og"), 1f));

            AssetMan.Add("EditorSpr/Room_Daycare", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Daycare/room_daycare"), 1f));
            AssetMan.Add("EditorSpr/Window_Daycare", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Daycare/window_daycare"), 1f));
            AssetMan.Add("EditorSpr/Light_Daycare", AssetLoader.SpriteFromTexture2D(AssetMan.Get<Texture2D>("EditorTex/Daycare/light_daycare"), 1f));

            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Editor", "MrDaycare.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_mrdaycare", ObjMan.Get<MrDaycare>("Npc_MrDaycare_Nerfed"));
            LevelStudioPlugin.Instance.npcDisplays.Add("recchars_mrdaycare_og", LevelStudioPlugin.Instance.npcDisplays["recchars_mrdaycare"]);

            LevelStudioCompatHelper.AddRoomDefaultTextures("recchars_daycare", "recchars_daycareflor", "recchars_daycarewall", "recchars_daycareceil");
            EditorInterface.AddWindow("recchars_daycare", LevelLoaderPlugin.Instance.windowObjects["recchars_daycare"]);
            LevelStudioPlugin.Instance.selectableTextures.AddRange(["recchars_daycareflor", "recchars_daycarewall", "recchars_daycareceil"]);

            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            // In case I add more functional variants
            string[] daycareRoomIds = ["recchars_daycare"];

            EditorInterfaceModes.AddToolsToCategory(mode, "npcs", [
                new ExtRoomNpcTool("recchars_mrdaycare", AssetMan.Get<Sprite>("EditorSpr/Npc_MrDaycare"),
                    daycareRoomIds),
                new ExtRoomNpcTool("recchars_mrdaycare_og", AssetMan.Get<Sprite>("EditorSpr/Npc_MrDaycare_Og"),
                    "Ed_Tool_npc_recchars_daycare_og_Title", "Ed_Tool_npc_recchars_daycare_og_Desc", daycareRoomIds)
            ]);
            EditorInterfaceModes.AddToolsToCategory(mode, "items", [
                new ItemTool("recchars_pie"),
                new ItemTool("recchars_doorkey"),
            ]);

            EditorInterfaceModes.AddToolToCategory(mode, "rooms",
                new RoomTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Room_Daycare")));
            EditorInterfaceModes.AddToolToCategory(mode, "doors",
                new WindowTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Window_Daycare")));
            EditorInterfaceModes.AddToolToCategory(mode, "lights",
                new LightTool("recchars_daycare", AssetMan.Get<Sprite>("EditorSpr/Light_Daycare")));
            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_daycare"),
                new PosterTool("recchars_daycareinfo"),
                new PosterTool("recchars_daycarerules"),
                new PosterTool("recchars_daycareclock")
            ]);
        }
    }
}
