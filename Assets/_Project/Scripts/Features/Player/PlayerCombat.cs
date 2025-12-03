using UnityEngine;
using System.Collections.Generic;
using Features.Player.Data;
using Features.Enemy;

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
            
            // Try to find the hitbox on the weapon (child object)
            _hitbox = player.GetComponentInChildren<SwordHitbox>();
            if (_hitbox != null)
            {
                _hitbox.Initialize(player.transform);
            }
            else
            {
                Debug.LogError("SwordHitbox component not found in Player children! Please attach SwordHitbox to the Sword GameObject.");
            }
        }

        public void EnableDamage()
        {
            if (_hitbox != null)
            {
                _hitbox.EnableDamage(_config.AttackDamage);
            }
        }

        public void DisableDamage()
        {
            if (_hitbox != null)
            {
                _hitbox.DisableDamage();
            }
        }

        // Update is no longer needed for hit detection
        private void Update()
        {
        }

        // Gizmos no longer needed as we use physical collider
        private void OnDrawGizmosSelected()
        {
        }
    }
}
