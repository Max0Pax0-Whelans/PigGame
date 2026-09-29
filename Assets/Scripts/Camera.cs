using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;              // The player
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f); // Look at player's chest height

    [Header("Distance & Height")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float heightOffset = 1f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private KeyCode unlockCursorKey = KeyCode.Escape;

    [Header("Smoothing")]
    [SerializeField] private float followSmoothness = 10f;
    [SerializeField] private float rotationSmoothness = 10f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = ~0;  // Everything by default
    [SerializeField] private float collisionRadius = 0.3f;

    private float yaw = 0f;
    private float pitch = 15f;
    private Vector3 currentVelocity;

    private void Start()
    {
        if (target == null)
        {
            // Try to auto-find the human player
            PlayerPickup[] players = FindObjectsOfType<PlayerPickup>();
            foreach (var p in players)
            {
                // Skip AI — assume the one with the camera script attached is the human
                if (p.GetComponent<AIController>() == null)
                {
                    target = p.transform;
                    break;
                }
            }
        }

        // Start behind the player
        if (target != null)
            yaw = target.eulerAngles.y;

        LockCursor(true);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleCursorToggle();
        HandleMouseInput();
        HandleCameraPosition();
    }

    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(unlockCursorKey))
        {
            LockCursor(Cursor.lockState != CursorLockMode.Locked);
        }
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HandleMouseInput()
    {
        // Don't rotate if cursor is unlocked
        if (Cursor.lockState != CursorLockMode.Locked) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1f : 1f);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void HandleCameraPosition()
    {
        // The look-at point (player's chest/shoulder area)
        Vector3 lookAtPoint = target.position + targetOffset;

        // Calculate desired camera rotation
        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);

        // Calculate desired camera position (behind the look-at point)
        Vector3 desiredPosition = lookAtPoint - desiredRotation * Vector3.forward * distance;

        // --- Collision check: pull camera in if a wall is between it and the player ---
        Vector3 dir = (desiredPosition - lookAtPoint).normalized;
        float desiredDist = distance;

        if (Physics.SphereCast(lookAtPoint, collisionRadius, dir, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            desiredDist = Mathf.Max(minDistance, hit.distance - 0.1f);
        }

        Vector3 finalPosition = lookAtPoint - desiredRotation * Vector3.forward * desiredDist;

        // Smooth the position
        transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * followSmoothness);

        // Smooth the rotation (look at the target)
        Quaternion lookRotation = Quaternion.LookRotation(lookAtPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSmoothness);
    }

    // Optional: call this to snap the camera instantly (useful at round start)
    public void SnapToTarget()
    {
        if (target == null) return;

        Vector3 lookAtPoint = target.position + targetOffset;
        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = lookAtPoint - desiredRotation * Vector3.forward * distance;
        transform.rotation = Quaternion.LookRotation(lookAtPoint - transform.position, Vector3.up);
    }
}