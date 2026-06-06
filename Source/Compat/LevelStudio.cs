using BepInEx;
using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;

using System.Linq;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    internal static class LevelStudioCompatHelper
    {
        private static Sprite _frameSprite;
        internal static Sprite FrameSprite
        {
            get
            {
                if (!_frameSprite)
                    _frameSprite = MTM101BaldAPI.AssetTools.AssetLoader.SpriteFromMod(RecommendedCharsPlugin.Plugin, Vector2.one / 2f, 1f,
                        "Textures", "Compat", "LevelStudio", "SlotRcPack.png");
                return _frameSprite;
            }
        }

        internal static T SetModdedFrame<T>(this T tool) where T : EditorTool
        {
            tool.frameOverride = FrameSprite;
            return tool;
        }
    }

    internal class ExtItemTool(string id, Sprite spr, string descKey, bool useItemName = true) : ItemTool(id, spr, useItemName)
    {
        protected string newDescKey = descKey;
        public override string descKey => newDescKey;
    }

    internal class ExtNpcTool(string id, Sprite spr) : NPCTool(id, spr)
    {
        protected string newTitleKey;
        protected string newDescKey;

        public override string titleKey => newTitleKey.IsNullOrWhiteSpace() ? base.titleKey : newTitleKey;
        public override string descKey => newDescKey.IsNullOrWhiteSpace() ? base.descKey : newDescKey;

        public ExtNpcTool(string id, Sprite spr, string desc) : this(id, spr)
            => newDescKey = desc;
        public ExtNpcTool(string id, Sprite spr, string title, string desc) : this(id, spr, desc)
            => newTitleKey = title;
    }

    internal class ExtRoomNpcTool(string id, Sprite spr, params string[] rooms) : ExtNpcTool(id, spr)
    {
        public ExtRoomNpcTool(string id, Sprite spr, string desc, string[] rooms) : this(id, spr, rooms)
            => newDescKey = desc;
        public ExtRoomNpcTool(string id, Sprite spr, string title, string desc, string[] rooms) : this(id, spr, desc, rooms)
            => newTitleKey = title;

        private readonly string[] allowedRoomIds = rooms;

        public override bool ValidLocation(IntVector2 pos)
        {
            if (!base.ValidLocation(pos)) return false;
            return allowedRoomIds.Contains(EditorController.Instance.levelData.RoomFromPos(pos, forEditor: true).roomType);
        }
    }
    
    internal class ExtRoomObjectTool(string id, Sprite spr, params string[] rooms) : ObjectToolNoRotation(id, spr, 5f)
    {
        private string[] allowedRoomIds = rooms;

        public override bool ValidLocation(IntVector2 pos)
        {
            if (!base.ValidLocation(pos)) return false;
            return allowedRoomIds.Contains(EditorController.Instance.levelData.RoomFromPos(pos, forEditor: true).roomType);
        }
    }

    internal class ExtInvisibleWallTool(string id, Sprite spr) : ObjectTool(id, spr, 5f)
    {
        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            EditorController.Instance.AddUndo();
            BasicObjectLocation local = new();
            local.prefab = type;
            local.position = position.ToWorld();
            local.position += (Vector3.up+dir.ToVector3()) * verticalOffset;
            local.rotation = dir.GetOpposite().ToRotation();
            EditorController.Instance.levelData.objects.Add(local);
            EditorController.Instance.AddVisual(local);
            SoundPlayOneshot("Slap");
            return true;
        }
    }
}
