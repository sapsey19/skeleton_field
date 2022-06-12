using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity {

	public float moveSpeed = 5;

	public Crosshair crossHair;

	Camera viewCamera;
	PlayerController controller;
	GunController gunController;

	// threshold at which gun no longer rotates toward cursor 
	float gunRotateTheshold = 1.18f;

	protected override void Start() {
		base.Start();
		Cursor.visible = false;
		controller = GetComponent<PlayerController>();
		gunController = GetComponent<GunController>();
		viewCamera = Camera.main;
	}

	void Update() {
		//Movement input
		Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		Vector3 moveVelocity = moveInput.normalized * moveSpeed;
		controller.Move(moveVelocity);

		//Look input 
		Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;

		if (groundPlane.Raycast(ray, out rayDistance)) {
			Vector3 point = ray.GetPoint(rayDistance);
			//Debug.DrawLine(ray.origin, point, Color.red);
			//Debug.DrawRay(ray.origin,ray.direction * 100,Color.red);
			controller.LookAt(point);
			crossHair.transform.position = point;
			crossHair.DetectTarget(ray);

            if ((new Vector3(point.x, point.y, point.z) - new Vector3(transform.position.x, transform.position.y, transform.position.z)).sqrMagnitude > Mathf.Pow(gunRotateTheshold, 2)) {
                gunController.Aim(point);
            }
        }

		//Weapon input
		if(Input.GetMouseButton(0)) {
			gunController.OnTriggerHold();
        }

		if (Input.GetMouseButtonUp(0)) {
			gunController.OnTriggerRelease();
		}
	}
}