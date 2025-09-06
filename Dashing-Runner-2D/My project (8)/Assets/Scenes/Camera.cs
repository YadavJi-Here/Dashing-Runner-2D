using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Assign your player here
    public float smoothSpeed = 0.125f;
    public Vector3 offset;    // e.g., new Vector3(0, 1, -10)

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 desiredPosition = player.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
