using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features.Map
{
    public static class TileComparer
    {
        public static bool CanAppendTile(VoxelTile existingTile, VoxelTile tileToAppend, Direction direction)
        {
            if (existingTile == null || tileToAppend == null) return true;

            return direction switch
            {
                Direction.Right => Enumerable.SequenceEqual(existingTile.ColorsRight, tileToAppend.ColorsLeft),
                Direction.Left => Enumerable.SequenceEqual(existingTile.ColorsLeft, tileToAppend.ColorsRight),
                Direction.Forward => Enumerable.SequenceEqual(existingTile.ColorsForward, tileToAppend.ColorsBack),
                Direction.Back => Enumerable.SequenceEqual(existingTile.ColorsBack, tileToAppend.ColorsForward),
                _ => false
            };
        }

        public static VoxelTile GetRandomTile(List<VoxelTile> candidates)
        {
            if (candidates == null || candidates.Count == 0) return null;

            float totalWeight = candidates.Sum(t => t.Weight);
            float randomValue = Random.Range(0, totalWeight);
            float currentSum = 0;

            foreach (var tile in candidates)
            {
                currentSum += tile.Weight;
                if (randomValue < currentSum)
                {
                    return tile;
                }
            }

            return candidates[candidates.Count - 1];
        }
    }
}
