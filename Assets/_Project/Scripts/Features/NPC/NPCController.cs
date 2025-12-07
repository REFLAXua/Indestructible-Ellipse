using UnityEngine;
using Core;
using Features.NPC.Interfaces;
using Features.NPC.Data;
using Features.NPC.Events;

namespace Features.NPC
{
    [RequireComponent(typeof(SphereCollider))]
    public class NPCController : MonoBehaviour, INPCInteractable
    {
        [Header("Configuration")]
        [SerializeField] private NPCConfigSO _config;
        
        [Header("Interaction Handler")]
        [SerializeField] private NPCInteractionHandlerBase _interactionHandler;

        private SphereCollider _detectionCollider;
        private Transform _playerTransform;
        private bool _isPlayerInRange;
        private bool _isInteracting;

        public string InteractionPrompt => _config != null ? _config.InteractionPrompt : "Interact";
        public bool CanInteract => _isPlayerInRange && !_isInteracting && IsInteractionAllowed();
        public NPCType NPCType => _config != null ? _config.NPCType : NPCType.Generic;
        public string NPCName => _config != null ? _config.NPCName : gameObject.name;
        public NPCConfigSO Config => _config;
        public Transform PlayerTransform => _playerTransform;
        public bool IsInteracting => _isInteracting;

        private void Awake()
        {
            ValidateConfiguration();
            SetupDetectionCollider();
        }

        private void ValidateConfiguration()
        {
            if (_config == null)
            {
                Debug.LogError($"[NPCController] {gameObject.name} має відсутню конфігурацію NPCConfigSO!");
            }

            if (_interactionHandler == null)
            {
                Debug.LogWarning($"[NPCController] {gameObject.name} не має призначеного обробника взаємодії.");
            }
        }

        private void SetupDetectionCollider()
        {
            _detectionCollider = GetComponent<SphereCollider>();
            
            if (_detectionCollider == null)
            {
                _detectionCollider = gameObject.AddComponent<SphereCollider>();
            }

            _detectionCollider.isTrigger = true;
            _detectionCollider.radius = _config != null ? _config.InteractionRadius : 3f;
        }

        private void Update()
        {
            if (!_isPlayerInRange) return;

            HandleInteractionInput();
            
            if (_config != null && _config.FacePlayerOnInteract && _isInteracting)
            {
                RotateTowardsPlayer();
            }
        }

        private void HandleInteractionInput()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.E) && CanInteract)
            {
                Interact();
            }
        }

        private void RotateTowardsPlayer()
        {
            if (_playerTransform == null) return;

            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    _config.RotationSpeed * Time.deltaTime
                );
            }
        }

        private bool IsInteractionAllowed()
        {
            if (_config == null) return true;
            
            if (!_config.CanInteractDuringCombat)
            {
                return true;
            }

            return true;
        }

        public void Interact()
        {
            if (!CanInteract) return;

            _isInteracting = true;
            
            Debug.Log($"[NPCController] Взаємодія з {_config?.NPCName ?? gameObject.name}");

            if (_interactionHandler != null)
            {
                _interactionHandler.OnInteractionStart(this);
                _interactionHandler.Execute(this);
            }

            EventBus.Publish(new NPCInteractionStartedEvent
            {
                NPC = this,
                NPCName = NPCName,
                NPCType = NPCType
            });
        }

        public void EndInteraction()
        {
            if (!_isInteracting) return;

            _isInteracting = false;

            if (_interactionHandler != null)
            {
                _interactionHandler.OnInteractionEnd(this);
            }

            EventBus.Publish(new NPCInteractionEndedEvent { NPC = this });
        }

        public void OnPlayerEnterRange()
        {
            if (_config != null && _config.ShowFloatingPrompt)
            {
                EventBus.Publish(new PlayerEnteredNPCRangeEvent
                {
                    NPC = this,
                    NPCName = NPCName,
                    InteractionPrompt = InteractionPrompt,
                    NPCType = NPCType
                });
            }
        }

        public void OnPlayerExitRange()
        {
            if (_isInteracting)
            {
                EndInteraction();
            }

            EventBus.Publish(new PlayerExitedNPCRangeEvent { NPC = this });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_config == null) return;

            if (((1 << other.gameObject.layer) & _config.PlayerLayer) != 0)
            {
                _isPlayerInRange = true;
                _playerTransform = other.transform;
                OnPlayerEnterRange();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_config == null) return;

            if (((1 << other.gameObject.layer) & _config.PlayerLayer) != 0)
            {
                _isPlayerInRange = false;
                _playerTransform = null;
                OnPlayerExitRange();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
            float radius = _config != null ? _config.InteractionRadius : 3f;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
