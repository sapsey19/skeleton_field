using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer))]
public class Lightning : MonoBehaviour {

    public LayerMask enemyMask;

    public Transform weaponPosition; //weapon position
    public Transform cursorPosition; //cursor position
    public float chainRadius;

    Transform enemyPosition;    //enemy position
    List<int> enemyIDs = new List<int>();
    Enemy enemy;

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

        enemyPosition = null;
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
        //FindClosestEnemyInHitbox();
    }

    private void LateUpdate() {
        if (firing) {
            if (hittingEnemy) {
                //DrawCurvedLine();
                FindClosestEnemyInHitbox();
                DrawLineToEnemy();
                //FindClosestEnemy();
                if (enemy != null && enemy.FindClosestEnemy(chainRadius) != null) {
                    chainPoints.Add(enemy.FindClosestEnemy(chainRadius));
                }

                ChainToEnemies();
                //Chain();
            }
            else {
                chainPoints.Clear();
                enemyPosition = null;
                DrawLine();
            }
        }
    }

    private void FindClosestEnemyInHitbox() {
        List<Collider> inHitboxColliders = GetColliders();

        float min = 10000f;
        int i = 0;

        Collider closestEnemy = null;


        foreach (Collider c in inHitboxColliders) {
            float distance = Vector3.Distance(weaponPosition.position, c.transform.position);
            if (distance < min) {
                min = distance;
                closestEnemy = c;
                i++;
            }
        }

        if (closestEnemy != null) {
            enemyIDs.Add(closestEnemy.gameObject.GetInstanceID());
            enemyPosition = closestEnemy.transform;
            enemy = closestEnemy.GetComponent<Enemy>();
        }
    }

    private void FindNearbyEnemies() {

    }

    private void OnDrawGizmos() {
        //Debug.Log("on draw gizmos");
        //if (enemyPosition) {
        //    Gizmos.DrawSphere(enemyPosition.position, chainRadius);
        //}
    }

    private void FindClosestEnemy() {
        if (enemyPosition == null) {
            return;
        }
        Collider[] nearbyEnemies = Physics.OverlapSphere(enemyPosition.position, chainRadius, enemyMask);


        float min = 10000f;
        int i = 0;
        int indexOfClosest = 0;

        Transform closestEnemy = null;

        //Debug.Log(nearbyEnemies.Length + " " + point3.position);

        foreach (Collider c in nearbyEnemies) {
            if (!enemyIDs.Contains(c.gameObject.GetInstanceID())) {
                //Debug.Log("here");
                float distance = Vector3.Distance(transform.position, c.transform.position);
                if (distance < min && distance > .01f) {
                    min = distance;
                    closestEnemy = c.transform;
                    indexOfClosest = i;
                    //Debug.Log("distance: " + distance);

                }
                enemyIDs.Add(c.gameObject.GetInstanceID());
            }
            i++;
        }

        //if closest enemy isn't null, and that enemy isn't already being hit
        if (closestEnemy != null) { // && !enemyIDs.Contains(closestEnemy.gameObject.GetInstanceID())) {
            chainPoints.Add(closestEnemy.position);
            Debug.Log("closest enemy to enemy: " + closestEnemy.name);
            //ChainToEnemies();
        }
    }

    private void ChainToEnemies() {
        for (int i = 0; i < chainPoints.Count; i++) {
            //Debug.Log("chainPoints count: " + chainPoints.Count);
            //Debug.Log("position count before: " + lr.positionCount);
            lr.positionCount++;
            lr.SetPosition(i + 2, chainPoints[i]);
            //Debug.Log("position count after: " + lr.positionCount);
        }
    }

    private void DrawLine() {
        lr.enabled = true;
        lr.positionCount = 2;
        lr.SetPosition(0, weaponPosition.position);
        lr.SetPosition(1, cursorPosition.position);
    }

    private void DrawLineToEnemy() {
        lr.enabled = true;
        lr.positionCount = 2;

        lr.SetPosition(0, weaponPosition.position);

        if (!enemyPosition) {
            lr.SetPosition(1, cursorPosition.position);
            return;
        }

        lr.SetPosition(1, enemyPosition.position);

    }

    private void DrawCurvedLine() {
        lr.enabled = true;
        lr.positionCount = 3;
        lr.SetPosition(0, weaponPosition.position);
        lr.SetPosition(1, cursorPosition.position);
        lr.SetPosition(2, enemyPosition.position);

        //BezierCurve();

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
            //Debug.Log(point1.position + " " + point2.position + " " + point3.position);

            Vector3 tangentLineVertex1 = Vector3.Lerp(weaponPosition.position, cursorPosition.position, ratio);
            Vector3 tangentLineVertex2 = Vector3.Lerp(cursorPosition.position, enemyPosition.position, ratio);
            Vector3 bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
            pointList.Add(bezierPoint);
        }
        lr.positionCount = pointList.Count;
        lr.SetPositions(pointList.ToArray());
    }


    //Taylor code (oh god) 


    //private Collider FindClosestEnemy(Vector3 enemyPosition) {
    //    if (enemyPosition == null) {
    //        return null;
    //    }
    //    Collider[] nearbyEnemies = Physics.OverlapSphere(enemyPosition, chainRadius, enemyMask);


    //    float min = 10000f;
    //    int i = 0;
    //    int indexOfClosest = 0;

    //    Collider closestEnemy = null;

    //    //Debug.Log(nearbyEnemies.Length + " " + point3.position);

    //    foreach (Collider c in nearbyEnemies) {
    //        if (!enemyIDs.Contains(c.gameObject.GetInstanceID())) {
    //            //Debug.Log("here");
    //            float distance = Vector3.Distance(transform.position, c.transform.position);
    //            if (distance < min && distance > .01f) {
    //                min = distance;
    //                closestEnemy = c;
    //                indexOfClosest = i;
    //                //Debug.Log("distance: " + distance);

    //            }
    //            enemyIDs.Add(c.gameObject.GetInstanceID());
    //        }
    //        i++;
    //    }

    //    return closestEnemy;
    //}

    //// Chain lightning to surrounding enemies
    //private void Chain(Collider enemy) {
    //    if (enemy == null) {
    //        return;
    //    }

    //    Collider closestEnemy = FindClosestEnemy(enemy.transform.position);
    //    if (!closestEnemy) {
    //        return;
    //    }

    //    hitEnemyIds.Add(closestEnemy.gameObject.GetInstanceID());
    //    chainPoints.Add(closestEnemy.transform.position);

    //    lr.positionCount = chainPoints.Count;
    //    lr.SetPositions(chainPoints.ToArray());
    //    DrawLine();
    //    Chain(closestEnemy);
    //}

    //private Collider FindClosestEnemyInHitboxReturn() {
    //    List<Collider> inHitboxColliders = GetColliders();

    //    float min = 10000f;
    //    int i = 0;

    //    Collider closestEnemy = null;


    //    foreach (Collider c in inHitboxColliders) {
    //        float distance = Vector3.Distance(weaponPosition.position, c.transform.position);
    //        if (distance < min) {
    //            min = distance;
    //            closestEnemy = c;
    //            i++;
    //        }
    //    }

    //    if (closestEnemy != null) {
    //        enemyIDs.Add(closestEnemy.gameObject.GetInstanceID());
    //        enemyPosition = closestEnemy.transform;
    //        enemy = closestEnemy.GetComponent<Enemy>();
    //    }

    //    return closestEnemy;
    //}


}
