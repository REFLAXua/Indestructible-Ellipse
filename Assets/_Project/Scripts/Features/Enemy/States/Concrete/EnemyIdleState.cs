using UnityEngine;

namespace Features.Enemy.States.Concrete
{
    public class EnemyIdleState : EnemyStateBase
    {
        private float _idleTimer;

        public EnemyIdleState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            Controller.SetWalking(false);
            _idleTimer = 0f;
        }

        public override void LogicUpdate()
        {
            _idleTimer += Time.deltaTime;

            if (Controller.Blackboard.Target != null)
            {
                StateMachine.ChangeState(Controller.ChaseState);
                return;
            }

            if (_idleTimer > 2f)
            {
                StateMachine.ChangeState(Controller.PatrolState);
            }
        }
    }
}
