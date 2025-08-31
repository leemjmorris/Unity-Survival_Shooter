using System;
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

    [Header("Camera Reference")]
    public CameraController cameraController;

    private AudioSource audioSource;
    private float lastFireTime;
    private Coroutine trailCoroutine;

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

        // LMJ: Find camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = Camera.main.GetComponent<CameraController>();
        }
    }

    private void OnEnable()
    {
        lastFireTime = 0f;
    }

    private void OnDisable()
    {
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        if (bulletTrail != null)
        {
            bulletTrail.enabled = false;
        }
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
        Vector3 startPosition;
        Vector3 shootDirection;

        // LMJ: Check camera mode
        bool isFirstPerson = cameraController != null && cameraController.IsFirstPerson();

        if (isFirstPerson)
        {
            // LMJ: First-person mode - shoot from active camera center
            Camera activeCamera = cameraController.GetActiveCamera();
            if (activeCamera != null)
            {
                startPosition = activeCamera.transform.position;
                shootDirection = activeCamera.transform.forward;
            }
            else
            {
                // LMJ: Fallback
                Transform shootPoint = firePosition ? firePosition : transform;
                startPosition = shootPoint.position;
                shootDirection = shootPoint.forward;
            }
        }
        else
        {
            // LMJ: Top-down mode - use original logic
            Transform shootPoint = firePosition ? firePosition : transform;
            startPosition = shootPoint.position;
            shootDirection = shootPoint.forward;
        }

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

        // LMJ: Show effects from gun position in both modes
        Vector3 effectStartPos = firePosition ? firePosition.position : transform.position;
        ShowShotEffects(effectStartPos, endPosition);
    }

    private void ShowShotEffects(Vector3 startPos, Vector3 endPos)
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
            if (trailCoroutine != null)
            {
                StopCoroutine(trailCoroutine);
            }
            trailCoroutine = StartCoroutine(ShowBulletTrail(startPos, endPos));
        }
    }

    private IEnumerator ShowBulletTrail(Vector3 startPos, Vector3 endPos)
    {
        bulletTrail.enabled = true;

        Vector3 localStartPos = transform.InverseTransformPoint(startPos);
        Vector3 localEndPos = transform.InverseTransformPoint(endPos);

        bulletTrail.SetPosition(0, localStartPos);
        bulletTrail.SetPosition(1, localEndPos);

        float waitTime = Mathf.Max(trailDuration, 0.05f);
        yield return new WaitForSeconds(trailDuration);

        bulletTrail.enabled = false;
        trailCoroutine = null;
    }
}