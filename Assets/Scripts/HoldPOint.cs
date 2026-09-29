using UnityEngine;

public class OrbitHoldPoint : MonoBehaviour
{
    [SerializeField] private Transform followTarget;   // The player/AI this orbits
    [SerializeField] private float distance = 1.5f;    // How far in front
    [SerializeField] private float height = 1f;        // How high above the player's pivot
    [SerializeField] private float smoothSpeed = 15f;  // How snappy the follow is

    private void LateUpdate()
    {
        if (followTarget == null) return;

        // Position in front of the target based on its forward direction
        Vector3 desiredPos = followTarget.position
                           + followTarget.forward * distance
                           + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);

        // Face the same way as the target so throws go forward
        transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.rotation, Time.deltaTime * smoothSpeed);
    }
}