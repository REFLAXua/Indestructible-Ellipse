using UnityEngine;
using Cinemachine;
using Features.Player;
using Core;
using Core.Input;

namespace Features.Camera
{
    public class CameraController : MonoBehaviour
    {
        public CinemachineVirtualCamera virtualCamera;
        public Transform playerTransform;

        public float sensitivity = 2f;
        private float baseSensitivity;

        [Range(1f, 30f)] public float rotationSmoothTime = 10f;

        public float minVerticalAngle = -30f;
        public float maxVerticalAngle = 60f;

        private Cinemachine3rdPersonFollow followComponent;
        private PlayerController _playerController;
        private IInputService _inputService;

        private float targetYaw;
        private float targetPitch;
        private float currentYaw;
        private float currentPitch;

        void Start()
        {
            baseSensitivity = sensitivity;

            if (virtualCamera != null)
                followComponent = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    _playerController = player.GetComponent<PlayerController>();
                }
            }
            else
            {
                _playerController = playerTransform.GetComponent<PlayerController>();
            }

            if (ServiceLocator.TryGet(out IInputService input))
            {
                _inputService = input;
            }

            Vector3 angles = transform.rotation.eulerAngles;
            targetYaw = angles.y;
            targetPitch = angles.x;
            currentYaw = targetYaw;
            currentPitch = targetPitch;
        }

        void LateUpdate()
        {
            if (followComponent == null || playerTransform == null || _inputService == null)
            {
                return;
            }

            float mouseX = _inputService.LookInput.x * sensitivity;
            float mouseY = _inputService.LookInput.y * sensitivity;

            targetYaw += mouseX;
            targetPitch -= mouseY;
            targetPitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);

            currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * rotationSmoothTime);
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * rotationSmoothTime);

            transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

            // playerTransform.rotation = Quaternion.Euler(0f, currentYaw, 0f); // REMOVED: Conflicts with PlayerMoveState rotation

            UpdateFOV();
        }

        private void UpdateFOV()
        {
            if (_playerController == null) return;

            // Logic adapted to new system
            bool isSprinting = _inputService.IsSprintPressed && _playerController.Velocity.magnitude > 0.1f;
            bool isStunned = _playerController.StateMachine.CurrentState is Features.Player.States.PlayerStunnedState;

            if (isSprinting)
            {
                virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, 75f, Time.deltaTime * 5f);
            }
            else if (isStunned)
            {
                virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, 45f, Time.deltaTime * 5f);
            }
            else
            {
                virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, 60f, Time.deltaTime * 5f);
            }
        }
    }
}
