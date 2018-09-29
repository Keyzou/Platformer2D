using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class EnemyController : MonoBehaviour {

	Controller2D controller;
	Animator animator;
	int health = 100;
	public float immunityTime = 1f; 
	float currentImmunity;
	float gravity;
	Vector2 velocity;
	Vector2 knockbackDir;
	public Vector2 knockbackForce;
	public float knockbackTime = 0.5f;
	float currentKnockbackTime;
	Vector2 currentKnockbackForce;
	// Use this for initialization
	void Start () {
		controller = GetComponent<Controller2D>();
		controller.onCollision += OnCollision;
		animator =  GetComponent<Animator>();
		gravity = -50f;
	}
	
	public void OnCollision(RaycastHit2D hit) {
	}

	public void Knockback(Vector2 dir) {
		if (currentKnockbackTime <= 0) {
			knockbackDir = dir;
			velocity.y = 1f;
			currentKnockbackTime = knockbackTime;
		}
	}

	public void Die() {
		Destroy(gameObject);
	}

	public void TakeDamage(int damage){
		if (currentImmunity <= 0) {
			health -= damage;
			currentImmunity = immunityTime;
		}
	}

	
	void CalculateVelocity() {
		if (currentKnockbackTime <= 0) {
			velocity.y += gravity * Time.fixedDeltaTime; 
			velocity.x = 0;
		} else {
			float ratio = currentKnockbackTime / knockbackTime;
			// a(x-d)² + bx + c => a = -2; b = 0; c = 0.5; d = 0.5
			velocity.y = -2 * (ratio - 0.5f) * (ratio - 0.5f) + 0.5f;
			velocity.x = -knockbackDir.x *  currentKnockbackForce.x  * Time.fixedDeltaTime;
		}
	}

	// Update is called once per frame
	void FixedUpdate () {
		CalculateVelocity();

		if (currentImmunity > 0) {
			currentImmunity -= Time.fixedDeltaTime;
			GetComponent<SpriteRenderer>().color = Color.red;
		} else {
			GetComponent<SpriteRenderer>().color = Color.white;
		}

		if (currentKnockbackTime > 0) {
			float ratio = 1 - currentKnockbackTime / knockbackTime;
			currentKnockbackForce = Vector2.Lerp(knockbackForce, Vector2.zero, ratio);
			currentKnockbackTime -= Time.fixedDeltaTime;
		} else {
			currentKnockbackForce = Vector2.zero;
		}

		if (health <= 0) {
			GetComponent<SpriteRenderer>().color = Color.white;
			controller.enabled = false;
			velocity = Vector2.zero;
			animator.SetTrigger("die");
		}

		controller.Move(velocity, false);

		if (controller.collisions.above || controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
		}
	}
}
