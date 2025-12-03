using System.Collections.Generic;
using UnityEngine;
using Features.Map;

namespace Features.Enemy
{
    public class EnemyPlacer : MonoBehaviour
    {
        public GameObject enemyPrefab;
        public int enemyCount = 5;

        private Dictionary<Vector2Int, VoxelTile> _spawnedTiles;
        private Vector2Int _mapSize;

        public void Initialize(Dictionary<Vector2Int, VoxelTile> tiles, Vector2Int mapSize)
        {
            _spawnedTiles = tiles;
            _mapSize = mapSize;
            PlaceEnemiesAndWaypoints();
        }

        private void PlaceEnemiesAndWaypoints()
        {
            if (_spawnedTiles == null || _spawnedTiles.Count == 0)
            {
                Debug.LogError("spawnedTiles not assigned or empty!");
                return;
            }

            List<Vector2Int> availablePositions = new List<Vector2Int>(_spawnedTiles.Keys);

            for (int i = 0; i < enemyCount; i++)
            {
                if (availablePositions.Count == 0) break;
                int idx = Random.Range(0, availablePositions.Count);
                Vector2Int pos = availablePositions[idx];
                availablePositions.RemoveAt(idx);

                VoxelTile tile = _spawnedTiles[pos];
                // Start raycast from a reasonable height above the tile
                Vector3 rayStart = tile.transform.position + Vector3.up * 10f; 

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f))
                {
                    // Found ground, now find nearest NavMesh point
                    if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out UnityEngine.AI.NavMeshHit navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        // Instantiate at the valid NavMesh position
                        GameObject enemy = Instantiate(enemyPrefab, navHit.position, Quaternion.identity);
                        
                        // Ensure agent is placed correctly (optional, as Instantiate position should be correct)
                        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (agent != null)
                        {
                            agent.Warp(navHit.position);
                        }
                    }
                    else
                    {
                        // Debug.LogWarning($"Could not find NavMesh near tile {pos}");
                    }
                }
            }
        }
    }
}
