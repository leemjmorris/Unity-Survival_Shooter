using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera References")]
    public Camera topDownCamera;
    public Camera firstPersonCamera;

    [Header("First Person Settings")]
    public float mouseSensitivity = 2f;
    public float verticalRotationLimit = 80f;

    [Header("UI")]
    public GameObject crosshairUI;

    [Header("References")]
    public Transform player;

    private bool isFirstPerson = false;
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // LMJ: Find cameras if not assigned
        if (topDownCamera == null)
        {
            GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCam != null)
            {
                topDownCamera = mainCam.GetComponent<Camera>();
            }
        }

        // LMJ: Ensure audio listeners exist
        EnsureAudioListeners();
    }

    private void EnsureAudioListeners()
    {
        // LMJ: Check and add audio listeners if needed
        if (topDownCamera != null)
        {
            AudioListener topListener = topDownCamera.GetComponent<AudioListener>();
            if (topListener == null)
            {
                topDownCamera.gameObject.AddComponent<AudioListener>();
            }
        }

        if (firstPersonCamera != null)
        {
            AudioListener fpListener = firstPersonCamera.GetComponent<AudioListener>();
            if (fpListener == null)
            {
                firstPersonCamera.gameObject.AddComponent<AudioListener>();
            }
        }
    }

    private void Start()
    {
        // LMJ: Ensure at least one camera is active at start
        if (topDownCamera != null && firstPersonCamera != null)
        {
            // LMJ: Start with top-down view
            topDownCamera.gameObject.SetActive(true);
            firstPersonCamera.gameObject.SetActive(false);

            // LMJ: Ensure audio listeners are properly set
            AudioListener topListener = topDownCamera.GetComponent<AudioListener>();
            if (topListener != null) topListener.enabled = true;

            AudioListener fpListener = firstPersonCamera.GetComponent<AudioListener>();
            if (fpListener != null) fpListener.enabled = false;
        }

        SetTopDownMode();
    }

    private void Update()
    {
        // LMJ: Toggle camera mode with V key
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (isFirstPerson)
            {
                SetTopDownMode();
            }
            else
            {
                SetFirstPersonMode();
            }
        }

        // LMJ: Update first person controls if active
        if (isFirstPerson && firstPersonCamera != null)
        {
            UpdateFirstPersonControls();
        }
    }

    private void LateUpdate()
    {
        // LMJ: Keep first person camera at player position
        if (isFirstPerson && firstPersonCamera != null && player != null)
        {
            firstPersonCamera.transform.position = player.position + Vector3.up * 1.5f;
        }
    }

    private void SetFirstPersonMode()
    {
        isFirstPerson = true;

        // LMJ: Switch cameras
        if (topDownCamera != null)
        {
            topDownCamera.gameObject.SetActive(false);
            // LMJ: Disable top-down audio listener
            AudioListener topListener = topDownCamera.GetComponent<AudioListener>();
            if (topListener != null) topListener.enabled = false;
        }

        if (firstPersonCamera != null)
        {
            firstPersonCamera.gameObject.SetActive(true);
            // LMJ: Enable first-person audio listener
            AudioListener fpListener = firstPersonCamera.GetComponent<AudioListener>();
            if (fpListener != null) fpListener.enabled = true;
        }

        // LMJ: Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // LMJ: Enable crosshair
        if (crosshairUI != null)
            crosshairUI.SetActive(true);

        // LMJ: Initialize rotation
        if (player != null)
        {
            horizontalRotation = player.eulerAngles.y;
            verticalRotation = 0f;
        }
    }

    private void SetTopDownMode()
    {
        isFirstPerson = false;

        // LMJ: Switch cameras
        if (firstPersonCamera != null)
        {
            firstPersonCamera.gameObject.SetActive(false);
            // LMJ: Disable first-person audio listener
            AudioListener fpListener = firstPersonCamera.GetComponent<AudioListener>();
            if (fpListener != null) fpListener.enabled = false;
        }

        if (topDownCamera != null)
        {
            topDownCamera.gameObject.SetActive(true);
            // LMJ: Enable top-down audio listener
            AudioListener topListener = topDownCamera.GetComponent<AudioListener>();
            if (topListener != null) topListener.enabled = true;
        }

        // LMJ: Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // LMJ: Disable crosshair
        if (crosshairUI != null)
            crosshairUI.SetActive(false);
    }

    private void UpdateFirstPersonControls()
    {
        // LMJ: Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // LMJ: Update rotation
        horizontalRotation += mouseX;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);

        // LMJ: Apply rotation to player and camera
        if (player != null)
        {
            player.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        }

        if (firstPersonCamera != null)
        {
            firstPersonCamera.transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        }
    }

    public bool IsFirstPerson()
    {
        return isFirstPerson;
    }

    public Camera GetActiveCamera()
    {
        return isFirstPerson ? firstPersonCamera : topDownCamera;
    }
}