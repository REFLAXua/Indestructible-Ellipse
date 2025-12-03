using UnityEngine;

namespace Features.Enemy.States.Concrete
{
    public class EnemyDeadState : EnemyStateBase
    {
        private float _dieTime;
        private bool _hasRotated;

        public EnemyDeadState(EnemyController controller, EnemyStateMachine stateMachine) : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            Controller.Mover.Stop();
            Controller.SetWalking(false);
            Controller.Animator.ResetTrigger("Attack"); // Force stop attack
            Controller.TriggerDieAnimation();
            
            var collider = Controller.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            var agent = Controller.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            var canvas = Controller.GetComponentInChildren<Canvas>();
            if (canvas != null) canvas.enabled = false;

            _dieTime = Controller.Config.DeathDuration;
            _hasRotated = false;
        }

        public override void LogicUpdate()
        {
            _dieTime -= Time.deltaTime;

            // Stop animator before sinking to freeze the pose (stop breathing)
            if (_dieTime < Controller.Config.DeathDuration && _dieTime <= Controller.Config.SinkDelay + 0.5f) // Small buffer
            {
                 if (Controller.Animator.enabled) Controller.Animator.enabled = false;
            }

            if (!_hasRotated && Controller.Blackboard.Target != null)
            {
                Vector3 directionToPlayer = (Controller.Blackboard.Target.position - Controller.transform.position).normalized;
                directionToPlayer.y = 0f;
                if (directionToPlayer != Vector3.zero)
                {
                     Quaternion lookAwayRotation = Quaternion.LookRotation(directionToPlayer);
                     Controller.transform.rotation = lookAwayRotation;
                }
                _hasRotated = true;
            }

            // Phase 1: Initial Fall/Sink
            if (_dieTime < Controller.Config.DeathDuration && _dieTime >= Controller.Config.SinkDelay)
            {
                Quaternion currentRotation = Controller.transform.rotation;
                Vector3 euler = currentRotation.eulerAngles;
                Quaternion targetRotation = Quaternion.Euler(-90f, euler.y, euler.z);
                
                // Use Config.DeathRotationSpeed
                Controller.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * Controller.Config.DeathRotationSpeed);
                
                Vector3 dieTargetVector1 = new Vector3(Controller.transform.position.x, Controller.transform.position.y - 1, Controller.transform.position.z);
                Controller.transform.position = Vector3.Lerp(Controller.transform.position, dieTargetVector1, Time.deltaTime * Controller.Config.SinkSpeed);
            }

            // Phase 2: Final Sink
            if (_dieTime <= 2f)
            {
                Vector3 dieTargetVector2 = new Vector3(Controller.transform.position.x, Controller.transform.position.y - 1, Controller.transform.position.z);
                Controller.transform.position = Vector3.Lerp(Controller.transform.position, dieTargetVector2, Time.deltaTime * Controller.Config.FinalSinkSpeed);
            }

            if (_dieTime <= 0)
            {
                GameObject.Destroy(Controller.gameObject);
            }
        }
    }
}
