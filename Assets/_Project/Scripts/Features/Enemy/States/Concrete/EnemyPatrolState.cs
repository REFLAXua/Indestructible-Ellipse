using UnityEngine;

namespace Features.Enemy.States.Concrete
{
    public class EnemyPatrolState : EnemyStateBase
    {
        public EnemyPatrolState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            Controller.SetWalking(true);
            Vector3 randomPoint = GetRandomPoint(Controller.transform.position, Controller.Config.PatrolRadius);
            Controller.Mover.MoveTo(randomPoint, Controller.Config.PatrolSpeed);
        }

        public override void LogicUpdate()
        {
            if (Controller.Blackboard.Target != null)
            {
                StateMachine.ChangeState(Controller.ChaseState);
                return;
            }

            if (Controller.Mover.HasReachedDestination())
            {
                StateMachine.ChangeState(Controller.IdleState);
            }
        }

        private Vector3 GetRandomPoint(Vector3 center, float range)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out UnityEngine.AI.NavMeshHit hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
            return center;
        }
    }
}
