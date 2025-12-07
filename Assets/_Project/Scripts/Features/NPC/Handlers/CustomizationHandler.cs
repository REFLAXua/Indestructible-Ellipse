using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class CustomizationHandler : NPCInteractionHandlerBase
    {
        [Header("Customization Settings")]
        [SerializeField] private string _customizationId = "character_customization";
        
        [Header("Available Options")]
        [SerializeField] private CustomizationCategory[] _availableCategories;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onCustomizationOpened;
        [SerializeField] private UnityEvent _onCustomizationClosed;
        [SerializeField] private UnityEvent<string, string> _onItemEquipped;
        [SerializeField] private UnityEvent<string> _onItemPurchased;

        public string CustomizationId => _customizationId;
        public CustomizationCategory[] AvailableCategories => _availableCategories;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[CustomizationHandler] Відкриття кастомізації: {_customizationId}");
            
            _onCustomizationOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onCustomizationClosed?.Invoke();
        }

        public void EquipItem(string categoryId, string itemId)
        {
            Debug.Log($"[CustomizationHandler] Екіпування: {categoryId} -> {itemId}");
            _onItemEquipped?.Invoke(categoryId, itemId);
        }

        public void PurchaseItem(string itemId)
        {
            Debug.Log($"[CustomizationHandler] Покупка косметики: {itemId}");
            _onItemPurchased?.Invoke(itemId);
        }

        public void PreviewItem(string categoryId, string itemId)
        {
            Debug.Log($"[CustomizationHandler] Перегляд: {categoryId} -> {itemId}");
        }
    }

    [System.Serializable]
    public struct CustomizationCategory
    {
        public string CategoryId;
        public string DisplayName;
        public CustomizationType Type;
        public CustomizationItem[] Items;
    }

    [System.Serializable]
    public struct CustomizationItem
    {
        public string ItemId;
        public string ItemName;
        public int Price;
        public bool IsOwned;
        public bool IsEquipped;
        public Sprite PreviewIcon;
        public GameObject PreviewModel;
    }

    public enum CustomizationType
    {
        Skin,
        Armor,
        Helmet,
        Cape,
        Weapon,
        Aura,
        Trail,
        Emote
    }
}
