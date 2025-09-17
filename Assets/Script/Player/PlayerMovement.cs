using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public bool isOnGround = false;

    public float baseSpeed = 5f;
    public float sprintSpeed = 7f;
    private float currentSpeed;

    public float jumpForce = 5f;
    public float jumpTime = 0.2f;
    private float jumpTimer;
    private bool isInAir = false;
    public bool hasJumped = false;

    public bool isPlayerSprinting = false;
    public bool isPlayerJumping = false;
    public bool noStamina = false;
    public bool consumingStamina = false;

    private PlayerStamina playerStamina;

    public float playerStunTime = 1f;
    private float playerStunTimeTimer = 0f;
    public bool playerStun = false;
    private float stunSensitivity;
    public float applyStunSensitivity = 0.05f;
    private float baseSensitivity;

    private PlayerCamera playerCamera;

    public float gravity = -9.81f;
    private CharacterController controller;
    private Vector3 velocity;

    private GameObject stunMark;

    private void Start()
    {
        playerStamina = GetComponent<PlayerStamina>();
        playerCamera = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
        stunSensitivity = playerCamera.sensitivity;
        baseSensitivity = stunSensitivity;
        playerStunTimeTimer = playerStunTime;
        controller = GetComponent<CharacterController>();
        currentSpeed = baseSpeed;
        stunMark = transform.Find("EffectUI/StunMark").gameObject;
        stunMark.SetActive(false);
    }

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Sprint
        if (!isSlowed && !isInAir)
        {
            if (Input.GetKey(KeyCode.LeftShift) && !noStamina && !playerStamina.isExhausted && move.magnitude > 0)
            {
                consumingStamina = true;
                currentSpeed = sprintSpeed;
                isPlayerSprinting = true;
            }
            else
            {
                consumingStamina = false;
                currentSpeed = baseSpeed;
                isPlayerSprinting = false;
            }
        }
        
        if (!playerStun)
        {
        controller.Move(move * currentSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        isOnGround = controller.isGrounded;

        if (isOnGround)
        {
            isInAir = false;
            hasJumped = false;
        }
        // Jump
        if (isOnGround && Input.GetButton("Jump") && !playerStun && playerStamina.currentStamina > 15)
        {
            hasJumped = true;
            isInAir = true;
            jumpTimer = jumpTime;
            velocity.y = jumpForce;
        }

        if (jumpTimer > 0)
            jumpTimer -= Time.deltaTime;
        else
            velocity.y += gravity * Time.deltaTime;

        //Stun
        if (playerStun)
        {
            stunMark.SetActive(true);
            stunSensitivity = applyStunSensitivity;
            playerCamera.sensitivity = stunSensitivity;
            var attackStun = GameObject.Find("Player").GetComponent<PlayerMeleeAttack>();
            attackStun.playerStunAttack = true;
            playerStunTimeTimer -= Time.deltaTime;
            if (playerStunTimeTimer <= 0)
            {
                stunMark.SetActive(false);
                playerStunTimeTimer = playerStunTime;
                playerStun = false;
                attackStun.playerStunAttack = false;
                stunSensitivity = baseSensitivity;
                playerCamera.sensitivity = stunSensitivity;
            }
        }
    }

    //Slowing 
    private bool isSlowed = false;

    public void ApplySlow(float slowAmount, float duration)
    {
        if (!isSlowed)
            StartCoroutine(SlowRoutine(slowAmount, duration));
    }

    private IEnumerator SlowRoutine(float slowAmount, float duration)
    {
        isSlowed = true;
        float originalSpeed = currentSpeed;
        currentSpeed -= slowAmount;

        yield return new WaitForSeconds(duration);

        isSlowed = false;
        currentSpeed = baseSpeed;
    }

}
