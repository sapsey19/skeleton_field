using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {
    public Transform weaponHold;
    public Gun startingWeapon;

    Gun equippedGun;
    private void Start() {
        if (startingWeapon != null)
            EquipWeapon(startingWeapon);
    }
    public void EquipWeapon(Gun gunToEquip) {
        if (equippedGun != null)
            Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation);
        equippedGun.transform.parent = weaponHold;
    }

    public void Shoot() {
        if(equippedGun != null) {
            equippedGun.Shoot();
        }
    }
}
