using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    enum AIState
    {
        Idle, Patrolling, Chasing, Attacking
    }

    [SerializeField] private Transform wayPoints;
    [SerializeField] private float waitAtPoint = 2f;
    private int currentWaypoint;
    private float waitCounter;

    private Animator animator;
    NavMeshAgent agent;

    [SerializeField] private AIState currentState;

    [SerializeField] private float chaseRange = 10;

    [SerializeField] private float suspiciousTime;
    private float timeSinceLastSawPlayer;

    public float enemyAttackRange = 1.5f;
    public float attackTime = 2f;
    private float firstAttackTime = 0.1f;
    private float timeToAttack;
    private float attackStateTime = 0.5f;
    private float attackStateTimer;

    public float enemyStunTime = 0.5f;
    private float stunTimeTimer = 0f;
    private bool stun = false;

    public float knockBackForce = 5f;
    private bool isKnockedBack = false;
    private float knockBackDuration = 1f;
    private float knockBackTimer = 0f;
    private Vector3 knockBackDirection;

    public bool isDead = false;
    private GameObject player;
    private GameObject stunMark;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");
        stunMark = transform.Find("EffectUI/StunMark").gameObject;
        stunMark.SetActive(false);

        waitCounter = waitAtPoint;
        timeSinceLastSawPlayer = suspiciousTime;
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        switch (currentState)
        {
            case AIState.Idle:
                if (waitCounter > 0)
                {
                    waitCounter -= Time.deltaTime;
                }
                else
                {
                    currentState = AIState.Patrolling;
                    if (!isDead && agent.enabled && agent.isOnNavMesh)
                        agent.SetDestination(wayPoints.GetChild(currentWaypoint).position);
                }

                if (distanceToPlayer <= chaseRange)
                {
                    currentState = AIState.Chasing;
                }
                break;

            case AIState.Patrolling:
                if (agent.enabled && agent.isOnNavMesh && agent.remainingDistance <= 0.2f)
                {
                    currentWaypoint++;
                    if (currentWaypoint >= wayPoints.childCount)
                    {
                        currentWaypoint = 0;
                    }
                    currentState = AIState.Idle;
                    waitCounter = waitAtPoint;
                }

                if (distanceToPlayer <= chaseRange)
                {
                    currentState = AIState.Chasing;
                }
                break;

            case AIState.Chasing:
                if (!isDead && agent.enabled && agent.isOnNavMesh)
                {
                    if (agent.isStopped)
                        agent.isStopped = false;

                    agent.SetDestination(player.transform.position);
                }

                if (distanceToPlayer > chaseRange)
                {
                    timeSinceLastSawPlayer -= Time.deltaTime;

                    if (timeSinceLastSawPlayer <= 0)
                    {
                        currentState = AIState.Idle;
                        timeSinceLastSawPlayer = suspiciousTime;
                    }
                }
                else
                {
                    timeSinceLastSawPlayer = suspiciousTime;
                }

                if (distanceToPlayer <= enemyAttackRange)
                {
                    currentState = AIState.Attacking;
                    if (!isDead && agent.enabled && agent.isOnNavMesh)
                    {
                        agent.velocity = Vector3.zero;
                        agent.isStopped = true;
                    }
                }
                break;

            case AIState.Attacking:
                if (isDead)
                {
                    break;
                }

                if (agent.enabled)
                {
                    agent.enabled = false;
                }

                Vector3 direction = (player.transform.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
                }

                timeToAttack -= Time.deltaTime;

                if (timeToAttack <= 0 && !stun)
                {
                    animator.SetTrigger("Attack");
                    timeToAttack = attackTime;
                }

                if (distanceToPlayer > enemyAttackRange && !isDead)
                {
                    attackStateTimer -= Time.deltaTime;

                    if (attackStateTimer <= 0)
                    {
                        if (!agent.enabled)
                        agent.enabled = true;
                        attackStateTimer = attackStateTime;
                        timeToAttack = firstAttackTime;

                    currentState = AIState.Chasing;
                    }
                }
                break;
        }

        if (stun)
        {
            stunMark.SetActive(true);
            stunTimeTimer -= Time.deltaTime;
            if (!isDead && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
            if (stunTimeTimer <= 0)
            {
                stunMark.SetActive(false);
                stun = false;
                if (!isDead && agent.enabled && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
            }
        }

        if (isKnockedBack)
        {
            float t = knockBackTimer / knockBackDuration;
            transform.position += knockBackDirection * knockBackForce * t * Time.deltaTime;
            knockBackTimer -= Time.deltaTime;
            if (knockBackTimer <= 0)
            {
                isKnockedBack = false;
            }
        }
    }

    public void EnemyKnockBack() //called in PlayerMeleeAttack
    {
        Vector3 direction = (transform.position - player.transform.position);
        knockBackDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        isKnockedBack = true;
        knockBackTimer = knockBackDuration;
        stunTimeTimer = enemyStunTime;
        stun = true;
        
    }
}
