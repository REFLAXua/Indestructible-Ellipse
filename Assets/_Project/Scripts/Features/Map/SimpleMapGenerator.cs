using System.Collections.Generic;
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
                if (TileComparer.CanAppendTile(spawnedTiles[x - 1, y], tile, Direction.Left) &&
                    TileComparer.CanAppendTile(spawnedTiles[x + 1, y], tile, Direction.Right) &&
                    TileComparer.CanAppendTile(spawnedTiles[x, y - 1], tile, Direction.Back) &&
                    TileComparer.CanAppendTile(spawnedTiles[x, y + 1], tile, Direction.Forward))
                {
                    candidates.Add(tile);
                }
            }

            if (candidates.Count == 0) return null;

            return TileComparer.GetRandomTile(candidates);
        }
    }
}
