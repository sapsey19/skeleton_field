using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public LayerMask collisionMask;
    public float speed;
    public float damage = 1;

    void Update() {
        float moveDistance = speed * Time.deltaTime;
        //Debug.Log(moveDistance);
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    public void SetSpeed(float newSpeed) {
        speed = newSpeed;
    }

    void CheckCollisions(float moveDistance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance+.1f, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit);
        }
    }

    //private void OnTriggerEnter(Collider other) {
    //    if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
    //        Debug.Log(other.gameObject.name);
    //        Destroy(gameObject);
    //    }
    //}

    void OnHitObject(RaycastHit hit) {
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
        if (damageableObject != null)
            damageableObject.TakeHit(damage, hit);
        //Debug.Log(hit.collider.gameObject.name);
        Destroy(gameObject);
    }
}
