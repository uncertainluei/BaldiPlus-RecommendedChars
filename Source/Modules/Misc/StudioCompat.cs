using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;

using UnityEngine;

using UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio;
using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using PlusStudioLevelLoader;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using PlusStudioLevelFormat;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public partial class Module_Misc : RecCharsModule
    {
        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Start)]
        private static void InitializeStudioCompat()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Poster_bee", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Poster", "bee.png"));
            
            AssetMan.Add("EditorSpr/Room_PartyCafeteria", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Room", "PartyCafeteria.png"));
            AssetMan.Add("EditorSpr/Room_PartyCafeteriaNoNanas", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Room", "PartyCafeteriaNoNanas.png"));

            AssetMan.Add("EditorSpr/Obj_ArtPaintings", AssetLoader.SpritesFromSpritesheet(2, 4, 1f, Vector2.one/2, AssetLoader.TextureFromMod(BasePlugin, "Textures", "Compat", "LevelStudio", "Object", "ArtPaintings.png")));
            AssetMan.Add("EditorSpr/Obj_Cake", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "Cake.png"));
            AssetMan.Add("EditorSpr/Obj_CakeWCandle", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "CakeWCandle.png"));
            AssetMan.Add("EditorSpr/Obj_PartyElevator", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "PartyElevator.png"));
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            TextureContainer cafeTextures = LevelStudioPlugin.Instance.defaultRoomTextures["cafeteria"];
            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_partycafeteria", cafeTextures);
            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_partycafeterianonanas", cafeTextures);

            for (int i = 0; i < 8; i++)
                EditorInterface.AddObjectVisual("recchars_painting"+i, LevelLoaderPlugin.Instance.basicObjects["recchars_painting"+i], true);

            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("recchars_cake", LevelLoaderPlugin.Instance.basicObjects["recchars_cake"], 18f, 9f, 0, Vector3.one);
            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("recchars_cakewcandle", LevelLoaderPlugin.Instance.basicObjects["recchars_cakewcandle"], 18f, 9f, 0, Vector3.one);
            EditorInterface.AddObjectVisual("recchars_partyelevator", LevelLoaderPlugin.Instance.basicObjects["recchars_partyelevator"], true);
            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "posters", 
                new PosterTool("recchars_bee", AssetMan.Get<Sprite>("EditorSpr/Poster_bee")).SetModdedFrame()
            );
            EditorInterfaceModes.InsertToolsInCategory(mode, "rooms", "room_cafeteria", [
                new RoomTool("recchars_partycafeteria", AssetMan.Get<Sprite>("EditorSpr/Room_PartyCafeteria")).SetModdedFrame(),
                new RoomTool("recchars_partycafeterianonanas", AssetMan.Get<Sprite>("EditorSpr/Room_PartyCafeteriaNoNanas")).SetModdedFrame()
            ]);

            Sprite[] paintingSprites = AssetMan.Get<Sprite[]>("EditorSpr/Obj_ArtPaintings");
            EditorTool[] paintings = new EditorTool[paintingSprites.Length];
            for (int i = 0; i < paintings.Length; i++)
                paintings[i] = new ObjectToolNoRotation("recchars_painting"+i, paintingSprites[0], 5f).SetModdedFrame();
            EditorInterfaceModes.AddToolsToCategory(mode, "objects", paintings);

            EditorInterfaceModes.AddToolsToCategory(mode, "objects", [
                new ObjectToolNoRotation("recchars_cake", AssetMan.Get<Sprite>("EditorSpr/Obj_Cake")).SetModdedFrame(),
                new ObjectToolNoRotation("recchars_cakewcandle", AssetMan.Get<Sprite>("EditorSpr/Obj_CakeWCandle")).SetModdedFrame(),
                new ObjectTool("recchars_partyelevator", AssetMan.Get<Sprite>("EditorSpr/Obj_PartyElevator")).SetModdedFrame(),
            ]);
        }

        [CaudexLoadEventMod(RecommendedCharsPlugin.LevelStudioGuid, LoadingEventOrder.Post)]
        private static void AddFallbacksToMissingChars()
        {
            // TBA!!!!
        }
    }
}
