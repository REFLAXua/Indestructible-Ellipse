namespace Features.Enemy.States
{
    public interface IEnemyState
    {
        void Enter();
        void LogicUpdate();
        void Exit();
    }
}
