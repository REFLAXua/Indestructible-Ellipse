using UnityEngine;
using Features.Enemy.Data;

namespace Features.Enemy.Data // Placing in Data namespace as it holds data
{
    public class EnemyBlackboard
    {
        public EnemyConfigSO Config { get; private set; }
        public Transform Target { get; set; }
        public bool CanAttack { get; set; }
        public Vector3 StartPosition { get; set; }

        public EnemyBlackboard(EnemyConfigSO config)
        {
            Config = config;
        }
    }
}
