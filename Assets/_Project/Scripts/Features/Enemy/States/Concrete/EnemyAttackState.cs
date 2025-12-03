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
            
            if (Controller.Blackboard.Target != null)
            {
                Controller.transform.LookAt(Controller.Blackboard.Target);
            }

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
            if (Controller.Blackboard.Target != null)
            {
                float distance = Vector3.Distance(Controller.transform.position, Controller.Blackboard.Target.position);
                if (distance <= Controller.Config.AttackRange + 0.5f)
                {
                    var player = Controller.Blackboard.Target.GetComponent<Features.Player.PlayerController>();
                    if (player != null)
                    {
                        Vector3 knockbackDir = (player.transform.position - Controller.transform.position).normalized;
                        // Flatten knockback to avoid shooting player into the air excessively, unless intended
                        knockbackDir.y = 0.2f; 
                        knockbackDir.Normalize();

                        player.TakeDamage(Controller.Config.AttackDamage, knockbackDir);
                    }
                }
            }
        }

        public void FinishAttack()
        {
            StateMachine.ChangeState(Controller.ChaseState);
        }
    }
}
