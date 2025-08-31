using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Processors;
using UnityEngine.Splines;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : LivingEntity
{
    private static readonly int hashMove = Animator.StringToHash("Move");
    private static readonly int hashDeath = Animator.StringToHash("Death");
    private const string playerTag = "Player";
    private const string gameManagerTag = "GameController";

    [Header("Enemy Stats")]
    public float moveSpeed;
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;

    [Header("Detection")]
    public float detectionRange;

    [Header("Detection")]
    public ParticleSystem hitParticle;

    protected NavMeshAgent navAgent;
    protected Animator animator;
    protected Collider enemyCollider;

    protected Transform player;
    protected PlayerHealth playerHealth;

    private float lastAttackTime;
    private bool isAttacking;

    protected GameManager gameManager;

    protected virtual void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider>();

        navAgent.speed = moveSpeed;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;

        if (enemyCollider != null)
        {
            enemyCollider.isTrigger = false;
        }

        navAgent.avoidancePriority = UnityEngine.Random.Range(30, 70);
        navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    protected virtual void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        player = playerObject?.transform;
        playerHealth = playerObject?.GetComponent<PlayerHealth>();

        GameObject gmObject = GameObject.FindGameObjectWithTag(gameManagerTag);
        gameManager = gmObject?.GetComponent<GameManager>();
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (player == null) return;

        if(playerHealth != null && playerHealth.isDead)
        {
            //navAgent.isStopped = true;
            navAgent.enabled = false;
            animator.SetFloat(hashMove, 0);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Idle();
        }
    }

    protected virtual void Idle()
    {
        navAgent.isStopped = true;
        animator.SetFloat(hashMove, 0);
    }

    protected virtual void ChasePlayer()
    {
        navAgent.isStopped = false;
        navAgent.SetDestination(player.position);

        float speed = navAgent.velocity.magnitude / moveSpeed;
        animator.SetFloat(hashMove, speed);

        if (navAgent.velocity.magnitude >= 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(navAgent.velocity.normalized);
        }
    }

    protected virtual void AttackPlayer()
    {
        navAgent.isStopped = true;
        animator.SetFloat(hashMove, 0);

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartCoroutine(performAttack());
        }
    }

    protected virtual IEnumerator performAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        yield return new WaitForSeconds(0.3f);

        if (player != null && playerHealth != null && !playerHealth.isDead)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                playerHealth.OnDamage(attackDamage, transform.position, transform.forward);
            }
        }
        isAttacking = false;
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.OnDamage(damage, hitPoint, hitNormal);

        if (isDead) return;

        if (hitParticle != null)
        {
            hitParticle.transform.position = hitPoint;
            hitParticle.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitParticle.Play();
        }
    }

    protected override void Die()
    {
        base.Die();

        navAgent.isStopped = true;
        navAgent.enabled = false;

        if (animator != null) animator.SetTrigger(hashDeath);

        Destroy(gameObject, 10f);
    }

    public void StartSinking()
    {
        StartCoroutine(SinkInToGround());
    }

    private IEnumerator SinkInToGround()
    {
        float sinkSpeed = 0.5f;
        float sinkDistance = 5f;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition - new Vector3(0, sinkDistance, 0);

        float sinkDuration = sinkDistance / sinkSpeed;

        float elapsedTime = 0f;

        while (elapsedTime < sinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / sinkDuration;

            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        Destroy(gameObject);
    }

}
