using System.Collections.Generic;
using UnityEngine;

namespace Features.Map
{
    public struct TilePlacementData
    {
        public Vector2Int GridPosition;
        public VoxelTile TilePrefab;
        public Quaternion Rotation;
    }

    public interface IMapGenerator
    {
        Queue<TilePlacementData> Generate(Vector2Int mapSize, List<VoxelTile> availableTiles);
    }
}
