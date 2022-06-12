using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour {

  
    MuzzleFlash muzzleFlash;
    Enemy[] enemies;

    float nextShotTime;
    void Shoot() {
        if (Time.time > nextShotTime) {

            //enemies = transform.Find("Enemies").Get<Enemy>();
            //Instantiate(shell, shellEjection.position, shellEjection.rotation);
            //muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold() {
        Shoot();
        
    }

}
