using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public Transform target;

    [Range(0,1)]
    public float smoothSpeed;
    public Vector3 offset;

    private void FixedUpdate() {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        //transform.LookAt(target);
    }

}
