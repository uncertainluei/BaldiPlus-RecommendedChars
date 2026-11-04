using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor.Tools;
using PlusLevelStudio.Editor;

using UnityEngine;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Gotta Bully (Editor)"), CaudexModulePriority(-100)]
    public sealed class EditorCompat_GottaBully : RecCharsEditorSubModule<Module_GottaBully>
    {
        protected override void Initialized()
        {
            // Load icon assets
            AssetMan.Add("EditorSpr/Npc_GottaBully", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Npc", "GottaBully.png"));
            AssetMan.Add("EditorSpr/Room_SwapCloset", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Room", "SwapCloset.png"));
            
            // Load localization
            CaudexAssetLoader.LocalizationFromMod(Language.English, BasePlugin, "Lang", "English", "Compat", "LevelStudio", "GottaBully.json5");
        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            EditorInterface.AddNPCVisual("recchars_gottabully", ObjMan.Get<GottaBully>("Npc/GottaBully"));
            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_swapcloset", new("recchars_swapflor", "recchars_swapwall", "BlueCarpet"));
            LevelStudioPlugin.Instance.selectableTextures.AddRange(["recchars_swapflor", "recchars_swapwall"]);
            LevelStudioPlugin.Instance.npcPropertyTypes.Add("recchars_gottabully", new()
            {
                npcPropertiesType = typeof(GottaBullyProperties),
                pageType = typeof(GottaSweepPropExchangeHandler),
                pagePath = LevelStudioPlugin.Instance.npcPropertyTypes["sweep"].pagePath
            });
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.InsertToolInCategory(mode, "npcs", "npc_sweep",
                new NPCTool("recchars_gottabully", AssetMan.Get<Sprite>("EditorSpr/Npc_GottaBully")).SetModdedFrame());

            EditorInterfaceModes.InsertToolInCategory(mode, "rooms", "room_closet",
                new RoomTool("recchars_swapcloset", AssetMan.Get<Sprite>("EditorSpr/Room_SwapCloset")).SetModdedFrame());

            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_pri_gbully").SetModdedFrame(),
                new PosterTool("recchars_sub2tapliasmy").SetModdedFrame()
            ]);
        }

        internal class GottaBullyProperties : GottaSweepProperties
        {
            public GottaBullyProperties() : base()
            {
                minDelay = 150f;
                maxDelay = 250f;
            }

            public override GameObject[] GeneratePrefabs(NPC baseNpc)
            {
                GameObject[] prefabs = base.GeneratePrefabs(baseNpc);
                prefabs[0].name = "GottaBully_Customized";
                return prefabs;
            }

            public override bool IsAtDefaultSettings()
            =>
                minDelay == 150f &&
                maxDelay == 250f &&
                minActive == 45f &&
                maxActive == 75f;
        }
    }
}
