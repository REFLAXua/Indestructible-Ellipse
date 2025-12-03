using Features.Enemy;

namespace Features.Enemy.States
{
    public abstract class EnemyStateBase : IEnemyState
    {
        protected readonly EnemyController Controller;
        protected readonly EnemyStateMachine StateMachine;

        protected EnemyStateBase(EnemyController controller, EnemyStateMachine stateMachine)
        {
            Controller = controller;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void LogicUpdate() { }
        public virtual void Exit() { }
    }
}
