using UnityEngine;

namespace Features.Player.States
{
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            _player.Velocity.x = 0;
            _player.Velocity.z = 0;
        }

        public override void LogicUpdate()
        {
            if (_player.InputService.IsAimPressed)
            {
                _player.RotateTowardsCamera();
            }

            if (_player.InputService.IsAttackPressed && _player.AttackCooldownTimer <= 0)
            {
                _stateMachine.ChangeState(_player.AttackState);
                return;
            }

            if (_player.InputService.MoveInput != Vector2.zero)
            {
                _stateMachine.ChangeState(_player.MoveState);
            }

            if (_player.InputService.IsJumpPressed && _player.CheckGround())
            {
                _stateMachine.ChangeState(_player.JumpState);
            }
        }
    }

    public class PlayerMoveState : PlayerState
    {
        public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void LogicUpdate()
        {
            if (_player.InputService.MoveInput == Vector2.zero)
            {
                _stateMachine.ChangeState(_player.IdleState);
                return;
            }

            if (_player.InputService.IsAttackPressed && _player.AttackCooldownTimer <= 0)
            {
                _stateMachine.ChangeState(_player.AttackState);
                return;
            }

            if (_player.InputService.IsJumpPressed && _player.CheckGround())
            {
                _stateMachine.ChangeState(_player.JumpState);
                return;
            }

            Move();
        }

        private void Move()
        {
            Vector2 input = _player.InputService.MoveInput;
            
            // Get direction relative to camera
            Vector3 moveDir = _player.GetMoveDirection(input);

            // Rotate Player smoothly
            if (_player.InputService.IsAimPressed)
            {
                _player.RotateTowardsCamera();
            }
            else
            {
                _player.RotateTowardsMoveDirection(new Vector3(input.x, 0, input.y));
            }

            // Sprint Logic
            bool isSprinting = _player.InputService.IsSprintPressed;
            if (isSprinting && _player.Stamina != null)
            {
                if (_player.Stamina.CanConsume(_config.SprintStaminaCost * Time.deltaTime) && !_player.Stamina.IsExhausted)
                {
                    _player.Stamina.Consume(_config.SprintStaminaCost * Time.deltaTime);
                }
                else
                {
                    isSprinting = false;
                }
            }
            else if (isSprinting && _player.Stamina == null)
            {
                isSprinting = false;
            }

            float targetSpeed = isSprinting ? _config.SprintSpeed : _config.BaseSpeed;
            targetSpeed *= _player.SpeedMultiplier;

            // Apply movement velocity (keeping Y gravity separate in Controller)
            Vector3 targetVelocity = moveDir * targetSpeed;
            
            _player.Velocity.x = targetVelocity.x;
            _player.Velocity.z = targetVelocity.z;
        }
        

    }

    public class PlayerJumpState : PlayerState
    {
        public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            if (_player.Stamina != null && _player.Stamina.CanConsume(_config.JumpStaminaCost) && !_player.Stamina.IsExhausted)
            {
                _player.Stamina.Consume(_config.JumpStaminaCost);
                _player.Velocity.y = _config.JumpForce;
                _stateMachine.ChangeState(_player.AirState);
            }
            else
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }

    public class PlayerAirState : PlayerState
    {
        private float _targetSpeed;

        public PlayerAirState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            float currentHorizontalSpeed = new Vector3(_player.Velocity.x, 0, _player.Velocity.z).magnitude;
            _targetSpeed = Mathf.Max(currentHorizontalSpeed, _config.BaseSpeed);
        }

        public override void LogicUpdate()
        {
            // Air Control
            Vector2 input = _player.InputService.MoveInput;
            // Rotation Logic
            if (_player.InputService.IsAimPressed)
            {
                _player.RotateTowardsCamera();
            }
            else if (input.magnitude > 0.1f)
            {
                _player.RotateTowardsMoveDirection(new Vector3(input.x, 0, input.y));
            }

            // Movement Logic
            if (input.magnitude > 0.1f)
            {
                 Vector3 moveDir = _player.GetMoveDirection(input);

                 // Use the captured target speed to maintain momentum
                 Vector3 targetVelocity = moveDir * _targetSpeed;
                 
                 // Smoothly interpolate air movement
                 _player.Velocity.x = Mathf.Lerp(_player.Velocity.x, targetVelocity.x, Time.deltaTime * 5f);
                 _player.Velocity.z = Mathf.Lerp(_player.Velocity.z, targetVelocity.z, Time.deltaTime * 5f);
            }

            if (_player.CheckGround() && _player.Velocity.y < 0)
            {
                _stateMachine.ChangeState(_player.IdleState);
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
            _knockbackVelocity = direction * _config.KnockbackForce;
        }

        public override void Enter()
        {
            _stunTimer = _config.StunDuration;
            
            // Apply initial knockback velocity
            _player.Velocity.x = _knockbackVelocity.x;
            _player.Velocity.z = _knockbackVelocity.z;
            
            // Enable Stun Visuals
            _player.SetStunVisuals(true);

            if (_player.StunVisuals != null)
            {
                _billboard = _player.StunVisuals.GetComponent<Core.Utils.BillBoard>();
            }
        }

        public override void LogicUpdate()
        {
            _stunTimer -= Time.deltaTime;
            
            if (_billboard != null)
            {
                // Rotate 200 degrees per second
                _billboard.ZRotation += 200f * Time.deltaTime;
            }

            // Apply friction/drag to knockback to simulate weight
            _player.Velocity.x = Mathf.Lerp(_player.Velocity.x, 0, Time.deltaTime * 5f);
            _player.Velocity.z = Mathf.Lerp(_player.Velocity.z, 0, Time.deltaTime * 5f);

            if (_stunTimer <= 0)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }

        public override void Exit()
        {
            _player.SetStunVisuals(false);
            if (_billboard != null)
            {
                _billboard.ZRotation = 0f;
            }
        }
    }

    public class PlayerAttackState : PlayerState
    {
        private float _attackTimer;
        private float _attackDuration = 0.5f;

        public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            _player.Velocity.x = 0;
            _player.Velocity.z = 0;
            _player.Animator.SetTrigger("Attack");
            _player.AttackCooldownTimer = _config.AttackCooldown;
            _attackTimer = _attackDuration;
        }

        public override void LogicUpdate()
        {
            _attackTimer -= Time.deltaTime;

            Vector2 input = _player.InputService.MoveInput;
            
            // Rotation with built-in slow
            if (_player.InputService.IsAimPressed)
            {
                _player.RotateTowardsCamera();
            }
            else if (input.magnitude > 0.01f)
            {
                _player.RotateTowardsMoveDirection(new Vector3(input.x, 0, input.y));
            }

            // Movement with centralized slow multiplier
            if (input.magnitude > 0.01f)
            {
                Vector3 moveDir = _player.GetMoveDirection(input);
                float baseAttackMultiplier = 0.4f;
                float slowedMultiplier = _player.GetMovementMultiplierWithSlow(baseAttackMultiplier);
                float moveSpeed = _config.BaseSpeed * slowedMultiplier * _player.SpeedMultiplier;
                Vector3 targetVelocity = moveDir * moveSpeed;

                _player.Velocity.x = targetVelocity.x;
                _player.Velocity.z = targetVelocity.z;
            }
            else
            {
                _player.Velocity.x = 0;
                _player.Velocity.z = 0;
            }

            if (_attackTimer <= 0)
            {
                _stateMachine.ChangeState(_player.IdleState);
            }
        }
    }
}
