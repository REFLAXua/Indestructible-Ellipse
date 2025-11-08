//associated with PlayerMovement.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    public float maxStamina = 100f;
    public float staminaRegenRate = 3f;
    public float staminaSprintConsume = 10f;
    public float staminaJumpConsume = 15f;
    public bool isExhausted = false;

    [SerializeField] private float _currentStamina;
    public float currentStamina
    {
        get { return _currentStamina; }
        set { _currentStamina = Mathf.Clamp(value, 0, maxStamina); }
    }

    [Tooltip("Put the PlayerMovement script")]
    [SerializeField] private PlayerMovement playerMovement;
    private bool isRegenerating = false;
    private Coroutine regenCoroutine = null;

    void Start()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        // Exhaustion State
        if(playerMovement.noStamina)
        {
            isExhausted = true;
        }
        else if (currentStamina >= 15 && isExhausted)
        {
            isExhausted = false;
        }

        // Regenerate Stamina
        if (!playerMovement.consumingStamina && currentStamina < maxStamina && !isRegenerating)
        {
            regenCoroutine = StartCoroutine(RegenerateStamina());
        }

        // Sprint
        if (playerMovement.isPlayerSprinting && currentStamina > 0 && !playerMovement.playerStun && playerMovement.isOnGround)
        {
            currentStamina -= staminaSprintConsume * Time.deltaTime;
            if (currentStamina < 0f)
            {
                currentStamina = 0f;
            }
        }
        // Jump
        if (playerMovement.hasJumped && currentStamina > 15)
        {
            currentStamina -= staminaJumpConsume;
            if (currentStamina < 0f)
            {
                currentStamina = 0f;
            }
            playerMovement.hasJumped = false; 

            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                isRegenerating = false;
                regenCoroutine = null;
            }
        }
        else if (currentStamina <= 0)
        {
            playerMovement.noStamina = true;
        }

        if (currentStamina > 0)
        {
            playerMovement.noStamina = false;
        }

        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
    }

    IEnumerator RegenerateStamina()
    {
        isRegenerating = true;
        yield return new WaitForSeconds(2f);
        while (!playerMovement.consumingStamina && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            yield return null;
        }
        isRegenerating = false;
        regenCoroutine = null;
    }
}
