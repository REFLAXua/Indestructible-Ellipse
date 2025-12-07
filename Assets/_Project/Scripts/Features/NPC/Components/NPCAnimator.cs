using UnityEngine;

namespace Features.NPC.Components
{
    [RequireComponent(typeof(NPCController))]
    public class NPCAnimator : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [SerializeField] private string _idleAnimationName = "Idle";
        [SerializeField] private string _talkAnimationName = "Talk";
        [SerializeField] private string _interactTrigger = "Interact";
        
        [Header("Look At Settings")]
        [SerializeField] private bool _enableLookAt = true;
        [SerializeField] private float _lookAtWeight = 0.5f;
        [SerializeField] private float _lookAtSmoothTime = 0.3f;

        private NPCController _npcController;
        private Animator _animator;
        private Transform _lookAtTarget;
        private float _currentLookAtWeight;
        private int _interactTriggerId;

        private void Awake()
        {
            _npcController = GetComponent<NPCController>();
            _animator = GetComponent<Animator>();
            
            if (_animator != null)
            {
                _interactTriggerId = Animator.StringToHash(_interactTrigger);
            }
        }

        private void OnEnable()
        {
            Core.EventBus.Subscribe<Events.NPCInteractionStartedEvent>(OnInteractionStarted);
            Core.EventBus.Subscribe<Events.NPCInteractionEndedEvent>(OnInteractionEnded);
            Core.EventBus.Subscribe<Events.PlayerEnteredNPCRangeEvent>(OnPlayerEntered);
            Core.EventBus.Subscribe<Events.PlayerExitedNPCRangeEvent>(OnPlayerExited);
        }

        private void OnDisable()
        {
            Core.EventBus.Unsubscribe<Events.NPCInteractionStartedEvent>(OnInteractionStarted);
            Core.EventBus.Unsubscribe<Events.NPCInteractionEndedEvent>(OnInteractionEnded);
            Core.EventBus.Unsubscribe<Events.PlayerEnteredNPCRangeEvent>(OnPlayerEntered);
            Core.EventBus.Unsubscribe<Events.PlayerExitedNPCRangeEvent>(OnPlayerExited);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_enableLookAt || _animator == null || _lookAtTarget == null) return;

            _currentLookAtWeight = Mathf.MoveTowards(
                _currentLookAtWeight, 
                _lookAtWeight, 
                _lookAtSmoothTime * Time.deltaTime
            );

            _animator.SetLookAtWeight(_currentLookAtWeight);
            _animator.SetLookAtPosition(_lookAtTarget.position + Vector3.up * 1.5f);
        }

        private void OnInteractionStarted(Events.NPCInteractionStartedEvent evt)
        {
            if (evt.NPC != _npcController) return;

            if (_animator != null)
            {
                _animator.SetTrigger(_interactTriggerId);
            }
        }

        private void OnInteractionEnded(Events.NPCInteractionEndedEvent evt)
        {
            if (evt.NPC != _npcController) return;

            _lookAtTarget = null;
            _currentLookAtWeight = 0f;
        }

        private void OnPlayerEntered(Events.PlayerEnteredNPCRangeEvent evt)
        {
            if (evt.NPC != _npcController) return;

            if (_npcController.PlayerTransform != null)
            {
                _lookAtTarget = _npcController.PlayerTransform;
            }
        }

        private void OnPlayerExited(Events.PlayerExitedNPCRangeEvent evt)
        {
            if (evt.NPC != _npcController) return;

            _lookAtTarget = null;
        }

        public void PlayAnimation(string animationName)
        {
            if (_animator != null)
            {
                _animator.Play(animationName);
            }
        }

        public void SetTrigger(string triggerName)
        {
            if (_animator != null)
            {
                _animator.SetTrigger(triggerName);
            }
        }
    }
}
