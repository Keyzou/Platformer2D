using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class BulletController : MonoBehaviour {

	public float bulletSpeed = 10f;
	public float dirX;
	Vector2 velocity;

	Controller2D controller;
 
	private void OnEnable() {
		controller = GetComponent<Controller2D>();
		controller.onCollision += OnCollision;
	}
	// Use this for initialization

	void OnCollision(RaycastHit2D other) {
	}

	void Start () {
		controller = GetComponent<Controller2D>();
		controller.onCollision += OnCollision;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		CalculateVelocity ();

		controller.Move(velocity, false);
	}
		void CalculateVelocity() {
		float targetVelocityX = dirX * bulletSpeed * Time.fixedDeltaTime;
		velocity.x = targetVelocityX;
	}
}
