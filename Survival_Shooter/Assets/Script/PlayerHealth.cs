using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : LivingEntity
{
    public static readonly int IdDeath = Animator.StringToHash("Die");

    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip deathClip;
    public AudioClip hitClip;

    [Header("Components")]
    private Animator animator;
    private PlayerControl playerControl;

    public float HealthPercentage => health / maxHealth;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        playerControl = GetComponent<PlayerControl>();
    }

    private void Start()
    {
        onHealthChanged?.Invoke(health);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnDamage(10f, transform.position, Vector3.up);
        }
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead) return;

        if (audioSource != null && hitClip != null)
        {
            audioSource.PlayOneShot(hitClip);
        }

        base.OnDamage(damage, hitPoint, hitNormal);

        Debug.Log($"체력: {health}/{maxHealth} (데미지: {damage})");
    }

    protected override void Die()
    {
        base.Die();

        if (animator != null)
        {
            animator.SetTrigger(IdDeath);
        }

        if (audioSource != null && deathClip != null)
        {
            audioSource.PlayOneShot(deathClip);
        }

        if (playerControl != null)
        {
            playerControl.enabled = false;
        }
    }

    public void RestartLevel()
    {

    }
}