using UnityEngine;

namespace Features.Enemy.States.Concrete
{
    public class EnemyStunnedState : EnemyStateBase
    {
        private float _stunTimer;
        private Core.Utils.BillBoard _billboard;

        public EnemyStunnedState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            Controller.Mover.Stop();
            Controller.SetWalking(false);
            Controller.SetStunVisuals(true);
            _stunTimer = 0f;

            var stunVisuals = Controller.StunVisuals;
            if (stunVisuals != null)
            {
                _billboard = stunVisuals.GetComponent<Core.Utils.BillBoard>();
            }
        }

        public override void LogicUpdate()
        {
            _stunTimer += Time.deltaTime;

            if (_billboard != null)
            {
                _billboard.ZRotation += Controller.Config.StunRotationSpeed * Time.deltaTime;
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
            _billboard = null;
        }
    }
}
