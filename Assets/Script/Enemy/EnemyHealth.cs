using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float enemyHealth = 10f;
    private float dieTime = 7f;
    private Transform player;
    private bool hasRotated = false;
    Animator animator;

    UnityEngine.AI.NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void EnemyTakeDamage(float damage)
    {
        enemyHealth -= 1;
    }
    void Update()
    {
        if (enemyHealth <= 0 )
        {
            dieTime -= Time.deltaTime;
            EnemyDie();
        }
    }

    void EnemyDie()
    {
        GetComponent<EnemyMovement>().isDead = true;
        agent.enabled = false;

        animator.SetTrigger("isDead");

        if (!hasRotated && player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0f;
            if (directionToPlayer != Vector3.zero)
            {
                 Quaternion lookAwayRotation = Quaternion.LookRotation(directionToPlayer);
                 transform.rotation = lookAwayRotation;
            }
            hasRotated = true;
        }

        if (dieTime < 7f && dieTime >= 6.5f)
        {
            Quaternion currentRotation = transform.rotation;
            Vector3 euler = currentRotation.eulerAngles;
            Quaternion targetRotation = Quaternion.Euler(-90f, euler.y, euler.z);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 7f);
            Vector3 dieTargetVector1 = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, dieTargetVector1, Time.deltaTime * 2f);
        }

        if (dieTime <= 2f)
        {
            Vector3 dieTargetVector2 = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, dieTargetVector2, Time.deltaTime *1.2f);
        }

        if (dieTime <= 0)
        {
            Destroy(gameObject);
        }
    }

}

