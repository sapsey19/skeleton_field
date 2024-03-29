﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	Vector3 velocity;
	Rigidbody rb;

	void Start() {
		//Application.targetFrameRate = 144;
		rb = GetComponent<Rigidbody>();
	}

	public void Move(Vector3 _velocity) {
		velocity = _velocity;
	}

	public void LookAt(Vector3 lookPoint) {
		Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
		transform.LookAt(heightCorrectedPoint);
	}

	public void FixedUpdate() {
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
	}
}