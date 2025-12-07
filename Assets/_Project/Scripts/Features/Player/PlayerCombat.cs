using UnityEngine;
using Features.Player.Data;

namespace Features.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        private PlayerController _player;
        private PlayerConfigSO _config;
        private SwordHitbox _hitbox;

        public void Initialize(PlayerController player)
        {
            _player = player;
            _config = player.Config;

            _hitbox = player.GetComponentInChildren<SwordHitbox>();
            if (_hitbox != null)
            {
                _hitbox.Initialize(player.transform);
            }
            else
            {
                Debug.LogError("[PlayerCombat] SwordHitbox component not found in Player children! Please attach SwordHitbox to the Sword GameObject.");
            }
        }

        public void EnableDamage()
        {
            _hitbox?.EnableDamage(_config.AttackDamage);
        }

        public void DisableDamage()
        {
            _hitbox?.DisableDamage();
        }
    }
}
