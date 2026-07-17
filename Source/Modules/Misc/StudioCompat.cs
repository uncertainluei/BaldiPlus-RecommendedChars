using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;

using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelLoader;
using PlusStudioLevelFormat;

using UncertainLuei.CaudexLib.Registers.ModuleSystem;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    [CaudexModule("Miscellaneous (Editor)"), CaudexModulePriority(-90)]
    public sealed class EditorCompat_Misc : RecCharsEditorSubModule<Module_Misc>
    {
        protected override void Initialized()
        {
            // Load texture assets
            AssetMan.Add("EditorSpr/Poster_bee", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Poster", "bee.png"));
            
            AssetMan.Add("EditorSpr/Room_PartyCafeteria", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Room", "PartyCafeteria.png"));
            AssetMan.Add("EditorSpr/Room_PartyCafeteriaNoNanas", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Room", "PartyCafeteriaNoNanas.png"));

            AssetMan.Add("EditorSpr/Obj_ArtPaintings", AssetLoader.SpritesFromSpritesheet(2, 4, 1f, Vector2.one/2, AssetLoader.TextureFromMod(BasePlugin, "Textures", "Compat", "LevelStudio", "Object", "ArtPaintings.png")));
            AssetMan.Add("EditorSpr/Obj_Cake", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "Cake.png"));
            AssetMan.Add("EditorSpr/Obj_CakeWCandle", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "CakeWCandle.png"));
            AssetMan.Add("EditorSpr/Obj_PartyElevator", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "PartyElevator.png"));
        
            AssetMan.Add("EditorSpr/Obj_InvisibleWall", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "InvisibleWall.png"));
            
            AssetMan.Add("EditorSpr/Obj_SurpriseBaldi", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "SurpriseBaldi.png"));
            AssetMan.Add("EditorSpr/Obj_SurpriseNpc", AssetLoader.SpriteFromMod(BasePlugin, Vector2.one/2, 1f, "Textures", "Compat", "LevelStudio", "Object", "SurpriseNpc.png"));

        }

        [CaudexLoadEvent(LoadingEventOrder.Pre)]
        private static void AddEditorContent()
        {
            TextureContainer cafeTextures = LevelStudioPlugin.Instance.defaultRoomTextures["cafeteria"];
            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_partycafeteria", cafeTextures);
            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_partycafeterianonanas", cafeTextures);
            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_partycafeteriawin", cafeTextures);
            LevelStudioPlugin.Instance.defaultRoomTextures.Add("recchars_baldioffice", LevelStudioPlugin.Instance.defaultRoomTextures["office"]);

            for (int i = 0; i < 8; i++)
                EditorInterface.AddObjectVisual("recchars_painting"+i, LevelLoaderPlugin.Instance.basicObjects["recchars_painting"+i], true);

            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("recchars_cake", LevelLoaderPlugin.Instance.basicObjects["recchars_cake"], 18f, 9f, 0, Vector3.one);
            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("recchars_cakewcandle", LevelLoaderPlugin.Instance.basicObjects["recchars_cakewcandle"], 18f, 9f, 0, Vector3.one);
            EditorInterface.AddObjectVisual("recchars_partyelevator", LevelLoaderPlugin.Instance.basicObjects["recchars_partyelevator"], true);

            EditorInterface.AddObjectVisual("recchars_invisiblewall", LevelLoaderPlugin.Instance.basicObjects["recchars_invisiblewall"], true)
                .GetComponent<MeshRenderer>().enabled = true;

            EditorBasicObject trigger = EditorInterface.AddObjectVisual("recchars_elevatortrigger", LevelLoaderPlugin.Instance.basicObjects["recchars_elevatortrigger"], true);
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = AssetMan.Get<Material>("Mat/ElevatorTrigger");
            cube.transform.parent = trigger.transform;
            cube.transform.localScale = Vector3.one * 5f;

            trigger = EditorInterface.AddObjectVisual("recchars_candletrigger", LevelLoaderPlugin.Instance.basicObjects["recchars_candletrigger"], true);
            cube = GameObject.Instantiate(cube, trigger.transform, false);
            cube.transform.localScale = Vector3.one * 3f;

            EditorBasicObject surpriseBaldi = EditorInterface.AddObjectVisualWithCustomCapsuleCollider("recchars_surprisebaldi", LevelLoaderPlugin.Instance.basicObjects["recchars_surprisebaldi"], 4f, 8f, 0, Vector3.zero);
            GameObject.DestroyImmediate(surpriseBaldi.GetComponentInChildren<Animator>());
            surpriseBaldi.gameObject.SetActive(true);

            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("recchars_surprisenpc", LevelLoaderPlugin.Instance.basicObjects["recchars_surprisenpc"], 4f, 8f, 0, Vector3.zero)
                .gameObject.SetActive(true);

            EditorInterfaceModes.AddModeCallback(AddContentToMode);
        }

        private static void AddContentToMode(EditorMode mode, bool vanillaCompliant)
        {
            EditorInterfaceModes.AddToolToCategory(mode, "posters", 
                new PosterTool("recchars_bee", AssetMan.Get<Sprite>("EditorSpr/Poster_bee")).SetModdedFrame()
            );

            EditorInterfaceModes.AddToolsToCategory(mode, "posters", [
                new PosterTool("recchars_ferriswheel").SetModdedFrame(),
                new PosterTool("recchars_magicedwayne").SetModdedFrame(),
                new PosterTool("recchars_jonjedi").SetModdedFrame(),
                new PosterTool("recchars_thesun").SetModdedFrame(),
                new PosterTool("recchars_meaning").SetModdedFrame(),
                new PosterTool("recchars_andycomic").SetModdedFrame(),
                new PosterTool("recchars_chk_paintings").SetModdedFrame(),
                new PosterTool("recchars_wantedbulletin").SetModdedFrame(),
                new PosterTool("recchars_endofline").SetModdedFrame(),
            ]);

            EditorInterfaceModes.InsertToolsInCategory(mode, "rooms", "room_cafeteria", [
                new RoomTool("recchars_partycafeteria", AssetMan.Get<Sprite>("EditorSpr/Room_PartyCafeteria")).SetModdedFrame(),
                new RoomTool("recchars_partycafeterianonanas", AssetMan.Get<Sprite>("EditorSpr/Room_PartyCafeteriaNoNanas")).SetModdedFrame(),
                //new RoomTool("recchars_partycafeteriawin", AssetMan.Get<Sprite>("EditorSpr/Room_PartyCafeteriaNoNanas")).SetModdedFrame()
            ]);
            EditorInterfaceModes.InsertToolsInCategory(mode, "rooms", "room_office", [
                //new RoomTool("recchars_baldioffice", null).SetModdedFrame(),
            ]);

            EditorInterfaceModes.InsertToolsInCategory(mode, "items", "item_tape", [
                //new ItemTool("recchars_endingtape").SetModdedFrame(),
            ]);

            Sprite[] paintingSprites = AssetMan.Get<Sprite[]>("EditorSpr/Obj_ArtPaintings");
            EditorTool[] paintings = new EditorTool[paintingSprites.Length];
            for (int i = 0; i < paintings.Length; i++)
                paintings[i] = new ObjectToolNoRotation("recchars_painting"+i, paintingSprites[i], 5f).SetModdedFrame();
            EditorInterfaceModes.AddToolsToCategory(mode, "objects", paintings);

            EditorInterfaceModes.AddToolsToCategory(mode, "objects", [
                new ObjectToolNoRotation("recchars_cake", AssetMan.Get<Sprite>("EditorSpr/Obj_Cake")).SetModdedFrame(),
                new ObjectToolNoRotation("recchars_cakewcandle", AssetMan.Get<Sprite>("EditorSpr/Obj_CakeWCandle")).SetModdedFrame(),
                new ObjectTool("recchars_partyelevator", AssetMan.Get<Sprite>("EditorSpr/Obj_PartyElevator")).SetModdedFrame(),
                //new ExtInvisibleWallTool("recchars_invisiblewall", AssetMan.Get<Sprite>("EditorSpr/Obj_InvisibleWall")).SetModdedFrame(),
                //new ObjectTool("recchars_elevatortrigger", null, 5).SetModdedFrame(),
                //new ObjectTool("recchars_candletrigger", null, 35).SetModdedFrame(),
                //new ExtRoomObjectTool("recchars_surprisebaldi", AssetMan.Get<Sprite>("EditorSpr/Obj_SurpriseBaldi"), "recchars_partycafeteriawin").SetModdedFrame(),
                //new ExtRoomObjectTool("recchars_surprisenpc", AssetMan.Get<Sprite>("EditorSpr/Obj_SurpriseNpc"), "recchars_partycafeteriawin").SetModdedFrame()
            ]);
        }

        [CaudexLoadEvent(LoadingEventOrder.Post)]
        private static void AddFallbacksToMissingChars()
        {
            // TBA!!!!
            // TODO: Create stuff for the ending
        }
    }
}
