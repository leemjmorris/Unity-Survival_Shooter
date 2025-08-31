using UnityEngine;

public class ZomBunny : EnemyBase
{
    [Header("Sounds")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    protected override void Awake()
    {
        base.Awake();

        maxHealth = 100f;
        moveSpeed = 2.5f;
        attackDamage = 10f;
        attackRange = 1f;
        attackCooldown = 1.5f;
        detectionRange = 25f;

        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.acceleration = 4f;
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!isDead && hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, 1f);
        }

        base.OnDamage(damage, hitPoint, hitNormal);
    }

    protected override void Die()
    {
        if (gameManager != null)
        {
            gameManager.AddScore(10);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        }

        base.Die();
    }
}