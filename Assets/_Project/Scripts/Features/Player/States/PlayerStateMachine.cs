using UnityEngine;

namespace Features.Player.States
{
    public abstract class PlayerState
    {
        protected PlayerController _player;
        protected PlayerStateMachine _stateMachine;
        protected Data.PlayerConfigSO _config;

        public PlayerState(PlayerController player, PlayerStateMachine stateMachine)
        {
            _player = player;
            _stateMachine = stateMachine;
            _config = player.Config;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }
    }

    public class PlayerStateMachine
    {
        public PlayerState CurrentState { get; private set; }

        public void Initialize(PlayerState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        public void ChangeState(PlayerState newState)
        {
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}
