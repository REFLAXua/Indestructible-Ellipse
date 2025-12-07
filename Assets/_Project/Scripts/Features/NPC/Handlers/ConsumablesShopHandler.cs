using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class ConsumablesShopHandler : NPCInteractionHandlerBase
    {
        [Header("Shop Settings")]
        [SerializeField] private string _shopId = "consumables_shop";
        
        [Header("Available Items")]
        [SerializeField] private ConsumableItem[] _availableItems;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onShopOpened;
        [SerializeField] private UnityEvent _onShopClosed;
        [SerializeField] private UnityEvent<string> _onItemPurchased;

        public string ShopId => _shopId;
        public ConsumableItem[] AvailableItems => _availableItems;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[ConsumablesShopHandler] Відкриття магазину витратників: {_shopId}");
            
            _onShopOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onShopClosed?.Invoke();
        }

        public void PurchaseItem(string itemId, int quantity)
        {
            Debug.Log($"[ConsumablesShopHandler] Покупка: {itemId} x{quantity}");
            _onItemPurchased?.Invoke(itemId);
        }
    }

    [System.Serializable]
    public struct ConsumableItem
    {
        public string ItemId;
        public string ItemName;
        public ConsumableType Type;
        public int Price;
        public int MaxStack;
        public Sprite Icon;
        [TextArea(1, 3)]
        public string Description;
    }

    public enum ConsumableType
    {
        HealthPotion,
        ManaPotion,
        StaminaPotion,
        BuffPotion,
        Antidote,
        Scroll,
        Other
    }
}
