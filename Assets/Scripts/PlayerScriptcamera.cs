using UnityEngine;

public class PlayerFacesCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSmoothness = 15f;
    [SerializeField] private bool smoothRotation = true;

    private void Start()
    {
        // Auto-find the main camera if not assigned
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Get camera's forward direction but flatten it so the player doesn't tilt
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;

        // Skip if the forward direction is basically straight up/down
        if (forward.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(forward);

        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSmoothness
            );
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }
}