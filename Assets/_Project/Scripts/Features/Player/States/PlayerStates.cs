using UnityEngine;

namespace Features.Player.States
{
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            Player.Velocity.x = 0;
            Player.Velocity.z = 0;
        }

        public override void LogicUpdate()
        {
            if (Player.InputService.IsAimPressed)
            {
                Player.RotateTowardsCamera();
            }

            if (Player.InputService.IsAttackPressed && Player.AttackCooldownTimer <= 0)
            {
                StateMachine.ChangeState(Player.AttackState);
                return;
            }

            if (Player.InputService.MoveInput != Vector2.zero)
            {
                StateMachine.ChangeState(Player.MoveState);
                return;
            }

            if (Player.InputService.IsJumpPressed && Player.CheckGround())
            {
                StateMachine.ChangeState(Player.JumpState);
            }
        }
    }

    public class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void LogicUpdate()
        {
            if (Player.InputService.MoveInput == Vector2.zero)
            {
                StateMachine.ChangeState(Player.IdleState);
                return;
            }

            if (Player.InputService.IsAttackPressed && Player.AttackCooldownTimer <= 0)
            {
                StateMachine.ChangeState(Player.AttackState);
                return;
            }

            if (Player.InputService.IsJumpPressed && Player.CheckGround())
            {
                StateMachine.ChangeState(Player.JumpState);
                return;
            }

            Move();
        }

        private void Move()
        {
            Vector2 input = Player.InputService.MoveInput;
            Vector3 moveDir = Player.GetMoveDirection(input);

            if (Player.InputService.IsAimPressed)
            {
                Player.RotateTowardsCamera();
            }
            else
            {
                Player.RotateTowardsMoveDirection(new Vector3(input.x, 0, input.y));
            }

            bool isSprinting = DetermineSprinting();
            float targetSpeed = isSprinting ? Config.SprintSpeed : Config.BaseSpeed;
            targetSpeed *= Player.SpeedMultiplier;

            Vector3 targetVelocity = moveDir * targetSpeed;
            Player.Velocity.x = targetVelocity.x;
            Player.Velocity.z = targetVelocity.z;
        }

        private bool DetermineSprinting()
        {
            if (!Player.InputService.IsSprintPressed) return false;
            if (Player.Stamina == null) return false;

            float staminaCost = Config.SprintStaminaCost * Time.deltaTime;
            if (Player.Stamina.CanConsume(staminaCost) && !Player.Stamina.IsExhausted)
            {
                Player.Stamina.Consume(staminaCost);
                return true;
            }
            return false;
        }
    }

    public class PlayerJumpState : PlayerState
    {
        public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            if (Player.Stamina != null && Player.Stamina.CanConsume(Config.JumpStaminaCost) && !Player.Stamina.IsExhausted)
            {
                Player.Stamina.Consume(Config.JumpStaminaCost);
                Player.Velocity.y = Config.JumpForce;
                StateMachine.ChangeState(Player.AirState);
            }
            else
            {
                StateMachine.ChangeState(Player.IdleState);
            }
        }
    }

    public class PlayerAirState : PlayerState
    {
        private float _targetSpeed;

        public PlayerAirState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            float currentHorizontalSpeed = new Vector3(Player.Velocity.x, 0, Player.Velocity.z).magnitude;
            _targetSpeed = Mathf.Max(currentHorizontalSpeed, Config.BaseSpeed);
        }

        public override void LogicUpdate()
        {
            Vector2 input = Player.InputService.MoveInput;

            HandleRotation(input);
            HandleAirMovement(input);
            CheckLanding();
        }

        private void HandleRotation(Vector2 input)
        {
            if (Player.InputService.IsAimPressed)
            {
                Player.RotateTowardsCamera();
            }
            else if (input.magnitude > 0.1f)
            {
                Player.RotateTowardsMoveDirection(new Vector3(input.x, 0, input.y));
            }
        }

        private void HandleAirMovement(Vector2 input)
        {
            if (input.magnitude <= 0.1f) return;

            Vector3 moveDir = Player.GetMoveDirection(input);
            Vector3 targetVelocity = moveDir * _targetSpeed;

            float airControl = Config.AirControlMultiplier;
            Player.Velocity.x = Mathf.Lerp(Player.Velocity.x, targetVelocity.x, Time.deltaTime * airControl);
            Player.Velocity.z = Mathf.Lerp(Player.Velocity.z, targetVelocity.z, Time.deltaTime * airControl);
        }

        private void CheckLanding()
        {
            if (Player.CheckGround() && Player.Velocity.y < 0)
            {
                StateMachine.ChangeState(Player.IdleState);
            }
        }
    }

    public class PlayerStunnedState : PlayerState
    {
        private float _stunTimer;
        private Vector3 _knockbackVelocity;
        private Core.Utils.BillBoard _billboard;

        public PlayerStunnedState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public void SetKnockback(Vector3 direction)
        {
            _knockbackVelocity = direction * Config.KnockbackForce;
        }

        public override void Enter()
        {
            _stunTimer = Config.StunDuration;

            Player.Velocity.x = _knockbackVelocity.x;
            Player.Velocity.z = _knockbackVelocity.z;

            Player.SetStunVisuals(true);

            var stunVisuals = Player.StunVisuals;
            if (stunVisuals != null)
            {
                _billboard = stunVisuals.GetComponent<Core.Utils.BillBoard>();
            }
        }

        public override void LogicUpdate()
        {
            _stunTimer -= Time.deltaTime;

            if (_billboard != null)
            {
                _billboard.ZRotation += Config.StunRotationSpeed * Time.deltaTime;
            }

            float dragSpeed = Config.KnockbackDragSpeed;
            Player.Velocity.x = Mathf.Lerp(Player.Velocity.x, 0, Time.deltaTime * dragSpeed);
            Player.Velocity.z = Mathf.Lerp(Player.Velocity.z, 0, Time.deltaTime * dragSpeed);

            if (_stunTimer <= 0)
            {
                StateMachine.ChangeState(Player.IdleState);
            }
        }

        public override void Exit()
        {
            Player.SetStunVisuals(false);
            if (_billboard != null)
            {
                _billboard.ZRotation = 0f;
            }
            _billboard = null;
        }
    }

    public class PlayerAttackState : PlayerState
    {
        private float _attackTimer;

        public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            Player.Velocity.x = 0;
            Player.Velocity.z = 0;
            Player.Animator.SetTrigger("Attack");
            Player.AttackCooldownTimer = Config.AttackCooldown;
            _attackTimer = Config.AttackDuration;
        }

        public override void LogicUpdate()
        {
            _attackTimer -= Time.deltaTime;

            Vector2 input = Player.InputService.MoveInput;

            HandleRotation(input);
            HandleSlowedMovement(input);

            if (_attackTimer <= 0)
            {
                StateMachine.ChangeState(Player.IdleState);
            }
        }

        private void HandleRotation(Vector2 input)
        {
            if (Player.InputService.IsAimPressed)
            {
                Player.RotateTowardsCamera();
            }
            else if (input.magnitude > 0.01f)
            {
                Player.RotateTowardsMoveDirection(new Vector3(input.x, 0, input.y));
            }
        }

        private void HandleSlowedMovement(Vector2 input)
        {
            if (input.magnitude > 0.01f)
            {
                Vector3 moveDir = Player.GetMoveDirection(input);
                float slowedMultiplier = Player.GetMovementMultiplierWithSlow(Config.AttackMoveMultiplier);
                float moveSpeed = Config.BaseSpeed * slowedMultiplier * Player.SpeedMultiplier;
                Vector3 targetVelocity = moveDir * moveSpeed;

                Player.Velocity.x = targetVelocity.x;
                Player.Velocity.z = targetVelocity.z;
            }
            else
            {
                Player.Velocity.x = 0;
                Player.Velocity.z = 0;
            }
        }
    }
}
