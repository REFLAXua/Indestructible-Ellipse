using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float enemyDamage = 2f;
    private float attackRange;
    private bool hasDealtDamage = false;

    [SerializeField] private LayerMask playerLayer;

    void Start()
    {
        attackRange = GetComponent<EnemyMovement>().enemyAttackRange;
    }

    void Update()
    {

    }

    public void DealDamage() // called in Animation event ("Attack")
    {

        if (hasDealtDamage) 
        {
            return;
        }
        
        hasDealtDamage = true;


        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);
        HashSet<PlayerHealth> damagedPlayers = new HashSet<PlayerHealth>();

        foreach (var hitCollider in hits)
        {
            var doPlayerStun = GameObject.Find("Player").GetComponent<PlayerMovement>(); 
            var playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null && !damagedPlayers.Contains(playerHealth))
            {
                doPlayerStun.playerStun = true;
                playerHealth.TakeDamage(enemyDamage);
                damagedPlayers.Add(playerHealth);
            }
        }
    }

    public void ResetDamage()
    {
        hasDealtDamage = false;
    }
}
