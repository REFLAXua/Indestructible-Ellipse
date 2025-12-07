using Features.Enemy.Data;
using UnityEngine;

namespace Features.Enemy
{
    public class EnemyBlackboard
    {
        public EnemyConfigSO Config { get; private set; }
        public Transform Target { get; set; }
        public Vector3 StartPosition { get; set; }
        public bool CanAttack { get; set; }
        public float LastAttackTime { get; set; }

        public EnemyBlackboard(EnemyConfigSO config)
        {
            Config = config;
            Target = null;
            CanAttack = false;
            LastAttackTime = -999f;
        }
    }
}
