using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class LootboxHandler : NPCInteractionHandlerBase
    {
        [Header("Lootbox Settings")]
        [SerializeField] private string _lootboxId = "reward_lootbox";
        
        [Header("Available Lootboxes")]
        [SerializeField] private LootboxType[] _availableLootboxes;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onLootboxMenuOpened;
        [SerializeField] private UnityEvent _onLootboxMenuClosed;
        [SerializeField] private UnityEvent<string> _onLootboxOpened;
        [SerializeField] private UnityEvent<LootboxReward[]> _onRewardsReceived;

        public string LootboxId => _lootboxId;
        public LootboxType[] AvailableLootboxes => _availableLootboxes;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[LootboxHandler] Відкриття меню лутбоксів: {_lootboxId}");
            
            _onLootboxMenuOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onLootboxMenuClosed?.Invoke();
        }

        public void OpenLootbox(string lootboxTypeId)
        {
            Debug.Log($"[LootboxHandler] Відкриття лутбокса: {lootboxTypeId}");
            _onLootboxOpened?.Invoke(lootboxTypeId);
            
            var rewards = GenerateRewards(lootboxTypeId);
            _onRewardsReceived?.Invoke(rewards);
        }

        private LootboxReward[] GenerateRewards(string lootboxTypeId)
        {
            Debug.Log("[LootboxHandler] Генерація нагород...");
            return new LootboxReward[0];
        }
    }

    [System.Serializable]
    public struct LootboxType
    {
        public string TypeId;
        public string DisplayName;
        public LootboxRarity Rarity;
        public int Price;
        public CurrencyType PriceCurrency;
        public Sprite Icon;
        [TextArea(1, 2)]
        public string Description;
    }

    [System.Serializable]
    public struct LootboxReward
    {
        public string RewardId;
        public string RewardName;
        public RewardType Type;
        public int Quantity;
        public LootboxRarity Rarity;
        public Sprite Icon;
    }

    public enum LootboxRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum RewardType
    {
        Currency,
        Consumable,
        Weapon,
        Spell,
        Skill,
        Cosmetic
    }

    public enum CurrencyType
    {
        Gold,
        Gems,
        Tokens,
        Free
    }
}
