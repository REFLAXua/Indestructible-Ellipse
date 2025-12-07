namespace Features.Enemy.States
{
    public interface IEnemyState
    {
        void Enter();
        void LogicUpdate();
        void Exit();
    }

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

    public sealed class EnemyStateMachine
    {
        public IEnemyState CurrentState { get; private set; }

        public void Initialize(IEnemyState startingState)
        {
            CurrentState = startingState;
            CurrentState?.Enter();
        }

        public void ChangeState(IEnemyState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }
    }
}
