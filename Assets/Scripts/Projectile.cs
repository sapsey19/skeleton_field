using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public LayerMask collisionMask;
    public Color trailColor;
    public float speed;
    public float damage = 1;

    float lifeTime = 3;
    float hitBox = .1f;

    private void Start() {
        Destroy(gameObject, lifeTime);

        Collider[] initalCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if(initalCollisions.Length > 0) {
            OnHitObject(initalCollisions[0], transform.position);
        }

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor);
    }

    void Update() {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
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
        if (damageableObject != null) {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }
        Destroy(gameObject);
    }
}
