using UnityEngine;
using UnityEngine.Events;

namespace Features.NPC.Handlers
{
    public class WeaponShopHandler : NPCInteractionHandlerBase
    {
        [Header("Shop Settings")]
        [SerializeField] private string _shopId = "weapon_shop";
        
        [Header("Shop Capabilities")]
        [SerializeField] private bool _canSellWeapons = true;
        [SerializeField] private bool _canUpgradeWeapons = true;
        
        [Header("Available Weapons")]
        [SerializeField] private WeaponItem[] _availableWeapons;
        
        [Header("Upgrade Settings")]
        [SerializeField] private int _maxUpgradeLevel = 10;
        [SerializeField] private float _upgradeCostMultiplier = 1.5f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onShopOpened;
        [SerializeField] private UnityEvent _onShopClosed;
        [SerializeField] private UnityEvent<string> _onWeaponPurchased;
        [SerializeField] private UnityEvent<string, int> _onWeaponUpgraded;

        public string ShopId => _shopId;
        public bool CanSellWeapons => _canSellWeapons;
        public bool CanUpgradeWeapons => _canUpgradeWeapons;
        public WeaponItem[] AvailableWeapons => _availableWeapons;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[WeaponShopHandler] Відкриття магазину зброї: {_shopId}");
            Debug.Log($"  - Продаж: {_canSellWeapons}, Апгрейди: {_canUpgradeWeapons}");
            
            _onShopOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onShopClosed?.Invoke();
        }

        public void PurchaseWeapon(string weaponId)
        {
            Debug.Log($"[WeaponShopHandler] Покупка зброї: {weaponId}");
            _onWeaponPurchased?.Invoke(weaponId);
        }

        public void UpgradeWeapon(string weaponId, int currentLevel)
        {
            if (currentLevel >= _maxUpgradeLevel)
            {
                Debug.LogWarning($"[WeaponShopHandler] Зброя {weaponId} вже максимального рівня");
                return;
            }

            int newLevel = currentLevel + 1;
            Debug.Log($"[WeaponShopHandler] Апгрейд зброї: {weaponId} до рівня {newLevel}");
            _onWeaponUpgraded?.Invoke(weaponId, newLevel);
        }

        public int CalculateUpgradeCost(int basePrice, int currentLevel)
        {
            return Mathf.RoundToInt(basePrice * Mathf.Pow(_upgradeCostMultiplier, currentLevel));
        }
    }

    [System.Serializable]
    public struct WeaponItem
    {
        public string WeaponId;
        public string WeaponName;
        public WeaponType Type;
        public int BasePrice;
        public int BaseDamage;
        public float AttackSpeed;
        public Sprite Icon;
        [TextArea(1, 3)]
        public string Description;
    }

    public enum WeaponType
    {
        Sword,
        Axe,
        Mace,
        Dagger,
        Spear,
        Bow,
        Staff
    }
}
