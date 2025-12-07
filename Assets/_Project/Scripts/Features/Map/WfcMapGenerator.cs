using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Features.Map
{
    public class WfcMapGenerator : IMapGenerator
    {
        private List<VoxelTile>[,] _possibleTiles;
        private Queue<Vector2Int> _recalcPossibleTilesQueue = new Queue<Vector2Int>();
        private Vector2Int _mapSize;
        private List<VoxelTile> _processedTiles;

        private const int MaxGenerationAttempts = 10;
        private const int MaxInnerIterations = 500;
        private const float TileSpacing = 20f;

        public Queue<TilePlacementData> Generate(Vector2Int mapSize, List<VoxelTile> availableTiles)
        {
            _mapSize = mapSize;
            _processedTiles = PrepareTiles(availableTiles);

            bool success = TryGenerate();

            if (!success)
            {
                Debug.LogError("[WfcMapGenerator] Failed to generate map after max attempts.");
                return new Queue<TilePlacementData>();
            }

            return BuildPlacementQueue();
        }

        private List<VoxelTile> PrepareTiles(List<VoxelTile> availableTiles)
        {
            var workingList = new List<VoxelTile>();

            GameObject tempRoot = new GameObject("WFC_Tile_Variants");
            tempRoot.transform.position = new Vector3(0, -5000, 0);

            int tileIndex = 0;

            foreach (var original in availableTiles)
            {
                VoxelTile clone = Object.Instantiate(original, tempRoot.transform);
                clone.transform.localPosition = new Vector3(tileIndex * TileSpacing, 0, 0);
                tileIndex++;

                Physics.SyncTransforms();
                clone.CalculateSidesColors();
                workingList.Add(clone);
            }

            ProcessRotations(workingList, tempRoot.transform, ref tileIndex);

            tempRoot.SetActive(false);

            return workingList;
        }

        private void ProcessRotations(List<VoxelTile> workingList, Transform holder, ref int tileIndex)
        {
            int countBeforeAdding = workingList.Count;

            for (int i = 0; i < countBeforeAdding; i++)
            {
                VoxelTile tile = workingList[i];

                switch (tile.Rotation)
                {
                    case VoxelTile.RotationType.OnlyRotation:
                        break;

                    case VoxelTile.RotationType.TwoRotations:
                        tile.Weight /= 2;
                        if (tile.Weight <= 0) tile.Weight = 1;

                        workingList.Add(CreateRotatedClone(tile, holder, ref tileIndex, 1));
                        break;

                    case VoxelTile.RotationType.FourRotations:
                        tile.Weight /= 4;
                        if (tile.Weight <= 0) tile.Weight = 1;

                        workingList.Add(CreateRotatedClone(tile, holder, ref tileIndex, 1));
                        workingList.Add(CreateRotatedClone(tile, holder, ref tileIndex, 2));
                        workingList.Add(CreateRotatedClone(tile, holder, ref tileIndex, 3));
                        break;
                }
            }
        }

        private VoxelTile CreateRotatedClone(VoxelTile original, Transform holder, ref int tileIndex, int rotations)
        {
            VoxelTile clone = Object.Instantiate(original, holder);
            clone.transform.localPosition = new Vector3(tileIndex * TileSpacing, 0, 0);
            tileIndex++;

            for (int r = 0; r < rotations; r++)
            {
                clone.Rotate90();
            }

            return clone;
        }

        private bool TryGenerate()
        {
            _possibleTiles = new List<VoxelTile>[_mapSize.x, _mapSize.y];

            for (int attempt = 0; attempt < MaxGenerationAttempts; attempt++)
            {
                InitializePossibleTiles();

                VoxelTile tileInCenter = TileComparer.GetRandomTile(_processedTiles);
                int centerX = _mapSize.x / 2;
                int centerY = _mapSize.y / 2;
                _possibleTiles[centerX, centerY] = new List<VoxelTile> { tileInCenter };

                _recalcPossibleTilesQueue.Clear();
                EnqueueNeighboursToRecalc(new Vector2Int(centerX, centerY));

                if (GenerateAllPossibleTiles())
                {
                    return true;
                }
            }

            return false;
        }

        private void InitializePossibleTiles()
        {
            for (int x = 0; x < _mapSize.x; x++)
            {
                for (int y = 0; y < _mapSize.y; y++)
                {
                    _possibleTiles[x, y] = new List<VoxelTile>(_processedTiles);
                }
            }
        }

        private bool GenerateAllPossibleTiles()
        {
            int maxIterations = _mapSize.x * _mapSize.y;
            int iterations = 0;
            int backtracks = 0;

            while (iterations++ < maxIterations)
            {
                if (!ProcessRecalcQueue(ref backtracks))
                {
                    break;
                }

                var (maxCountTile, maxCountTilePosition) = FindMaxEntropyCell();

                if (maxCountTile.Count == 1)
                {
                    Debug.Log($"[WfcMapGenerator] Generated in {iterations} iterations, with {backtracks} backtracks");
                    return true;
                }

                VoxelTile tileToCollapse = TileComparer.GetRandomTile(maxCountTile);
                _possibleTiles[maxCountTilePosition.x, maxCountTilePosition.y] = new List<VoxelTile> { tileToCollapse };
                EnqueueNeighboursToRecalc(maxCountTilePosition);
            }

            Debug.Log($"[WfcMapGenerator] Failed, ran out of iterations with {backtracks} backtracks");
            return false;
        }

        private bool ProcessRecalcQueue(ref int backtracks)
        {
            int innerIterations = 0;

            while (_recalcPossibleTilesQueue.Count > 0 && innerIterations++ < MaxInnerIterations)
            {
                Vector2Int position = _recalcPossibleTilesQueue.Dequeue();

                if (IsEdgePosition(position)) continue;

                List<VoxelTile> possibleTilesHere = _possibleTiles[position.x, position.y];

                int countRemoved = possibleTilesHere.RemoveAll(t => !IsTilePossible(t, position));

                if (countRemoved > 0) EnqueueNeighboursToRecalc(position);

                if (possibleTilesHere.Count == 0)
                {
                    HandleDeadEnd(position);
                    backtracks++;
                }
            }

            return innerIterations < MaxInnerIterations;
        }

        private bool IsEdgePosition(Vector2Int position)
        {
            return position.x == 0 || position.y == 0 ||
                   position.x == _mapSize.x - 1 || position.y == _mapSize.y - 1;
        }

        private void HandleDeadEnd(Vector2Int position)
        {
            _possibleTiles[position.x, position.y] = new List<VoxelTile>(_processedTiles);
            _possibleTiles[position.x + 1, position.y] = new List<VoxelTile>(_processedTiles);
            _possibleTiles[position.x - 1, position.y] = new List<VoxelTile>(_processedTiles);
            _possibleTiles[position.x, position.y + 1] = new List<VoxelTile>(_processedTiles);
            _possibleTiles[position.x, position.y - 1] = new List<VoxelTile>(_processedTiles);

            EnqueueNeighboursToRecalc(position);
        }

        private (List<VoxelTile>, Vector2Int) FindMaxEntropyCell()
        {
            List<VoxelTile> maxCountTile = _possibleTiles[1, 1];
            Vector2Int maxCountTilePosition = new Vector2Int(1, 1);

            for (int x = 1; x < _mapSize.x - 1; x++)
            {
                for (int y = 1; y < _mapSize.y - 1; y++)
                {
                    if (_possibleTiles[x, y].Count > maxCountTile.Count)
                    {
                        maxCountTile = _possibleTiles[x, y];
                        maxCountTilePosition = new Vector2Int(x, y);
                    }
                }
            }

            return (maxCountTile, maxCountTilePosition);
        }

        private bool IsTilePossible(VoxelTile tile, Vector2Int position)
        {
            bool isAllRightImpossible = _possibleTiles[position.x - 1, position.y]
                .All(rightTile => !TileComparer.CanAppendTile(tile, rightTile, Direction.Right));
            if (isAllRightImpossible) return false;

            bool isAllLeftImpossible = _possibleTiles[position.x + 1, position.y]
                .All(leftTile => !TileComparer.CanAppendTile(tile, leftTile, Direction.Left));
            if (isAllLeftImpossible) return false;

            bool isAllForwardImpossible = _possibleTiles[position.x, position.y - 1]
                .All(fwdTile => !TileComparer.CanAppendTile(tile, fwdTile, Direction.Forward));
            if (isAllForwardImpossible) return false;

            bool isAllBackImpossible = _possibleTiles[position.x, position.y + 1]
                .All(backTile => !TileComparer.CanAppendTile(tile, backTile, Direction.Back));
            if (isAllBackImpossible) return false;

            return true;
        }

        private void EnqueueNeighboursToRecalc(Vector2Int position)
        {
            _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x + 1, position.y));
            _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x - 1, position.y));
            _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x, position.y + 1));
            _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x, position.y - 1));
        }

        private Queue<TilePlacementData> BuildPlacementQueue()
        {
            var queue = new Queue<TilePlacementData>();

            for (int x = 1; x < _mapSize.x - 1; x++)
            {
                for (int y = 1; y < _mapSize.y - 1; y++)
                {
                    var list = _possibleTiles[x, y];
                    if (list.Count > 0)
                    {
                        var tile = list[0];
                        queue.Enqueue(new TilePlacementData
                        {
                            GridPosition = new Vector2Int(x, y),
                            TilePrefab = tile,
                            Rotation = tile.transform.rotation
                        });
                    }
                }
            }

            return queue;
        }
    }
}
