using UnityEngine;
using UnityEngine.Events;
using Core;
using Features.NPC.Events;

namespace Features.NPC.Handlers
{
    public class MerchantHandler : NPCInteractionHandlerBase
    {
        [Header("Merchant Settings")]
        [SerializeField] private string _shopId;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onShopOpened;
        [SerializeField] private UnityEvent _onShopClosed;

        public string ShopId => _shopId;

        public override void Execute(NPCController npc)
        {
            Debug.Log($"[MerchantHandler] Відкриття магазину: {_shopId}");
            
            _onShopOpened?.Invoke();
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            _onShopClosed?.Invoke();
        }
    }
}
