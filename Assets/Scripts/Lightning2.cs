using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class Lightning2 : MonoBehaviour {

    public LayerMask enemyMask;

    public Transform weaponPosition;
    public Transform cursorPosition;
    public float chainRadius;

    Transform enemyPosition = null;

    [SerializeField]
    List<Vector3> chainPoints = new List<Vector3>(); //list of chain points for lightning chain effect 

    LineRenderer lr;
    MeshCollider meshCollider;

    List<Collider> colliders = new List<Collider>();
    List<int> hitEnemyIds = new List<int>();

    private void Start() {
        meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.enabled = false;

        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
    }

    private void Update() {
        if (Input.GetMouseButton(0)) {
            Fire();
        }
        if (Input.GetMouseButtonUp(0)) {
            ResetLightning();
        }
    }

    private void Fire() {
        meshCollider.enabled = true;

        Collider closestEnemy = FindClosestEnemyInHitbox();

        if (closestEnemy == null) {
            lr.positionCount = 2;
            lr.SetPosition(0, weaponPosition.position);
            lr.SetPosition(1, cursorPosition.position);
            DrawLine(); // draw line to cursor
        }
        else {
            //chainPoints.Clear();
            lr.SetPosition(0, weaponPosition.position);
            lr.SetPosition(1, closestEnemy.transform.position);

            //chainPoints.Add(weaponPosition.position);

            hitEnemyIds.Add(closestEnemy.gameObject.GetInstanceID());

            //adding enemy position every frame this is no good 
            //chainPoints.Add(closestEnemy.transform.position);

            lr.positionCount = chainPoints.Count;
            lr.SetPositions(chainPoints.ToArray());
            DrawLine();

            Chain(closestEnemy);
        }
    }

    // Chain lightning to surrounding enemies
    private void Chain(Collider enemy) {
        if (enemy == null) {
            return;
        }

        Collider closestEnemy = FindClosestEnemy(enemy.transform.position);
        if (!closestEnemy) {
            return;
        }

        hitEnemyIds.Add(closestEnemy.gameObject.GetInstanceID());
        chainPoints.Add(closestEnemy.transform.position);

        lr.positionCount = chainPoints.Count;
        lr.SetPositions(chainPoints.ToArray());
        DrawLine();
        Chain(closestEnemy);
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

    private void ResetLightning() {
        meshCollider.enabled = false;
        lr.enabled = false;
        colliders.Clear();
        hitEnemyIds.Clear();
        chainPoints.Clear();
    }

    // Finds closest enemy to player's weapon position (must be inside of weapon hitbox)
    private Collider FindClosestEnemyInHitbox() {
        float min = 10000f;

        Collider closestEnemy = null;

        foreach (Collider c in colliders) {
            float distance = Vector3.Distance(weaponPosition.position, c.transform.position);
            if (distance < min) {
                min = distance;
                closestEnemy = c;
            }
        }

        if (closestEnemy != null) {
            enemyPosition = closestEnemy.transform;
        }

        return closestEnemy;
    }

    // Finds closest enemy to given position (max distance away is chainRadius)
    private Collider FindClosestEnemy(Vector3 position) {
        //if (position == null) {
        //    return null;
        //}

        float min = 10000f;
        Collider closestEnemy = null;

        Collider[] nearbyEnemies = Physics.OverlapSphere(position, chainRadius, enemyMask);

        foreach (Collider c in nearbyEnemies) {

            if (hitEnemyIds.Contains(c.gameObject.GetInstanceID())) {
                continue; // skip to next collider
            }

            float distance = Vector3.Distance(transform.position, c.transform.position);
            if (distance < min) {
                min = distance;
                closestEnemy = c;
            }
        }

        return closestEnemy;
    }

    private void DrawLine() {
        lr.enabled = true;
    }

    private void OnDrawGizmos() {
        if (enemyPosition) {
            Gizmos.DrawWireSphere(enemyPosition.position, chainRadius);
        }
    }

    private void OnTriggerStay(Collider other) {
        GameObject hitObject = other.gameObject;
        if (hitObject.layer == LayerMask.NameToLayer("Enemy")) {
            if (!colliders.Contains(other))
                colliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            colliders.Remove(other);
        }
    }

}