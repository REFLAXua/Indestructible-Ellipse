using UnityEngine;
using Cinemachine;

public class CinemachineCameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public Transform playerTransform;

    public float sensitivity = 2f;
    private float baseSensitivity;

    [Range(1f, 30f)] public float rotationSmoothTime = 10f;

    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private Cinemachine3rdPersonFollow followComponent;
    [Tooltip("Put the PlayerMovement script")]
    [SerializeField] private PlayerMovement playerMovement;

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
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        Vector3 angles = transform.rotation.eulerAngles;
        targetYaw = angles.y;
        targetPitch = angles.x;
        currentYaw = targetYaw;
        currentPitch = targetPitch;
    }

    void LateUpdate()
    {
        if (followComponent == null || playerTransform == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        targetYaw += mouseX;
        targetPitch -= mouseY;
        targetPitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);

        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * rotationSmoothTime);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * rotationSmoothTime);

        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        playerTransform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        if (playerMovement.isPlayerSprinting)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, 75f, Time.deltaTime * 5f);
        }
        else if (playerMovement.playerStun)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, 45f, Time.deltaTime * 5f);
        }
        else
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, 60f, Time.deltaTime * 5f);
        }
    }

    public void SensitivitySlow()
    {
        sensitivity = 0.15f;
    }

    public void SensitivityNormilized()
    {
        sensitivity = baseSensitivity;
    }
}
