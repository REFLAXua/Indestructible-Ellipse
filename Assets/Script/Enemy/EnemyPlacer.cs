using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlacer : MonoBehaviour
{
    public VoxelTile[,] spawnedTiles; // Assign this from your tile placer
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public int waypointsPerEnemy = 3;

    void Start()
    {
        PlaceEnemiesAndWaypoints();
    }

    public void PlaceEnemiesAndWaypoints()
    {
        if (spawnedTiles == null)
        {
            Debug.LogError("spawnedTiles not assigned!");
            return;
        }

        List<Vector2Int> availablePositions = new List<Vector2Int>();
        int mapSizeX = spawnedTiles.GetLength(0);
        int mapSizeY = spawnedTiles.GetLength(1);

        for (int x = 1; x < mapSizeX - 1; x++)
        for (int y = 1; y < mapSizeY - 1; y++)
        {
            if (spawnedTiles[x, y] != null)
                availablePositions.Add(new Vector2Int(x, y));
        }

        for (int i = 0; i < enemyCount; i++)
        {
            if (availablePositions.Count == 0) break;
            int idx = Random.Range(0, availablePositions.Count);
            Vector2Int pos = availablePositions[idx];
            availablePositions.RemoveAt(idx);

            VoxelTile tile = spawnedTiles[pos.x, pos.y];
            Vector3 spawnAbove = tile.transform.position + Vector3.up * 50f; // spawn high above

            GameObject enemy = Instantiate(enemyPrefab, spawnAbove, Quaternion.identity);

            // Raycast down to find the tile surface
            RaycastHit hit;
            if (Physics.Raycast(spawnAbove, Vector3.down, out hit, 100f))
            {
                enemy.transform.position = hit.point + Vector3.up * 1.5f; // offset to avoid clipping
            }
            else
            {
                Debug.LogWarning("Enemy raycast did not hit a tile!", enemy);
            }

            List<Transform> waypoints = new List<Transform>();
            for (int w = 0; w < waypointsPerEnemy; w++)
            {
                Vector2Int wpPos = availablePositions[Random.Range(0, availablePositions.Count)];
                VoxelTile wpTile = spawnedTiles[wpPos.x, wpPos.y];
                Vector3 wpSpawnAbove = wpTile.transform.position + Vector3.up * 50f;

                // Raycast down to find the tile surface for the waypoint
                RaycastHit wpHit;
                Vector3 wpWorldPos = wpTile.transform.position + Vector3.up * 0.5f; // fallback position
                if (Physics.Raycast(wpSpawnAbove, Vector3.down, out wpHit, 100f))
                {
                    wpWorldPos = wpHit.point + Vector3.up * 0.5f; // offset to avoid clipping
                }
                else
                {
                    Debug.LogWarning($"Waypoint raycast did not hit a tile for enemy {i}, waypoint {w}!", wpTile);
                }

                GameObject wpObj = new GameObject($"Waypoint_{i}_{w}");
                wpObj.transform.position = wpWorldPos;
                waypoints.Add(wpObj.transform);
            }

            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.wayPoints = new GameObject("WaypointsParent").transform;
                foreach (var wp in waypoints)
                {
                    wp.parent = movement.wayPoints;
                }
            }
        }
    }
}
