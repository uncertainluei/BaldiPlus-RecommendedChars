using HarmonyLib;

using MonoMod.Utils;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;

using BaldiLevelEditor;
using PlusLevelFormat;
using PlusLevelLoader;

using System.Collections.Generic;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LegacyEditor
{
    [ConditionalPatchMod(RecommendedCharsPlugin.LegacyEditorGuid)]
    [HarmonyPatch]
    internal static class LegacyEditorCompatHelper
    {
        // Modified version of the Editor's strip object method as it does not immediately destroy components
        internal static GameObject GetStrippedObject(Component obj, bool stripColliders = false)
        {
            GameObject gameObj = obj.gameObject;
            GameObject newObj = UnityEngine.Object.Instantiate(gameObj,MTM101BaldiDevAPI.prefabTransform);
            newObj.GetComponentsInChildren<MonoBehaviour>().Do(delegate (MonoBehaviour x)
            {
                if (x is not BillboardUpdater)
                    GameObject.DestroyImmediate(x);
            });
            if (stripColliders)
            {
                newObj.GetComponentsInChildren<Collider>().Do(x =>
                    GameObject.DestroyImmediate(x));
            }
            newObj.name = "REFERENCE_" + newObj.name;
            return newObj;
        }

        internal static void AddCharacterObject(string id, Component obj) => BaldiLevelEditorPlugin.characterObjects.Add(id, GetStrippedObject(obj, true));
        internal static void AddObject(string id, Component obj, Vector3 offset = default) => BaldiLevelEditorPlugin.editorObjects.Add(EditorObjectType.CreateFromGameObject<EditorPrefab, PrefabLocation>(id, obj.gameObject, offset));

        internal static void AddToEditor<T>(this T tool, string cat) where T : EditorTool, IExtendedEditorTool
        {
            if (!toolsToInit.ContainsKey(cat))
                toolsToInit.Add(cat, []);
            toolsToInit[cat].Add(tool);
        }
        internal static void AddRoomDefaultTextures(string id, string florTex, string wallTex, string ceilTex)
        {
            roomsToInit.Add(id, new(florTex, wallTex, ceilTex));
        }

        private static readonly Dictionary<string, TextureContainer> roomsToInit = [];
        private static readonly Dictionary<string, List<EditorTool>> toolsToInit = [];

        [HarmonyPatch(typeof(EditorLevel), "InitializeDefaultTextures"), HarmonyPostfix]
        private static void InitializeRoomTextures(EditorLevel __instance)
        {
            __instance.defaultTextures.AddRange(roomsToInit);
        }

        [HarmonyPatch(typeof(PlusLevelEditor), "Initialize"), HarmonyPostfix]
        private static void AddEditorTools(PlusLevelEditor __instance)
        {
            foreach (ToolCategory cat in __instance.toolCats)
            {
                if (!toolsToInit.ContainsKey(cat.name)) continue;

                List<EditorTool> catToAdd = toolsToInit[cat.name];
                foreach (EditorTool toolToAdd in catToAdd)
                    cat.tools.Add(toolToAdd);
            }
        }
    }

    internal interface IExtendedEditorTool
    {}

    internal class ExtendedNpcTool(string prefab, string spritePath) : NpcTool(prefab), IExtendedEditorTool
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }

    internal class ExtendedRoomNpcTool(string prefab, string spritePath, string type) : ExtendedNpcTool(prefab, spritePath)
    {
        private readonly string type = type;

        public override void OnDrop(IntVector2 pos)
        {
            if (IsOutOfBounds(pos)) return;

            TiledArea area = PlusLevelEditor.Instance.level.GetAreaOfPos(pos.ToByte());
            if (area == null || area.roomId == 0) return;
            if (PlusLevelEditor.Instance.level.rooms[area.roomId - 1].type != type) return;

            base.OnDrop(pos);
        }
    }

    internal class ExtendedItemTool(string prefab, string spritePath) : ItemTool(prefab), IExtendedEditorTool
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }

    internal class ExtendedFloorTool(string prefab, string spritePath) : FloorTool(prefab), IExtendedEditorTool
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }

    internal class ExtendedObjectTool(string prefab, string spritePath) : ObjectTool(prefab), IExtendedEditorTool
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }

    internal class ExtendedRoomObjTool(string prefab, string spritePath, string type) : ExtendedObjectTool(prefab, spritePath)
    {
        private readonly string type = type;

        public override void OnDrop(IntVector2 pos)
        {
            if (IsOutOfBounds(pos)) return;

            TiledArea area = PlusLevelEditor.Instance.level.GetAreaOfPos(pos.ToByte());
            if (area == null || area.roomId == 0) return;
            if (PlusLevelEditor.Instance.level.rooms[area.roomId-1].type != type) return;

            base.OnDrop(pos);
        }
    }

    internal class ExtendedRotatableTool(string prefab, string spritePath) : RotateAndPlacePrefab(prefab), IExtendedEditorTool
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }
}
