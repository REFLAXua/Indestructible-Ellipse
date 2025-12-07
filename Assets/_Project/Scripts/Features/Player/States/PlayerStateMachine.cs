using UnityEngine;

namespace Features.Player.States
{
    public abstract class PlayerState
    {
        protected readonly PlayerController Player;
        protected readonly PlayerStateMachine StateMachine;
        protected readonly Data.PlayerConfigSO Config;

        protected PlayerState(PlayerController player, PlayerStateMachine stateMachine)
        {
            Player = player;
            StateMachine = stateMachine;
            Config = player.Config;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }
    }

    public sealed class PlayerStateMachine
    {
        public PlayerState CurrentState { get; private set; }

        public void Initialize(PlayerState startingState)
        {
            CurrentState = startingState;
            CurrentState?.Enter();
        }

        public void ChangeState(PlayerState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }
    }
}
