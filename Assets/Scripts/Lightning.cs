using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

public class Lightning : MonoBehaviour {
    public LayerMask enemyMask;

    public Transform weapon; 
    public Transform cursor; 

    private Enemy directHitEnemy;

    public float chainRadius;     //how close enemies must be to chain off of 


    [SerializeField]
    private List<Enemy> chainEnemies = new List<Enemy>(); //list of chain points for lightning chain effect. maybe use linkedList? 

    private bool hittingEnemy;
    private bool firing;

    private LineRenderer lr;
    private MeshCollider coneHitbox;

    private List<Collider> colliders = new List<Collider>();

    private List<Collider> GetColliders() { return colliders; }

    private int chainNum;
    public int maxChains;

    private void Start() {
        coneHitbox = gameObject.GetComponent<MeshCollider>();
        coneHitbox.enabled = false;

        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        //chainNum = maxChains;
    }

    private void Update() {

        if (Input.GetMouseButton(0)) {
            coneHitbox.enabled = true;
            firing = true;
        }
        if (Input.GetMouseButtonUp(0)) {
            hittingEnemy= false;
            coneHitbox.enabled = false;
            lr.enabled = false;
            firing = false;
            colliders.Clear();
            ResetChain();
        }

        if (firing) {
            // Lightning latches on to nearest enemy in hitbox
            directHitEnemy = FindClosestEnemyInHitbox();

            if (hittingEnemy && directHitEnemy) {
                DrawCurvedLine(weapon.position, cursor.position, directHitEnemy.transform.position);

                directHitEnemy.isChaining = true;
                
                //chain off of all points in chainPoints
                Chain(directHitEnemy);
                DrawChains();
            }
            else {
                DrawLine(weapon.position, cursor.position);
                ResetChain();
            }
        }

        //idk why this works but it does 
        if (directHitEnemy)
            directHitEnemy.isChaining = false;
    }

    private void FixedUpdate() {
        if(directHitEnemy)
            directHitEnemy.TakeDamage(directHitEnemy.lightningDamageReceived);
    }

    private void ResetChain() {
        foreach (Enemy e in chainEnemies) {
            e.isChaining= false;
        }
        chainEnemies.Clear();
        chainNum = maxChains;
    }

    //recursive function that finds nearby enemies that are not already getting hit 
    //Enemy.isChaining prevents lightning from chaining to the same enemy
    private void Chain(Enemy enemy) {
        if (enemy == null || chainNum <= 0) {
            return;
        }

        Enemy closestEnemy = enemy.FindClosestEnemy(chainRadius, enemyMask);

        if (!closestEnemy) {
            return;
        }

        closestEnemy.isChaining = true;
        chainEnemies.Add(closestEnemy);

        chainNum--;
        Chain(closestEnemy);
    }


    private void DrawChains() {
        for (int i = 0; i < chainEnemies.Count(); i++) {
            if (!chainEnemies[i]) {
                chainEnemies.RemoveAt(i);
                lr.positionCount--;
            }
            else {
                lr.positionCount++;
                //prevents errors when chainEnemies gets cleared 
                if(lr.positionCount != i+14) {
                    return;
                }
                //offset from Bezier curved line
                lr.SetPosition(13 + i, chainEnemies[i].transform.position);
            }
        }
    }

    private Enemy FindClosestEnemyInHitbox() {
        List<Collider> inHitboxColliders = GetColliders();

        float min = float.MaxValue;

        Collider closestEnemy = null;

        foreach (Collider c in inHitboxColliders) {
            //if collider has been destroyed (enemy killed), skip
            if (!c) continue;
            float distance = Vector3.Distance(weapon.position, c.transform.position);
            if (distance < min) {
                min = distance;
                closestEnemy = c;
            }
        }
        if (!closestEnemy) return null;

        return closestEnemy.gameObject.GetComponent<Enemy>();
    }


    private void DrawLine(Vector3 point1, Vector3 point2) {
        lr.enabled = true;
        lr.positionCount = 2;
        lr.SetPosition(0, point1);
        lr.SetPosition(1, point2);
    }

    private void DrawCurvedLine(Vector3 point1, Vector3 point2, Vector3 point3) {
        lr.enabled = true;
        lr.positionCount = 3;
        lr.SetPosition(0, point1);
        lr.SetPosition(1, point2);
        lr.SetPosition(2, point3);

        BezierCurve();

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

    private void OnDrawGizmos() {
        if (directHitEnemy) {
            Gizmos.DrawWireSphere(directHitEnemy.transform.position, chainRadius);
        }
    }

    private void BezierCurve() {
        List<Vector3> pointList = new List<Vector3>();
        for (float ratio = 0; ratio <= 1; ratio += 1.0f / 12) {
            Vector3 tangentLineVertex1 = Vector3.Lerp(weapon.position, cursor.position, ratio);
            Vector3 tangentLineVertex2 = Vector3.Lerp(cursor.position, directHitEnemy.transform.position, ratio);
            Vector3 bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
            pointList.Add(bezierPoint);
        }
        lr.positionCount = pointList.Count;
        lr.SetPositions(pointList.ToArray());
    }
}
