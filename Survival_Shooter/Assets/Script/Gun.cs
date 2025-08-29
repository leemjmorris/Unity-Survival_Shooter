using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Effects")]
    public ParticleSystem muzzleEffect;
    public AudioClip shootClip;
    public LineRenderer bulletTrail;
    public float trailDuration = 0.1f;

    [Header("Shooting")]
    public Transform firePosition;
    public float fireDistance = 50f;
    public float timeBetFire = 0.15f;
    public int damage = 10;
    public LayerMask hitLayerMask = -1;

    private AudioSource audioSource;
    private float lastFireTime;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (bulletTrail == null)
        {
            bulletTrail = GetComponent<LineRenderer>();
        }

        if (bulletTrail != null)
        {
            bulletTrail.enabled = false;
            bulletTrail.positionCount = 2;
        }
    }

    private void OnEnable()
    {
        lastFireTime = 0f;
    }

    public void Fire()
    {
        if (Time.time >= lastFireTime + timeBetFire)
        {
            lastFireTime = Time.time;
            Shoot();
        }
    }

    private void Shoot()
    {
        Transform shootPoint = firePosition ? firePosition : transform;
        Vector3 startPosition = shootPoint.position;
        Vector3 shootDirection = shootPoint.forward;
        Vector3 endPosition;

        RaycastHit hit;
        if (Physics.Raycast(startPosition, shootDirection, out hit, fireDistance, hitLayerMask))
        {
            endPosition = hit.point;

            var target = hit.collider.GetComponent<IDamagable>();
            if (target != null)
            {
                target.OnDamage(damage, hit.point, hit.normal);
            }
        }
        else
        {
            endPosition = startPosition + shootDirection * fireDistance;
        }

        StartCoroutine(ShowShotEffects(startPosition, endPosition));
    }

    private IEnumerator ShowShotEffects(Vector3 startPos, Vector3 endPos)
    {
        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

        if (muzzleEffect != null)
        {
            muzzleEffect.Play();
        }

        if (bulletTrail != null)
        {
            bulletTrail.enabled = true;

            Vector3 localStartPos = transform.InverseTransformPoint(startPos);
            Vector3 localEndPos = transform.InverseTransformPoint(endPos);

            bulletTrail.SetPosition(0, localStartPos);
            bulletTrail.SetPosition(1, localEndPos);

            yield return new WaitForSeconds(trailDuration);

            bulletTrail.enabled = false;
        }
        else
        {
            yield return null;
        }
    }
}