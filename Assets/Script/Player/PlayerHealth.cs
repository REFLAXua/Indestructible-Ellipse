using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health = 20f;

    void Start()
    {
        
    }

    void Update()
    {
        if (health <= 0 )
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float enemyDamage)
    {
        health -= enemyDamage;
    }
}
