using UnityEngine;
using Features.NPC.Interfaces;

namespace Features.NPC.Data
{
    [CreateAssetMenu(fileName = "MerchantConfig", menuName = "LetsRoll/NPC/Merchant Config")]
    public class MerchantConfigSO : NPCConfigSO
    {
        [Header("Merchant Specific")]
        [SerializeField] private string _shopId;
        [SerializeField] private MerchantCategory _category;
        [SerializeField] private float _priceMultiplier = 1f;
        [SerializeField] private bool _canBuyFromPlayer = true;
        [SerializeField] private float _buyPriceMultiplier = 0.5f;

        public string ShopId => _shopId;
        public MerchantCategory Category => _category;
        public float PriceMultiplier => _priceMultiplier;
        public bool CanBuyFromPlayer => _canBuyFromPlayer;
        public float BuyPriceMultiplier => _buyPriceMultiplier;

        private void OnValidate()
        {
            _priceMultiplier = Mathf.Max(0.1f, _priceMultiplier);
            _buyPriceMultiplier = Mathf.Clamp(_buyPriceMultiplier, 0.1f, 1f);
        }
    }

    public enum MerchantCategory
    {
        Weapons,
        Armor,
        Potions,
        Scrolls,
        Miscellaneous,
        General
    }
}
