using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    public enum FireMode {Auto, Burst, Single};
    public FireMode fireMode;

    [Header("Projectile")]
    public Transform[] projectileSpawn;
    public ChainingBullet projectile;
    public float delay = 100;
    public float muzzleVelocity = 35;
    public int burstCount;

    [Header("Recoil")]
    public Vector2 recoilMinMax= new Vector2(.05f, .02f); //x is min, y is max
    public Vector2 recoilAngleMinMax = new Vector2(3, 5);
    public float recoilReturnTime = .1f;
    public float recoilRotationReturnTime = .1f;

    [Header("Effects")]
    public Transform shell;
    public Transform shellEjection;
    MuzzleFlash muzzleFlash;
    public AudioClip shootAudio;

    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;

    Vector3 recoilSmoothDampVelocity;
    float recoilAngle;
    float recoilRotationSmoothDampVelocity;

    private void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
    }

    private void Update() {
        //transform.localEulerAngles = Vector3.left * recoilAngle;
    }

    private void LateUpdate() {
        // animate the recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilReturnTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilRotationReturnTime);
        transform.localEulerAngles = Vector3.left * recoilAngle;
    }
    void Shoot() {
        if (Time.time > nextShotTime) {
            if (fireMode == FireMode.Burst) {
                if (shotsRemainingInBurst == 0) return;
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single) {
                if (!triggerReleasedSinceLastShot) return;
            }

            for (int i = 0; i < projectileSpawn.Length; i++) {
                nextShotTime = Time.time + delay / 1000;
                ChainingBullet newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation);
                newProjectile.SetSpeed(muzzleVelocity);
            }
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(recoilMinMax.x, recoilMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        } 
    }

    public void Aim(Vector3 aimPoint) {
        transform.LookAt(aimPoint);
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
