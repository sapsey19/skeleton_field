using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour {

  
    MuzzleFlash muzzleFlash;
    Enemy[] enemies;

    float nextShotTime;
    void Shoot() {
        Debug.Log("Lightning!");
    }

    public void OnTriggerHold() {
        Shoot();
    }

}
