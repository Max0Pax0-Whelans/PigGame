using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Camera (required for camera-relative movement)")]
    [Tooltip("Assign the camera transform. Leave empty to auto-find Main Camera.")]
    public Transform cameraTransform;

    Rigidbody rigidbody;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Let PlayerFacesCamera handle rotation — physics must not fight it
        rigidbody.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // Running state
        IsRunning = canRun && Input.GetKey(runningKey);

        // Target speed
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Raw input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector2 input = new Vector2(horizontal, vertical);

        // Clamp so diagonal isn't faster
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Camera-relative movement
        Vector3 moveDirection;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = camForward * input.y + camRight * input.x;
        }
        else
        {
            moveDirection = new Vector3(input.x, 0f, input.y);
        }

        // Apply movement (preserve vertical velocity for gravity)
        Vector3 velocity = moveDirection * targetMovingSpeed;
        rigidbody.linearVelocity = new Vector3(velocity.x, rigidbody.linearVelocity.y, velocity.z);

        // NOTE: Rotation is handled by PlayerFacesCamera, so we don't rotate here.
    }
}