using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features.Map
{
    public class SimpleMapGenerator : IMapGenerator
    {
        public Queue<TilePlacementData> Generate(Vector2Int mapSize, List<VoxelTile> availableTiles)
        {
            var queue = new Queue<TilePlacementData>();
            var spawnedTiles = new VoxelTile[mapSize.x, mapSize.y];

            for (int x = 1; x < mapSize.x - 1; x++)
            {
                for (int y = 1; y < mapSize.y - 1; y++)
                {
                    VoxelTile selectedTile = SelectTile(x, y, spawnedTiles, availableTiles);
                    
                    if (selectedTile != null)
                    {
                        spawnedTiles[x, y] = selectedTile;
                        queue.Enqueue(new TilePlacementData
                        {
                            GridPosition = new Vector2Int(x, y),
                            TilePrefab = selectedTile,
                            Rotation = selectedTile.transform.rotation
                        });
                    }
                }
            }

            return queue;
        }

        private VoxelTile SelectTile(int x, int y, VoxelTile[,] spawnedTiles, List<VoxelTile> availableTiles)
        {
            List<VoxelTile> candidates = new List<VoxelTile>();

            foreach (VoxelTile tile in availableTiles)
            {
                if (CanAppendTile(spawnedTiles[x - 1, y], tile, Direction.Left) &&
                    CanAppendTile(spawnedTiles[x + 1, y], tile, Direction.Right) &&
                    CanAppendTile(spawnedTiles[x, y - 1], tile, Direction.Back) &&
                    CanAppendTile(spawnedTiles[x, y + 1], tile, Direction.Forward))
                {
                    candidates.Add(tile);
                }
            }

            if (candidates.Count == 0) return null;

            return GetRandomTile(candidates);
        }

        private VoxelTile GetRandomTile(List<VoxelTile> candidates)
        {
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

            return candidates.Last();
        }

        private bool CanAppendTile(VoxelTile existingTile, VoxelTile tileToAppend, Direction direction)
        {
            if (existingTile == null) return true;

            switch (direction)
            {
                case Direction.Right:
                    return Enumerable.SequenceEqual(existingTile.ColorsRight, tileToAppend.ColorsLeft);
                case Direction.Left:
                    return Enumerable.SequenceEqual(existingTile.ColorsLeft, tileToAppend.ColorsRight);
                case Direction.Forward:
                    return Enumerable.SequenceEqual(existingTile.ColorsForward, tileToAppend.ColorsBack);
                case Direction.Back:
                    return Enumerable.SequenceEqual(existingTile.ColorsBack, tileToAppend.ColorsForward);
                default:
                    return false;
            }
        }
    }
}
