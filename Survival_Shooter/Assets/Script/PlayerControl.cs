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

    [Header("Camera Reference")]
    public CameraController cameraController;

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

        // LMJ: Find camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = Camera.main.GetComponent<CameraController>();
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void FixedUpdate()
    {
        // LMJ: Check if camera controller exists and mode
        bool isFirstPerson = cameraController != null && cameraController.IsFirstPerson();

        if (!isFirstPerson)
        {
            // LMJ: Top-down mode - rotate to mouse position
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
        }
        // LMJ: In first-person mode, rotation is handled by CameraController

        // LMJ: Movement works the same in both modes
        Vector3 moveDirection;

        if (isFirstPerson)
        {
            // LMJ: Move relative to player's rotation in first-person
            moveDirection = transform.right * playerInput.MoveHorizontal + transform.forward * playerInput.MoveVertical;
        }
        else
        {
            // LMJ: Move in world space for top-down
            moveDirection = new Vector3(playerInput.MoveHorizontal, 0f, playerInput.MoveVertical);
        }

        moveDirection.y = 0;
        moveDirection.Normalize();

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