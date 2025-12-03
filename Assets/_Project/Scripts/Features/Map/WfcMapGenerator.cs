using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Features.Map
{
    public class WfcMapGenerator : IMapGenerator
    {
        private List<VoxelTile>[,] _possibleTiles;
        private Queue<Vector2Int> _recalcPossibleTilesQueue = new Queue<Vector2Int>();
        private Vector2Int _mapSize;
        private List<VoxelTile> _processedTiles;

        public Queue<TilePlacementData> Generate(Vector2Int mapSize, List<VoxelTile> availableTiles)
        {
            _mapSize = mapSize;
            
            // Prepare tiles (Rotation and Color Calculation) - Logic from Legacy Start()
            _processedTiles = new List<VoxelTile>();
            
            // Create a container for temporary tile variants
            GameObject tempRoot = new GameObject("WFC_Tile_Variants");
            // IMPORTANT: Keep active initially so Physics.Raycast works in CalculateSidesColors
            // Move far away to avoid interference with existing world objects
            tempRoot.transform.position = new Vector3(0, -5000, 0); 

            int tileIndex = 0;
            float spacing = 20f; // Space out tiles to prevent raycast overlap

            // 1. Calculate colors for base tiles and create working copies
            List<VoxelTile> workingList = new List<VoxelTile>();
            foreach (var original in availableTiles)
            {
                 // Instantiate to avoid modifying the original prefab assets
                 VoxelTile clone = UnityEngine.Object.Instantiate(original, tempRoot.transform);
                 clone.transform.localPosition = new Vector3(tileIndex * spacing, 0, 0);
                 tileIndex++;
                 
                 // Force physics update if necessary, though Instantiate usually handles it.
                 Physics.SyncTransforms(); 
                 
                 clone.CalculateSidesColors();
                 workingList.Add(clone);
            }

            // 2. Handle Rotations
            int countBeforeAdding = workingList.Count;
            for (int i = 0; i < countBeforeAdding; i++)
            {
                VoxelTile tile = workingList[i];
                VoxelTile clone;
                
                switch (tile.Rotation)
                {
                    case VoxelTile.RotationType.OnlyRotation:
                        // No extra rotations needed, just keep the original (already in workingList)
                        break;

                    case VoxelTile.RotationType.TwoRotations:
                        tile.Weight /= 2;
                        if (tile.Weight <= 0) tile.Weight = 1;

                        // Add 90 degrees rotated version
                        clone = UnityEngine.Object.Instantiate(tile, tempRoot.transform);
                        clone.transform.localPosition = new Vector3(tileIndex * spacing, 0, 0);
                        tileIndex++;
                        
                        clone.Rotate90();
                        workingList.Add(clone);
                        break;

                    case VoxelTile.RotationType.FourRotations:
                        tile.Weight /= 4;
                        if (tile.Weight <= 0) tile.Weight = 1;

                        // Add 90, 180, 270 degrees rotated versions
                        clone = UnityEngine.Object.Instantiate(tile, tempRoot.transform);
                        clone.transform.localPosition = new Vector3(tileIndex * spacing, 0, 0);
                        tileIndex++;
                        clone.Rotate90();
                        workingList.Add(clone);

                        clone = UnityEngine.Object.Instantiate(tile, tempRoot.transform);
                        clone.transform.localPosition = new Vector3(tileIndex * spacing, 0, 0);
                        tileIndex++;
                        clone.Rotate90();
                        clone.Rotate90();
                        workingList.Add(clone);

                        clone = UnityEngine.Object.Instantiate(tile, tempRoot.transform);
                        clone.transform.localPosition = new Vector3(tileIndex * spacing, 0, 0);
                        tileIndex++;
                        clone.Rotate90();
                        clone.Rotate90();
                        clone.Rotate90();
                        workingList.Add(clone);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            // Hide the prototypes after processing
            tempRoot.SetActive(false);
            
            _processedTiles = workingList;

            // 3. WFC Generation Loop
            _possibleTiles = new List<VoxelTile>[_mapSize.x, _mapSize.y];
            bool success = false;

            int maxAttempts = 10;
            int attempts = 0;
            while (attempts++ < maxAttempts)
            {
                for (int x = 0; x < _mapSize.x; x++)
                for (int y = 0; y < _mapSize.y; y++)
                {
                    _possibleTiles[x, y] = new List<VoxelTile>(_processedTiles);
                }

                VoxelTile tileInCenter = GetRandomTile(_processedTiles);
                _possibleTiles[_mapSize.x / 2, _mapSize.y / 2] = new List<VoxelTile> {tileInCenter};

                _recalcPossibleTilesQueue.Clear();
                EnqueueNeighboursToRecalc(new Vector2Int(_mapSize.x / 2, _mapSize.y / 2));

                success = GenerateAllPossibleTiles();
                
                if (success) break;
            }

            if (!success)
            {
                Debug.LogError("WFC Failed to generate map after max attempts.");
                return new Queue<TilePlacementData>();
            }

            return BuildPlacementQueue();
        }

        private bool GenerateAllPossibleTiles()
        {
            int maxIterations = _mapSize.x * _mapSize.y;
            int iterations = 0;
            int backtracks = 0;
            
            while (iterations++ < maxIterations)
            {
                int maxInnerIterations = 500;
                int innerIterations = 0;
                
                while (_recalcPossibleTilesQueue.Count > 0 && innerIterations++ < maxInnerIterations)
                {
                    Vector2Int position = _recalcPossibleTilesQueue.Dequeue();
                    if (position.x == 0 || position.y == 0 ||
                        position.x == _mapSize.x - 1 || position.y == _mapSize.y - 1)
                    {
                        continue;
                    }

                    List<VoxelTile> possibleTilesHere = _possibleTiles[position.x, position.y];

                    int countRemoved = possibleTilesHere.RemoveAll(t => !IsTilePossible(t, position));

                    if (countRemoved > 0) EnqueueNeighboursToRecalc(position);

                    if (possibleTilesHere.Count == 0)
                    {
                        // Dead end, reset this and neighbors
                        possibleTilesHere.AddRange(_processedTiles);
                        _possibleTiles[position.x + 1, position.y] = new List<VoxelTile>(_processedTiles);
                        _possibleTiles[position.x - 1, position.y] = new List<VoxelTile>(_processedTiles);
                        _possibleTiles[position.x, position.y + 1] = new List<VoxelTile>(_processedTiles);
                        _possibleTiles[position.x, position.y - 1] = new List<VoxelTile>(_processedTiles);
                        
                        EnqueueNeighboursToRecalc(position);

                        backtracks++;
                    }
                }
                if (innerIterations == maxInnerIterations) break;

                // Find cell with MAX entropy (Legacy logic)
                List<VoxelTile> maxCountTile = _possibleTiles[1, 1];
                Vector2Int maxCountTilePosition = new Vector2Int(1, 1);

                for (int x = 1; x < _mapSize.x - 1; x++)
                for (int y = 1; y < _mapSize.y - 1; y++)
                {
                    if (_possibleTiles[x, y].Count > maxCountTile.Count)
                    {
                        maxCountTile = _possibleTiles[x, y];
                        maxCountTilePosition = new Vector2Int(x, y);
                    }
                }

                if (maxCountTile.Count == 1)
                {
                    Debug.Log($"Generated for {iterations} iterations, with {backtracks} backtracks");
                    return true;
                }

                VoxelTile tileToCollapse = GetRandomTile(maxCountTile);
                _possibleTiles[maxCountTilePosition.x, maxCountTilePosition.y] = new List<VoxelTile> {tileToCollapse};
                EnqueueNeighboursToRecalc(maxCountTilePosition);
            }
            
            Debug.Log($"Failed, run out of iterations with {backtracks} backtracks");
            return false;
        }

        private bool IsTilePossible(VoxelTile tile, Vector2Int position)
        {
            bool isAllRightImpossible = _possibleTiles[position.x - 1, position.y]
                .All(rightTile => !CanAppendTile(tile, rightTile, Direction.Right));
            if (isAllRightImpossible) return false;
            
            bool isAllLeftImpossible = _possibleTiles[position.x + 1, position.y]
                .All(leftTile => !CanAppendTile(tile, leftTile, Direction.Left));
            if (isAllLeftImpossible) return false;
            
            bool isAllForwardImpossible = _possibleTiles[position.x, position.y - 1]
                .All(fwdTile => !CanAppendTile(tile, fwdTile, Direction.Forward));
            if (isAllForwardImpossible) return false;
            
            bool isAllBackImpossible = _possibleTiles[position.x, position.y + 1]
                .All(backTile => !CanAppendTile(tile, backTile, Direction.Back));
            if (isAllBackImpossible) return false;

            return true;
        }

        private bool CanAppendTile(VoxelTile existingTile, VoxelTile tileToAppend, Direction direction)
        {
            if (existingTile == null) return true;

            if (direction == Direction.Right)
            {
                return Enumerable.SequenceEqual(existingTile.ColorsRight, tileToAppend.ColorsLeft);
            }
            else if (direction == Direction.Left)
            {
                return Enumerable.SequenceEqual(existingTile.ColorsLeft, tileToAppend.ColorsRight);
            }
            else if (direction == Direction.Forward)
            {
                return Enumerable.SequenceEqual(existingTile.ColorsForward, tileToAppend.ColorsBack);
            }
            else if (direction == Direction.Back)
            {
                return Enumerable.SequenceEqual(existingTile.ColorsBack, tileToAppend.ColorsForward);
            }
            else
            {
                throw new ArgumentException("Wrong direction value, should be Vector3.left/right/back/forward",
                    nameof(direction));
            }
        }

        private void EnqueueNeighboursToRecalc(Vector2Int position)
        {
            _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x + 1, position.y));
            _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x - 1, position.y));
            _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x, position.y + 1));
            _recalcPossibleTilesQueue.Enqueue(new Vector2Int(position.x, position.y - 1));
        }

        private VoxelTile GetRandomTile(List<VoxelTile> availableTiles)
        {
            List<float> chances = new List<float>();
            for (int i = 0; i < availableTiles.Count; i++)
            {
                chances.Add(availableTiles[i].Weight);
            }

            float value = Random.Range(0, chances.Sum());
            float sum = 0;

            for (int i = 0; i < chances.Count; i++)
            {
                sum += chances[i];
                if (value < sum)
                {
                    return availableTiles[i];
                }
            }

            return availableTiles[availableTiles.Count - 1];
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
