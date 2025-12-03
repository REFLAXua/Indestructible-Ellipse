using UnityEngine;

namespace Features.Enemy.States.Concrete
{
    public class EnemyChaseState : EnemyStateBase
    {
        public EnemyChaseState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            Controller.SetWalking(true);
        }

        public override void LogicUpdate()
        {
            if (Controller.Blackboard.Target == null)
            {
                StateMachine.ChangeState(Controller.IdleState);
                return;
            }

            float distance = Vector3.Distance(Controller.transform.position, Controller.Blackboard.Target.position);

            if (distance <= Controller.Config.AttackRange)
            {
                StateMachine.ChangeState(Controller.AttackState);
                return;
            }

            Controller.Mover.MoveTo(Controller.Blackboard.Target.position, Controller.Config.ChaseSpeed);
        }
    }
}
