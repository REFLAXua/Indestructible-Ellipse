using System.Collections.Generic;
using UnityEngine;

namespace Features.Map
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private MapVisualizer _visualizer;
        [SerializeField] private Vector2Int _mapSize = new Vector2Int(10, 10);
        [SerializeField] private List<VoxelTile> _tilePrefabs;

        private IMapGenerator _generator;
        private List<VoxelTile> _processedTiles;

        private void Start()
        {
            _generator = new WfcMapGenerator();
            InitializeTiles();
            GenerateMap();
        }

        private void InitializeTiles()
        {
            _processedTiles = new List<VoxelTile>();
            
            // Pre-calculate colors and handle rotations
            // This logic is copied/adapted from original VoxelTilePlacerSimple
            
            // Note: We need to instantiate them to calculate colors if they rely on MeshCollider bounds in scene?
            // Or can we do it on prefabs? Original code instantiated them.
            // For safety, let's instantiate prototypes, calculate, and use them as "Prefabs" for the generator logic,
            // but the Visualizer needs to instantiate fresh copies.
            
            // Actually, the generator logic needs the COLORS.
            // The Visualizer needs the PREFABS.
            
            // Let's create a list of "Prototype" tiles that are hidden, just for logic.
            GameObject prototypeHolder = new GameObject("TilePrototypes");
            prototypeHolder.SetActive(false);

            foreach (var prefab in _tilePrefabs)
            {
                // We need to handle rotations here too
                // This is a bit complex to port 1:1 without refactoring VoxelTile itself, 
                // but let's try to keep it functional.
                
                ProcessTile(prefab, prototypeHolder.transform);
            }
        }

        private void ProcessTile(VoxelTile prefab, Transform holder)
        {
             VoxelTile clone;
             
             // Base (0 rotation)
             clone = Instantiate(prefab, holder);
             clone.CalculateSidesColors();
             _processedTiles.Add(clone);
             
             // Rotations
             if (prefab.Rotation == VoxelTile.RotationType.TwoRotations)
             {
                 clone = Instantiate(prefab, holder);
                 clone.CalculateSidesColors(); // Calculate BEFORE rotation
                 clone.Rotate90();
                 _processedTiles.Add(clone);
             }
             else if (prefab.Rotation == VoxelTile.RotationType.FourRotations)
             {
                 for (int i = 0; i < 3; i++)
                 {
                     clone = Instantiate(prefab, holder); // Always instantiate from prefab
                     clone.CalculateSidesColors(); // Calculate base colors
                     
                     // Rotate i+1 times (90, 180, 270)
                     for(int r = 0; r <= i; r++)
                     {
                        clone.Rotate90();
                     }
                     _processedTiles.Add(clone);
                 }
             }
        }

        public void GenerateMap()
        {
            var queue = _generator.Generate(_mapSize, _processedTiles);
            _visualizer.Visualize(queue, OnGenerationComplete);
        }

        private void OnGenerationComplete(System.Collections.Generic.Dictionary<Vector2Int, VoxelTile> tiles)
        {
            Debug.Log("Map Generation Complete!");
            
            var enemyPlacer = FindObjectOfType<Features.Enemy.EnemyPlacer>();
            if (enemyPlacer != null)
            {
                enemyPlacer.Initialize(tiles, _mapSize);
            }
        }
    }
}
