using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

namespace Features.Map
{
    public class MapVisualizer : MonoBehaviour
    {
        [SerializeField] private float _delayBetweenTiles = 0.05f;
        [SerializeField] private float _spawnHeight = 10f;
        [SerializeField] private float _fallSpeed = 10f;
        [SerializeField] private NavMeshSurface _navMeshSurface;

        private Dictionary<string, Queue<VoxelTile>> _pool = new Dictionary<string, Queue<VoxelTile>>();

        public void Visualize(Queue<TilePlacementData> placementQueue, Action<Dictionary<Vector2Int, VoxelTile>> onComplete = null)
        {
            StartCoroutine(BuildRoutine(placementQueue, onComplete));
        }

        private IEnumerator BuildRoutine(Queue<TilePlacementData> queue, Action<Dictionary<Vector2Int, VoxelTile>> onComplete)
        {
            Dictionary<Vector2Int, VoxelTile> spawnedTiles = new Dictionary<Vector2Int, VoxelTile>();

            while (queue.Count > 0)
            {
                var data = queue.Dequeue();
                var tile = SpawnAndAnimateTile(data);
                spawnedTiles[data.GridPosition] = tile;
                yield return new WaitForSeconds(_delayBetweenTiles);
            }

            yield return new WaitForSeconds(0.5f); // Wait for last tiles to fall
            
            if (_navMeshSurface != null)
            {
                _navMeshSurface.BuildNavMesh();
            }

            onComplete?.Invoke(spawnedTiles);
        }

        private VoxelTile SpawnAndAnimateTile(TilePlacementData data)
        {
            float tileSize = data.TilePrefab.VoxelSize * data.TilePrefab.TileSideVoxels;
            Vector3 targetPos = new Vector3(data.GridPosition.x * tileSize, 0, data.GridPosition.y * tileSize);
            
            if (tileSize <= 0.001f)
            {
                Debug.LogError($"TileSize is zero or negative! VoxelSize: {data.TilePrefab.VoxelSize}, SideVoxels: {data.TilePrefab.TileSideVoxels}, Prefab: {data.TilePrefab.name}");
            }
            // Debug.Log($"Spawning tile at Grid: {data.GridPosition}, World: {targetPos}, TileSize: {tileSize}");

            VoxelTile tileInstance = GetFromPool(data.TilePrefab);
            tileInstance.transform.SetParent(transform);
            tileInstance.transform.position = targetPos + Vector3.up * _spawnHeight;
            tileInstance.transform.rotation = data.Rotation;
            tileInstance.gameObject.SetActive(true);

            StartCoroutine(AnimateFall(tileInstance.transform, targetPos));
            return tileInstance;
        }

        private VoxelTile GetFromPool(VoxelTile prefab)
        {
            string key = prefab.name;
            if (!_pool.ContainsKey(key))
            {
                _pool[key] = new Queue<VoxelTile>();
            }

            if (_pool[key].Count > 0)
            {
                return _pool[key].Dequeue();
            }

            // Instantiate new if empty
            var instance = Instantiate(prefab);
            instance.name = prefab.name; // Keep name consistent
            return instance;
        }

        public void ReturnToPool(VoxelTile tile)
        {
            tile.gameObject.SetActive(false);
            string key = tile.name;
            if (!_pool.ContainsKey(key))
            {
                _pool[key] = new Queue<VoxelTile>();
            }
            _pool[key].Enqueue(tile);
        }

        private IEnumerator AnimateFall(Transform target, Vector3 endPos)
        {
            while (Vector3.Distance(target.position, endPos) > 0.01f)
            {
                target.position = Vector3.MoveTowards(target.position, endPos, _fallSpeed * Time.deltaTime);
                yield return null;
            }
            target.position = endPos;
        }
    }
}
