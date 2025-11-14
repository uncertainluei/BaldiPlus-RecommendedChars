using BepInEx;

using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelFormat;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelStudio
{
    internal static class LevelStudioCompatHelper
    {
        /*internal static void AddRoomDefaultTextures(string id, string florTex, string wallTex, string ceilTex)
        {
            EditorLevelData.AddDefaultTextureAction((Dictionary<string, TextureContainer> texs) =>
                texs.Add(id, new(florTex, wallTex, ceilTex)));
        }*/
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
    
}
