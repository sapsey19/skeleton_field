using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class Lightning : MonoBehaviour {

    public LayerMask enemyMask;

    public Transform point1; //weapon position
    public Transform point2; //cursor position
    public float chainRadius;

    Transform point3;    //enemy position
    List<int> enemyIDs = new List<int>();

    [SerializeField]
    List<Vector3> chainPoints = new List<Vector3>(); //list of chain points for lightning chain effect 

    bool hittingEnemy;
    bool firing;

    LineRenderer lr;
    MeshCollider meshCollider;

    List<Collider> colliders = new List<Collider>();
    List<int> hitEnemyIds = new List<int>();

    public List<Collider> GetColliders() { return colliders; }

    private void Start() {
        meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.enabled = false;

        lr = GetComponent<LineRenderer>();
        lr.enabled = false;

        point3 = null;
    }

    private void Update() {
        if (Input.GetMouseButton(0)) {
            meshCollider.enabled = true;
            firing = true;

        }
        if (Input.GetMouseButtonUp(0)) {
            meshCollider.enabled = false;
            lr.enabled = false;
            firing = false;
            colliders.Clear();
            enemyIDs.Clear();
            chainPoints.Clear();
        }

        // Lightning latches on to nearest enemy in hitbox
        FindClosestEnemyInHitbox();
    }

    private void LateUpdate() {
        if (firing) {
            if (hittingEnemy) {
                DrawCurvedLine();
            }
            else {
                DrawLine();
            }
        }
    }

    private void FindClosestEnemyInHitbox() {
        List<Collider> inHitboxColliders = GetColliders();

        float min = 10000f;
        int i = 0;

        Transform closestEnemy = null;

        foreach (Collider c in inHitboxColliders) {
            float distance = Vector3.Distance(point1.position, c.transform.position);
            if (distance < min) {
                min = distance;
                closestEnemy = c.transform;
                i++;
            }
        }

        point3 = closestEnemy;
    }

    private void OnDrawGizmos() {
        //Debug.Log("on draw gizmos");
        if (point3) {
            Gizmos.DrawSphere(point3.position, chainRadius);
        }
    }

    private void FindClosestEnemy() {
        Collider[] nearbyEnemies = Physics.OverlapSphere(point3.position, chainRadius, enemyMask);

        float min = 10000f;
        int i = 0;
        int indexOfClosest = 0;

        Transform closestEnemy = null;

        //Debug.Log(nearbyEnemies.Length + " " + point3.position);

        foreach (Collider c in nearbyEnemies) {
            if (!enemyIDs.Contains(c.GetInstanceID())) {
                Debug.Log("here");
                float distance = Vector3.Distance(transform.position, c.transform.position);
                if (distance < min) {
                    min = distance;
                    closestEnemy = c.transform;
                    indexOfClosest = i;
                }
                enemyIDs.Add(c.GetInstanceID());
            }
            i++;
        }

        if (closestEnemy!=null && !enemyIDs.Contains(closestEnemy.GetInstanceID())) {
            chainPoints.Add(closestEnemy.position);
        }
    }

    private void DrawLine() {
        lr.enabled = true;
        lr.positionCount = 2;
        lr.SetPosition(0, point1.position);
        lr.SetPosition(1, point2.position);
    }

    private void DrawCurvedLine() {
        lr.enabled = true;
        lr.positionCount = 3;
        lr.SetPosition(0, point1.position);
        lr.SetPosition(1, point2.position);
        lr.SetPosition(2, point3.position);

        BezierCurve();

        FindClosestEnemy();

        lr.positionCount += chainPoints.Count;

        lr.SetPositions(chainPoints.ToArray());
    }

    private void OnTriggerStay(Collider other) {
        GameObject hitObject = other.gameObject;
        if (hitObject.layer == LayerMask.NameToLayer("Enemy")) {
            if (!colliders.Contains(other))
                colliders.Add(other);

            lr.enabled = true;
            hittingEnemy = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            hittingEnemy = false;
            colliders.Remove(other);
        }
    }

    private void BezierCurve() {
        List<Vector3> pointList = new List<Vector3>();
        for (float ratio = 0; ratio <= 1; ratio += 1.0f / 12) {
            Vector3 tangentLineVertex1 = Vector3.Lerp(point1.position, point2.position, ratio);
            Vector3 tangentLineVertex2 = Vector3.Lerp(point2.position, point3.position, ratio);
            Vector3 bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
            pointList.Add(bezierPoint);
        }
        lr.positionCount = pointList.Count;
        lr.SetPositions(pointList.ToArray());
    }

}
