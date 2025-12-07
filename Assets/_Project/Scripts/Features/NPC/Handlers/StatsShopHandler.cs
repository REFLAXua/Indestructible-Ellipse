using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class StatsShopHandler : NPCInteractionHandlerBase
    {
        [Header("Shop Settings")]
        [SerializeField] private string _shopId = "stats_shop";
        
        [Header("Available Stats")]
        [SerializeField] private StatUpgrade[] _availableStats;
        
        [Header("Pricing")]
        [SerializeField] private float _costMultiplierPerLevel = 1.5f;
        [SerializeField] private int _maxLevelPerStat = 100;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onShopOpened;
        [SerializeField] private UnityEvent _onShopClosed;
        [SerializeField] private UnityEvent<StatType, int> _onStatUpgraded;

        public string ShopId => _shopId;
        public StatUpgrade[] AvailableStats => _availableStats;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[StatsShopHandler] Відкриття магазину характеристик: {_shopId}");
            
            _onShopOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onShopClosed?.Invoke();
        }

        public void UpgradeStat(StatType statType, int currentLevel)
        {
            if (currentLevel >= _maxLevelPerStat)
            {
                Debug.LogWarning($"[StatsShopHandler] Характеристика {statType} вже максимального рівня");
                return;
            }

            int newLevel = currentLevel + 1;
            Debug.Log($"[StatsShopHandler] Підвищення {statType} до рівня {newLevel}");
            _onStatUpgraded?.Invoke(statType, newLevel);
        }

        public int CalculateUpgradeCost(StatType statType, int currentLevel)
        {
            var statConfig = System.Array.Find(_availableStats, s => s.Type == statType);
            return Mathf.RoundToInt(statConfig.BaseCost * Mathf.Pow(_costMultiplierPerLevel, currentLevel));
        }

        public float GetStatBonusAtLevel(StatType statType, int level)
        {
            var statConfig = System.Array.Find(_availableStats, s => s.Type == statType);
            return statConfig.BonusPerLevel * level;
        }
    }

    [System.Serializable]
    public struct StatUpgrade
    {
        public StatType Type;
        public string DisplayName;
        public int BaseCost;
        public float BonusPerLevel;
        public Sprite Icon;
        [TextArea(1, 2)]
        public string Description;
    }

    public enum StatType
    {
        MaxHealth,
        MaxMana,
        MaxStamina,
        Strength,
        Agility,
        Intelligence,
        Vitality,
        Luck,
        CriticalChance,
        CriticalDamage,
        AttackSpeed,
        MoveSpeed,
        Defense,
        MagicResist
    }
}
