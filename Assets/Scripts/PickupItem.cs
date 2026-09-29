using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Idle Animation")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.25f;

    [Header("Identity")]
    [SerializeField] private string itemName = "It Ball";

    [Header("Throw")]
    [SerializeField] private float hitCooldown = 0.3f;   // Short — just prevents self-catch

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Vector3 startPosition;
    private bool isHeld = false;
    private bool hasBeenThrown = false;
    private float cooldownTimer = 0f;
    private bool canBePickedUp = true;
    private float lastHitTime = -999f;

    private Rigidbody rb;
    private Collider[] colliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponents<Collider>();
    }

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Don't animate while held or while flying after a throw
        if (isHeld || hasBeenThrown) return;

        // Idle spin + bob
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public string GetItemName() => itemName;
    public bool IsHeld() => isHeld;
    public bool CanBePickedUp() => canBePickedUp;

    public void SetHeld(bool held)
    {
        isHeld = held;

        if (rb != null)
        {
            rb.isKinematic = held;
            if (held)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        foreach (Collider c in colliders)
            if (c != null) c.enabled = !held;

        if (held)
        {
            // Picked up — reset throw state
            hasBeenThrown = false;
            lastHitTime = -999f;
            canBePickedUp = true;
        }
    }

    public void StartCooldown()
    {
        canBePickedUp = false;
        cooldownTimer = hitCooldown;
        hasBeenThrown = true;
        lastHitTime = -999f;

        if (debugLogs) Debug.Log($"[Ball] Thrown. Cooldown {hitCooldown}s.");
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canBePickedUp = true;
                if (debugLogs) Debug.Log("[Ball] Cooldown over, can be picked up.");
            }
        }
    }

    // Run cooldown even when thrown (since Update returns early for thrown balls)
    private void FixedUpdate()
    {
        UpdateCooldown();
    }

    // ---------- HIT DETECTION ----------

    private void OnCollisionEnter(Collision collision)
    {
        if (debugLogs) Debug.Log($"[Ball] OnCollisionEnter with {collision.collider.name}");

        if (isHeld) return;
        TryHitPlayer(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (debugLogs) Debug.Log($"[Ball] OnTriggerEnter with {other.name}");

        if (isHeld) return;
        TryHitPlayer(other);
    }

    private void TryHitPlayer(Collider other)
    {
        // Ignore rapid re-hits
        if (Time.time - lastHitTime < 0.15f) return;

        PlayerPickup hitPlayer = other.GetComponent<PlayerPickup>();
        if (hitPlayer == null)
            hitPlayer = other.GetComponentInParent<PlayerPickup>();

        if (hitPlayer == null)
        {
            if (debugLogs) Debug.Log($"[Ball] No PlayerPickup on {other.name}");
            return;
        }

        if (hitPlayer.IsHoldingItem())
        {
            if (debugLogs) Debug.Log($"[Ball] {hitPlayer.name} is already holding something");
            return;
        }

        lastHitTime = Time.time;
        if (debugLogs) Debug.Log($"[Ball] Tagging {hitPlayer.name}!");

        hitPlayer.ForcePickUp(this);
    }
}