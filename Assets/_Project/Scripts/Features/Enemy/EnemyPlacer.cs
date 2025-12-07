using System.Collections.Generic;
using UnityEngine;
using Features.Map;

namespace Features.Enemy
{
    public class EnemyPlacer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private int _enemyCount = 5;
        [SerializeField] private float _raycastHeight = 10f;
        [SerializeField] private float _raycastDistance = 20f;
        [SerializeField] private float _navMeshSampleRadius = 2.0f;

        private Dictionary<Vector2Int, VoxelTile> _spawnedTiles;
        private Vector2Int _mapSize;

        public void Initialize(Dictionary<Vector2Int, VoxelTile> tiles, Vector2Int mapSize)
        {
            if (tiles == null || tiles.Count == 0)
            {
                Debug.LogError("[EnemyPlacer] Tiles not assigned or empty!");
                return;
            }

            _spawnedTiles = tiles;
            _mapSize = mapSize;
            PlaceEnemies();
        }

        private void PlaceEnemies()
        {
            if (_enemyPrefab == null)
            {
                Debug.LogError("[EnemyPlacer] Enemy prefab not assigned!");
                return;
            }

            List<Vector2Int> availablePositions = new List<Vector2Int>(_spawnedTiles.Keys);

            for (int i = 0; i < _enemyCount; i++)
            {
                if (availablePositions.Count == 0) break;

                Vector2Int pos = SelectAndRemoveRandomPosition(availablePositions);
                TrySpawnEnemyAtTile(pos);
            }
        }

        private Vector2Int SelectAndRemoveRandomPosition(List<Vector2Int> positions)
        {
            int idx = Random.Range(0, positions.Count);
            Vector2Int pos = positions[idx];
            positions.RemoveAt(idx);
            return pos;
        }

        private void TrySpawnEnemyAtTile(Vector2Int pos)
        {
            if (!_spawnedTiles.TryGetValue(pos, out VoxelTile tile)) return;

            Vector3 rayStart = tile.transform.position + Vector3.up * _raycastHeight;

            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, _raycastDistance)) return;

            if (!UnityEngine.AI.NavMesh.SamplePosition(hit.point, out UnityEngine.AI.NavMeshHit navHit, _navMeshSampleRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                return;
            }

            SpawnEnemy(navHit.position);
        }

        private void SpawnEnemy(Vector3 position)
        {
            GameObject enemy = Instantiate(_enemyPrefab, position, Quaternion.identity);

            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.Warp(position);
            }
        }
    }
}
