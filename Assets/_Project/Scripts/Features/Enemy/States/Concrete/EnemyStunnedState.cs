using UnityEngine;

namespace Features.Enemy.States.Concrete
{
    public class EnemyStunnedState : EnemyStateBase
    {
        private float _stunTimer;

        public EnemyStunnedState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine)
        {
        }

        private Core.Utils.BillBoard _billboard;

        public override void Enter()
        {
            Controller.Mover.Stop();
            Controller.SetWalking(false);
            Controller.SetStunVisuals(true);
            _stunTimer = 0f;

            if (Controller.StunVisuals != null)
            {
                _billboard = Controller.StunVisuals.GetComponent<Core.Utils.BillBoard>();
            }
        }

        public override void LogicUpdate()
        {
            _stunTimer += Time.deltaTime;

            if (_billboard != null)
            {
                // Rotate 200 degrees per second
                _billboard.ZRotation += 200f * Time.deltaTime;
            }

            if (_stunTimer >= Controller.Config.StunDuration)
            {
                StateMachine.ChangeState(Controller.ChaseState);
            }
        }

        public override void Exit()
        {
            Controller.SetStunVisuals(false);
            if (_billboard != null)
            {
                _billboard.ZRotation = 0f;
            }
        }
    }
}
