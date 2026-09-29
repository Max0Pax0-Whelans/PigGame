using UnityEngine;

public class BallThrow : MonoBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 15f;    // How hard the ball is thrown
    [SerializeField] private float upwardAngle = 20f;   // Degrees above horizontal — makes it arc
    [SerializeField] private float spinForce = 5f;      // Spin as it flies

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Throws the ball from its current position in the given direction.
    /// </summary>
    /// <param name="thrower">The transform of whoever is throwing (player or AI).</param>
    public void Throw(Transform thrower)
    {
        // 1) Detach from any parent (in case it was parented to the hold point)
        transform.SetParent(null);

        // 2) Make sure physics is on and colliders are enabled
        rb.isKinematic = false;
        rb.useGravity = true;

        foreach (Collider c in GetComponents<Collider>())
            c.enabled = true;

        // 3) Clear any leftover velocity from being carried around
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 4) Work out the throw direction: thrower's forward, tilted up by upwardAngle
        Vector3 flatForward = thrower.forward;
        flatForward.y = 0f;                             // ignore any downward tilt
        flatForward.Normalize();

        // Rotate that flat forward direction upward by upwardAngle degrees
        Vector3 throwDir = Quaternion.AngleAxis(-upwardAngle, thrower.right) * flatForward;

        // 5) Move ball slightly forward and up so it doesn't collide with the thrower
        transform.position += throwDir * 0.5f + Vector3.up * 0.3f;

        // 6) Apply the force
        rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * spinForce, ForceMode.Impulse);
    }
}