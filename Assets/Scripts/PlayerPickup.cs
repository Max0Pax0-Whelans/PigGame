using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    [Header("Hold Settings")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float holdFollowSpeed = 15f;

    [Header("Throw Settings")]
    [SerializeField] private KeyCode throwKey = KeyCode.Mouse0;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float upwardAngle = 20f;    // Degrees above horizontal

    [Header("It Visual")]
    [SerializeField] private Renderer[] renderersToTint; // Assign the player's renderer(s)
    [SerializeField] private Color itColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<PickupItem> nearbyItems = new List<PickupItem>();
    private PickupItem closestItem;
    private PickupItem heldItem;

    public bool IsIt { get; private set; } = false;

    private void Start()
    {
        SetItVisual(false);
    }

    private void Update()
    {
        if (heldItem != null)
        {
            HandleHolding();
            HandleThrow();
        }
        else
        {
            HandlePickupDetection();
            HandlePickup();
        }
    }

    // ---------- DETECTION ----------

    private void HandlePickupDetection()
    {
        nearbyItems.RemoveAll(item => item == null);
        closestItem = GetClosestItem();
    }

    private PickupItem GetClosestItem()
    {
        PickupItem closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (PickupItem item in nearbyItems)
        {
            if (item == null || !item.CanBePickedUp()) continue;
            float distance = Vector3.Distance(transform.position, item.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = item;
            }
        }
        return closest;
    }

    // ---------- PICKUP ----------

    private void HandlePickup()
    {
        if (closestItem != null && Input.GetKeyDown(pickupKey))
        {
            PickUp(closestItem);
        }
    }

    public void PickUp(PickupItem item)
    {
        if (heldItem != null) return;           // Already holding something
        if (!item.CanBePickedUp()) return;       // On cooldown

        heldItem = item;
        heldItem.SetHeld(true);
        nearbyItems.Remove(item);
        closestItem = null;

        heldItem.transform.position = holdPoint.position;

        // Player is now "It"
        SetIt(true);
    }

    // Called by PickupItem when it hits this player during a throw
    public void ForcePickUp(PickupItem item)
    {
        if (heldItem != null) return;   // Safety check
        PickUp(item);
    }

    // ---------- HOLDING ----------

    private void HandleHolding()
    {
        if (holdPoint == null) return;

        heldItem.transform.position = Vector3.Lerp(
            heldItem.transform.position,
            holdPoint.position,
            Time.deltaTime * holdFollowSpeed
        );
        heldItem.transform.rotation = holdPoint.rotation;
    }

    // ---------- THROW ----------

    private void HandleThrow()
    {
        if (Input.GetKeyDown(throwKey))
        {
            Throw(throwForce);
        }
    }

    private void Throw(float force)
    {
        if (heldItem == null) return;

        PickupItem itemToThrow = heldItem;
        heldItem = null;

        // 1) Release from held state
        itemToThrow.SetHeld(false);
        itemToThrow.StartCooldown();

        // 2) Make sure the ball isn't parented to anything
        itemToThrow.transform.SetParent(null);

        // 3) Work out the throw direction
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();
        Vector3 throwDir = Quaternion.AngleAxis(-upwardAngle, transform.right) * flatForward;

        // 4) Nudge the ball out of the player's collider
        itemToThrow.transform.position += throwDir * 0.5f + Vector3.up * 0.3f;

        // 5) Apply force
        Rigidbody rb = itemToThrow.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.AddForce(throwDir * force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

            // ---- DETAILED DEBUG ----
            if (debugLogs)
            {
                Debug.Log($"[Throw] dir={throwDir}, force={force}, mass={rb.mass}");
                Debug.Log($"[Throw] rb.isKinematic={rb.isKinematic}, useGravity={rb.useGravity}, " +
                          $"drag={rb.linearDamping}, angularDrag={rb.angularDamping}, " +
                          $"constraints={rb.constraints}");
                Debug.Log($"[Throw] Ball position={itemToThrow.transform.position}, " +
                          $"parent={itemToThrow.transform.parent}, " +
                          $"scale={itemToThrow.transform.lossyScale}");
                Debug.Log($"[Throw] Colliders enabled: {string.Join(", ", System.Array.ConvertAll(itemToThrow.GetComponents<Collider>(), c => c.enabled.ToString()))}");
            }
        }
        else
        {
            Debug.LogError("[Throw] Ball has no Rigidbody!");
        }

        // Player is no longer "It"
        SetIt(false);
    }

    // ---------- IT STATE ----------

    private void SetIt(bool value)
    {
        IsIt = value;
        SetItVisual(value);
    }

    private void SetItVisual(bool isIt)
    {
        if (renderersToTint == null) return;

        foreach (Renderer r in renderersToTint)
        {
            if (r == null) continue;
            r.material.color = isIt ? itColor : normalColor;
        }
    }

    // ---------- TRIGGER DETECTION ----------

    private void OnTriggerEnter(Collider other)
    {
        PickupItem item = other.GetComponent<PickupItem>();
        if (item != null && item != heldItem && !nearbyItems.Contains(item))
        {
            nearbyItems.Add(item);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PickupItem item = other.GetComponent<PickupItem>();
        if (item != null)
        {
            nearbyItems.Remove(item);
        }
    }

    // ---------- UI HELPERS ----------

    public bool HasItemInRange() => closestItem != null && !IsHoldingItem();
    public string GetClosestItemName() => closestItem != null ? closestItem.GetItemName() : "";
    public bool IsHoldingItem() => heldItem != null;
    public string GetHeldItemName() => heldItem != null ? heldItem.GetItemName() : "";

    // Used by the AI to trigger a throw
    public void ThrowAt(float force)
    {
        if (heldItem == null) return;
        Throw(force);
    }
}