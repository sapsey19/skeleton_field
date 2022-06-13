using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChainingBullet : MonoBehaviour {
    public LayerMask collisionMask;
    public LayerMask enemyMask;
    public Color trailColor;
    public float speed;
    public float damage = 1;
    public int chainNumber;
    public float chainRadius;

    float lifeTime = 1000;
    float hitBox = .1f;

    List<int> hitEnemyIds;
    Transform closestEnemyTarget = null;
 
    private void Start() {
        Destroy(gameObject, lifeTime);

        hitEnemyIds = new List<int>();

        Collider[] initalCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initalCollisions.Length > 0) {
            OnHitObject(initalCollisions[0], transform.position);
        }

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

    void Update() {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);

        if(closestEnemyTarget != null)
            gameObject.transform.LookAt(closestEnemyTarget.position);
    }

    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }

    void CheckCollisions(float moveDistance) {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, moveDistance + hitBox, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint) {
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null && chainNumber > 0) {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);

            if(c.GetComponent<Enemy>() != null)
                hitEnemyIds.Add(c.GetInstanceID());
            
            FindClosestEnemy(c, hitPoint);
        }
        else
          Destroy(gameObject);
    }

    void FindClosestEnemy(Collider hitEnemyCollider, Vector3 hitPoint) {
        Collider[] nearbyEnemies = Physics.OverlapSphere(hitPoint, chainRadius, enemyMask);
    
        float min = 10000f;
        int i = 0;

        Transform closestEnemy = null;

        foreach (Collider c in nearbyEnemies) { 
            if (!hitEnemyIds.Contains(c.GetInstanceID())) {
                float distance = Vector3.Distance(transform.position, c.transform.position);
                if (distance < min) {
                    min = distance;
                    closestEnemy = c.transform;
                }
                i++;
            }
        }
        if (closestEnemy == null) {
            Destroy(gameObject);
            hitEnemyIds.Clear();
            return;
        }
        closestEnemyTarget = closestEnemy;
        gameObject.transform.LookAt(closestEnemy.position);
        chainNumber--;
    }
}
