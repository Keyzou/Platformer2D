using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class BulletController : MonoBehaviour {

	public Animator impactAnimator;
	public float bulletSpeed = 10f;
	public float bulletRange = 5f;
	public Vector2 direction;
	Vector2 velocity;
	Vector2 initialPos;
	Controller2D controller;
 	private void OnEnable() {
		controller = GetComponent<Controller2D>();
		controller.onCollision += OnCollision;
	}
	// Use this for initialization

	private void DestroyBullet() {
		Destroy(gameObject);
	}

	void OnCollision(RaycastHit2D other) {
		EnemyController ec = other.transform.GetComponent<EnemyController>();
		if(ec != null) {
			ec.TakeDamage(20);
			Vector2 knockbackDir = (transform.position - ec.transform.position).normalized;
			ec.Knockback(knockbackDir);
		}
		
		impactAnimator.SetTrigger("explode");
		bulletSpeed = 0f;
	}

	void Start () {
		controller = GetComponent<Controller2D>();
		controller.onCollision += OnCollision;
		initialPos = transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		CalculateVelocity ();
		if (Vector2.Distance(transform.position, initialPos) >= bulletRange)
			Destroy(gameObject);
		controller.Move(velocity, false);
	}
		void CalculateVelocity() {
		float targetVelocityX = direction.x * bulletSpeed * Time.fixedDeltaTime;
		float targetVelocityY = direction.y * bulletSpeed * Time.fixedDeltaTime;
		velocity.x = targetVelocityX;
		velocity.y = targetVelocityY;
	}
}
