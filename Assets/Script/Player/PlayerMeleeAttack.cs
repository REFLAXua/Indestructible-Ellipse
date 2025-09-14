using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    public float attackCooldown = 1.5f;
    public float damage = 1f;

    private float attackCooldownTimer = 0f;
    private bool canDealDamage = false;

    public float slowAmount = 4f;
    public float slowDuration = 1.5f;
    public bool playerStunAttack = false;

    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    private Animator animator;
    private PlayerMovement playerMovement;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && attackCooldownTimer <= 0 && !playerStunAttack)
        {
            animator.SetTrigger("Attack");

            attackCooldownTimer = attackCooldown;

            if (playerMovement != null)
                playerMovement.ApplySlow(slowAmount, slowDuration);
        }
    }

    public void EnableDamage()
    {
        canDealDamage = true;
        hitEnemies.Clear();
    }

    public void DisableDamage()
    {
        canDealDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;
        if (!other.CompareTag("Enemy")) return;
        if (hitEnemies.Contains(other.gameObject)) return;

        hitEnemies.Add(other.gameObject);

        var health = other.GetComponent<EnemyHealth>();
        if (health != null)
            health.EnemyTakeDamage(damage);

        var enemyScript = other.GetComponent<EnemyMovement>();
        if (enemyScript != null)
            enemyScript.EnemyKnockBack();
    }
}
