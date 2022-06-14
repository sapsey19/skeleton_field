using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform target;

    [Range(0, 1)]
    public float smoothSpeed;
    public Vector3 offset;

    Vector3 smoothDampSpeed;

    private void FixedUpdate() {
        if (target != null) {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothDampSpeed, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

}
