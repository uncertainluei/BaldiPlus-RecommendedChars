using MTM101BaldAPI.AssetTools;
using BaldiLevelEditor;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LegacyEditor
{
    internal class ExtendedNpcTool(string prefab, string spritePath) : NpcTool(prefab)
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }

    internal class ExtendedItemTool(string prefab, string spritePath) : ItemTool(prefab)
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }

    internal class ExtendedFloorTool(string prefab, string spritePath) : FloorTool(prefab)
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }

    internal class ExtendedObjectTool(string prefab, string spritePath) : ObjectTool(prefab)
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }

    internal class ExtendedRotatableTool(string prefab, string spritePath) : RotateAndPlacePrefab(prefab)
    {
        private readonly Sprite sprite = AssetLoader.SpriteFromTexture2D(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(spritePath), 40);
        public override Sprite editorSprite => sprite;
    }
}
