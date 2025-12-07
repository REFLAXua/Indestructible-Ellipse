using UnityEngine;

namespace Features.Enemy.States.Concrete
{
    public class EnemyAttackState : EnemyStateBase
    {
        private float _lastAttackTime;

        public EnemyAttackState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            Controller.Mover.Stop();
            Controller.SetWalking(false);

            RotateTowardsTargetHorizontal();

            if (Time.time >= _lastAttackTime + Controller.Config.AttackCooldown)
            {
                Controller.TriggerAttackAnimation();
                _lastAttackTime = Time.time;
            }
            else
            {
                StateMachine.ChangeState(Controller.ChaseState);
            }
        }

        public override void LogicUpdate()
        {
            if (Controller.Blackboard.Target == null)
            {
                StateMachine.ChangeState(Controller.IdleState);
            }
        }

        public void TriggerDamage()
        {
            if (Controller.Blackboard.Target == null) return;

            float distance = Vector3.Distance(Controller.transform.position, Controller.Blackboard.Target.position);
            float effectiveRange = Controller.Config.AttackRange + Controller.Config.AttackRangeTolerance;

            if (distance <= effectiveRange)
            {
                var player = Controller.Blackboard.Target.GetComponent<Features.Player.PlayerController>();
                if (player != null)
                {
                    Vector3 knockbackDir = (player.transform.position - Controller.transform.position).normalized;
                    knockbackDir.y = 0.2f;
                    knockbackDir.Normalize();

                    player.TakeDamage(Controller.Config.AttackDamage, knockbackDir);
                }
            }
        }

        public void FinishAttack()
        {
            StateMachine.ChangeState(Controller.ChaseState);
        }

        private void RotateTowardsTargetHorizontal()
        {
            if (Controller.Blackboard.Target == null) return;

            Vector3 direction = Controller.Blackboard.Target.position - Controller.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Controller.transform.rotation = targetRotation;
            }
        }
    }
}
