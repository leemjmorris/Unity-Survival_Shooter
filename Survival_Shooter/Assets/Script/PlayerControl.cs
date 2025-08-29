using System;
using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private static readonly int hashMove = Animator.StringToHash("Move");

    [Header("Player Movement")]
    public float moveSpeed;

    [Header("Gun Reference")]
    public Gun gun;

    private Rigidbody rigidBody;
    private PlayerInput playerInput;
    private Animator animator;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        
        if (gun == null)
        {
            gun = GetComponentInChildren<Gun>();
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void FixedUpdate()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);

        Plane groundPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance);
            Vector3 direction = (targetPoint - transform.position).normalized;

            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                rigidBody.MoveRotation(lookRotation);
            }
        }

        Vector3 moveDirection = new Vector3(playerInput.MoveHorizontal, 0f, playerInput.MoveVertical);
        Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

        rigidBody.MovePosition(rigidBody.position + movement);

        animator.SetFloat(hashMove, moveDirection.magnitude);
    }

    private void Update()
    {
        if (playerInput.Fire && gun != null)
        {
            gun.Fire();
        }
    }
}