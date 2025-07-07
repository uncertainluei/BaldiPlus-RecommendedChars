using MTM101BaldAPI;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public static class WeightedHelperExtensions
    {
        public static T Weighted<X, T>(this X selection, int weight) where T : WeightedSelection<X>, new()
        {
            return new T() { selection = selection, weight = weight };
        }
        public static WeightedNPC Weighted(this NPC selection, int weight) => selection.Weighted<NPC, WeightedNPC>(weight);
        public static WeightedItemObject Weighted(this ItemObject selection, int weight) => selection.Weighted<ItemObject, WeightedItemObject>(weight);
        public static WeightedPosterObject Weighted(this PosterObject selection, int weight) => selection.Weighted<PosterObject, WeightedPosterObject>(weight);
        public static WeightedRoomAsset Weighted(this RoomAsset selection, int weight) => selection.Weighted<RoomAsset, WeightedRoomAsset>(weight);
        public static WeightedGameObject Weighted(this GameObject selection, int weight) => selection.Weighted<GameObject, WeightedGameObject>(weight);
        public static WeightedTransform Weighted(this Transform selection, int weight) => selection.Weighted<Transform, WeightedTransform>(weight);

        // Done for future-proofing
        public static void CopyCharacterWeight(this List<WeightedNPC> list, Character characterToCopy, NPC newNpc)
        {
            WeightedNPC weightedToCopy = list.First(x => x.selection?.character == characterToCopy);

            if (weightedToCopy == null)
            {
                RecommendedCharsPlugin.Log.LogWarning($"No character with enum {characterToCopy.ToStringExtended()} exists to copy in the list!");
                return;
            }
            list.Add(newNpc.Weighted(weightedToCopy.weight));
        }
    }

    // This is here to make creating new rooms from scratch easier
    public static class RoomAssetHelper
    {
        public static CellData Cell(int x, int y, int type) => new() { pos = new(x, y), type = type };
        public static List<CellData> CellRect(int width, int height)
        {
            List<CellData> cells = [];
            for (int y = 0; y < height; y++)
            {
                int yOffset = 0;
                if (y == 0)
                    yOffset = 4;
                if (y == height-1)
                    yOffset += 1;

                for (int x = 0; x < width; x++)
                {
                    int type = yOffset;
                    if (x == 0)
                        type += 8;
                    if (x == width-1)
                        type += 2;

                    cells.Add(Cell(x, y, type));
                }
            }
            return cells;
        }
        public static PosterData PosterData(int x, int y, PosterObject pst, Direction dir) => new PosterData() { position = new IntVector2(x, y), poster = pst, direction = dir};
        public static BasicObjectData ObjectPlacement(Component obj, Vector3 pos, Vector3 eulerAngles) => new BasicObjectData() {position = pos, prefab = obj.transform, rotation = Quaternion.Euler(eulerAngles)};
        public static BasicObjectData ObjectPlacement(Component obj, Vector3 pos, float angle) => ObjectPlacement(obj, pos, Vector3.up * angle);
    }

    public class RoomBlueprint(string name, RoomCategory cat)
    {
        public RoomBlueprint(string name, string cat) : this(name, EnumExtensions.ExtendEnum<RoomCategory>(cat))
        {
        }

        public string name = name;
        public RoomCategory category = cat;

        public RoomType type = RoomType.Room;
        public Color color = Color.white;

        public Texture2D texFloor;
        public Texture2D texCeil;
        public Texture2D texWall;
        public bool keepTextures;

        public Transform lightObj;
        public Material mapMaterial;
        public StandardDoorMats doorMats;

        public float posterChance = 0.25f;
        public List<WeightedPosterObject> posters = [];

        public float windowChance;
        public WindowObject windowSet;

        public int itemValMin;
        public int itemValMax = 100;

        public bool offLimits;
        public bool holdsActivity;

        public RoomFunctionContainer functionContainer;

        public RoomAsset CreateAsset(string idName)
        {
            RoomAsset roomAsset = RoomAsset.CreateInstance<RoomAsset>();
            ((ScriptableObject)roomAsset).name = $"{type}_{name}_{idName}";
            roomAsset.name = $"{name}_{idName}";

            roomAsset.type = type;
            roomAsset.category = category;
            roomAsset.color = color;

            roomAsset.florTex = texFloor;
            roomAsset.ceilTex = texCeil;
            roomAsset.wallTex = texWall;
            roomAsset.keepTextures = keepTextures;

            roomAsset.lightPre = lightObj;
            roomAsset.mapMaterial = mapMaterial;
            roomAsset.doorMats = doorMats;

            roomAsset.posterChance = posterChance;
            roomAsset.posters = posters;

            roomAsset.windowChance = windowChance;
            roomAsset.windowObject = windowSet;

            roomAsset.minItemValue = itemValMin;
            roomAsset.maxItemValue = itemValMax;

            roomAsset.offLimits = offLimits;
            roomAsset.hasActivity = holdsActivity;

            roomAsset.roomFunctionContainer = functionContainer;

            return roomAsset;
        }
    }
}
