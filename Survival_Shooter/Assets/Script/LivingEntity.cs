using System;
using UnityEngine;
using UnityEngine.Events;

public class LivingEntity : MonoBehaviour, IDamagable
{
    public float maxHealth = 100f;

    public float health { get; private set; }
    public bool isDead { get; private set; }

    public event Action OnDeath;
    public UnityEvent<float> onDamaged;
    public UnityEvent<float> onHealthChanged;

    protected virtual void OnEnable()
    {
        isDead = false;
        health = maxHealth;

        onHealthChanged?.Invoke(health);
    }

    public virtual void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

        onDamaged?.Invoke(damage);
        onHealthChanged?.Invoke(health);

        if (health <= 0f && !isDead)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        health = 0f;

        OnDeath?.Invoke();
    }

    public virtual void RestoreHealth(float amount)
    {
        if (isDead) return;

        health += amount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        onHealthChanged?.Invoke(health);
    }
}
