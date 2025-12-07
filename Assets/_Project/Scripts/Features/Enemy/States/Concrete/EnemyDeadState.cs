using UnityEngine;

namespace Features.Enemy.States.Concrete
{
    public class EnemyDeadState : EnemyStateBase
    {
        private float _dieTime;
        private bool _hasRotated;
        private bool _animatorDisabled;

        public EnemyDeadState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            Controller.Mover.Stop();
            Controller.SetWalking(false);
            
            if (Controller.Animator != null)
            {
                Controller.Animator.ResetTrigger("Attack");
            }
            Controller.TriggerDieAnimation();

            DisableComponents();

            _dieTime = Controller.Config.DeathDuration;
            _hasRotated = false;
            _animatorDisabled = false;
        }

        private void DisableComponents()
        {
            var collider = Controller.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            var agent = Controller.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            var canvas = Controller.GetComponentInChildren<Canvas>();
            if (canvas != null) canvas.enabled = false;
        }

        public override void LogicUpdate()
        {
            _dieTime -= Time.deltaTime;

            TryDisableAnimator();
            TryRotateTowardsPlayer();
            HandleSinking();
            CheckDestroy();
        }

        private void TryDisableAnimator()
        {
            if (_animatorDisabled) return;

            float disableThreshold = Controller.Config.SinkDelay + 0.5f;
            if (_dieTime < Controller.Config.DeathDuration && _dieTime <= disableThreshold)
            {
                if (Controller.Animator != null && Controller.Animator.enabled)
                {
                    Controller.Animator.enabled = false;
                    _animatorDisabled = true;
                }
            }
        }

        private void TryRotateTowardsPlayer()
        {
            if (_hasRotated) return;
            if (Controller.Blackboard.Target == null) return;

            Vector3 directionToPlayer = (Controller.Blackboard.Target.position - Controller.transform.position).normalized;
            directionToPlayer.y = 0f;
            
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion lookAwayRotation = Quaternion.LookRotation(directionToPlayer);
                Controller.transform.rotation = lookAwayRotation;
            }
            _hasRotated = true;
        }

        private void HandleSinking()
        {
            float config_DeathDuration = Controller.Config.DeathDuration;
            float config_SinkDelay = Controller.Config.SinkDelay;
            float config_DeathRotationSpeed = Controller.Config.DeathRotationSpeed;
            float config_SinkSpeed = Controller.Config.SinkSpeed;
            float config_FinalSinkSpeed = Controller.Config.FinalSinkSpeed;
            float config_FinalPhase = Controller.Config.FinalSinkPhaseTime;

            if (_dieTime < config_DeathDuration && _dieTime >= config_SinkDelay)
            {
                Quaternion currentRotation = Controller.transform.rotation;
                Vector3 euler = currentRotation.eulerAngles;
                Quaternion targetRotation = Quaternion.Euler(-90f, euler.y, euler.z);

                Controller.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * config_DeathRotationSpeed);

                Vector3 pos = Controller.transform.position;
                Vector3 targetPos = new Vector3(pos.x, pos.y - 1, pos.z);
                Controller.transform.position = Vector3.Lerp(pos, targetPos, Time.deltaTime * config_SinkSpeed);
            }

            if (_dieTime <= config_FinalPhase)
            {
                Vector3 pos = Controller.transform.position;
                Vector3 targetPos = new Vector3(pos.x, pos.y - 1, pos.z);
                Controller.transform.position = Vector3.Lerp(pos, targetPos, Time.deltaTime * config_FinalSinkSpeed);
            }
        }

        private void CheckDestroy()
        {
            if (_dieTime <= 0)
            {
                GameObject.Destroy(Controller.gameObject);
            }
        }
    }
}
