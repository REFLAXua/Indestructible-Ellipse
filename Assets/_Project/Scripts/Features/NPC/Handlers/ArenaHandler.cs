using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class ArenaHandler : NPCInteractionHandlerBase
    {
        [Header("Arena Settings")]
        [SerializeField] private string _arenaId = "main_arena";
        [SerializeField] private string _arenaSceneName = "ArenaScene";
        
        [Header("Difficulty Settings")]
        [SerializeField] private ArenaDifficulty[] _difficulties;
        
        [Header("Available Enemies")]
        [SerializeField] private ArenaEnemy[] _availableEnemies;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onArenaMenuOpened;
        [SerializeField] private UnityEvent _onArenaMenuClosed;
        [SerializeField] private UnityEvent<ArenaDifficulty[]> _onDifficultiesLoaded;
        [SerializeField] private UnityEvent<ArenaEnemy[]> _onEnemiesLoaded;
        [SerializeField] private UnityEvent<string> _onDifficultySelected;
        [SerializeField] private UnityEvent<string> _onEnemySelected;
        [SerializeField] private UnityEvent _onBattleStarted;
        [SerializeField] private UnityEvent<string> _onEnemyLocked;

        private ArenaDifficulty? _selectedDifficulty;
        private ArenaEnemy? _selectedEnemy;

        public string ArenaId => _arenaId;
        public ArenaDifficulty[] Difficulties => _difficulties;
        public ArenaEnemy[] AvailableEnemies => _availableEnemies;
        public ArenaDifficulty? SelectedDifficulty => _selectedDifficulty;
        public ArenaEnemy? SelectedEnemy => _selectedEnemy;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[ArenaHandler] Відкриття меню арени: {_arenaId}");
            
            _onArenaMenuOpened?.Invoke();
            _onDifficultiesLoaded?.Invoke(_difficulties);
            _onEnemiesLoaded?.Invoke(GetAvailableEnemies());
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _selectedDifficulty = null;
            _selectedEnemy = null;
            _onArenaMenuClosed?.Invoke();
        }

        public ArenaEnemy[] GetAvailableEnemies()
        {
            return _availableEnemies;
        }

        public void SelectDifficulty(string difficultyId)
        {
            var difficulty = System.Array.Find(_difficulties, d => d.DifficultyId == difficultyId);
            
            if (string.IsNullOrEmpty(difficulty.DifficultyId))
            {
                Debug.LogWarning($"[ArenaHandler] Складність не знайдено: {difficultyId}");
                return;
            }

            _selectedDifficulty = difficulty;
            Debug.Log($"[ArenaHandler] Обрано складність: {difficulty.DisplayName}");
            _onDifficultySelected?.Invoke(difficultyId);
        }

        public void SelectEnemy(string enemyId)
        {
            var enemy = System.Array.Find(_availableEnemies, e => e.EnemyId == enemyId);
            
            if (string.IsNullOrEmpty(enemy.EnemyId))
            {
                Debug.LogWarning($"[ArenaHandler] Ворога не знайдено: {enemyId}");
                return;
            }

            if (!enemy.IsUnlocked)
            {
                Debug.Log($"[ArenaHandler] Ворог заблокований: {enemy.DisplayName}");
                _onEnemyLocked?.Invoke(enemyId);
                return;
            }

            _selectedEnemy = enemy;
            Debug.Log($"[ArenaHandler] Обрано ворога: {enemy.DisplayName}");
            _onEnemySelected?.Invoke(enemyId);
        }

        public bool CanStartBattle()
        {
            return _selectedDifficulty.HasValue && _selectedEnemy.HasValue;
        }

        public void StartBattle()
        {
            if (!CanStartBattle())
            {
                Debug.LogWarning("[ArenaHandler] Оберіть складність та ворога");
                return;
            }

            var difficulty = _selectedDifficulty.Value;
            var enemy = _selectedEnemy.Value;

            Debug.Log($"[ArenaHandler] Початок бою: {enemy.DisplayName} на складності {difficulty.DisplayName}");
            Debug.Log($"  - HP множник: {difficulty.HealthMultiplier}x");
            Debug.Log($"  - Урон множник: {difficulty.DamageMultiplier}x");
            Debug.Log($"  - Нагорода: {difficulty.RewardMultiplier}x");

            _onBattleStarted?.Invoke();
        }

        public void QuickBattle(string difficultyId, string enemyId)
        {
            SelectDifficulty(difficultyId);
            SelectEnemy(enemyId);
            
            if (CanStartBattle())
            {
                StartBattle();
            }
        }

        public ArenaEnemy[] GetEnemiesByType(EnemyArenaType type)
        {
            return System.Array.FindAll(_availableEnemies, e => e.Type == type);
        }
    }

    [System.Serializable]
    public struct ArenaDifficulty
    {
        public string DifficultyId;
        public string DisplayName;
        public float HealthMultiplier;
        public float DamageMultiplier;
        public float SpeedMultiplier;
        public float RewardMultiplier;
        public int RequiredPlayerLevel;
        public Sprite Icon;
        public Color DisplayColor;
        [TextArea(1, 2)]
        public string Description;
    }

    [System.Serializable]
    public struct ArenaEnemy
    {
        public string EnemyId;
        public string DisplayName;
        public EnemyArenaType Type;
        public int BaseHealth;
        public int BaseDamage;
        public int RequiredPlayerLevel;
        public bool IsUnlocked;
        public Sprite Icon;
        public Sprite LockedIcon;
        public GameObject EnemyPrefab;
        [TextArea(1, 3)]
        public string Description;
    }

    public enum EnemyArenaType
    {
        Normal,
        Elite,
        Boss,
        Swarm
    }
}

