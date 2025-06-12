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

    // This is here to 
    public static class RoomAssetHelper
    {
        public static CellData Cell(int x, int y, int type) => new CellData() { pos = new IntVector2(x, y), type = type };
        public static List<CellData> CellRect(int width, int height)
        {
            List<CellData> cells = new List<CellData>();
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
}
