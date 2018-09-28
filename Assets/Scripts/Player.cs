﻿using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	public float moveSpeed = 10;

	public Animator animator;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	[Range(0, 1)]
	public float crouchingModifier = 0.5f;
	float timeToWallUnstick;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

	Vector2 directionalInput;
	bool wallSliding;
	[HideInInspector]
	public bool crouching;

	int wallDirX;
	int dirX = 1;

	Grid mapGrid;
	Tilemap tilemap;

	void Start() {
		controller = GetComponent<Controller2D> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
		
		mapGrid = FindObjectOfType<Grid>();
		tilemap = mapGrid.GetComponentInChildren<Tilemap>();
	}

	void FixedUpdate() {

		if (directionalInput.y == -1 && controller.collisions.below && !crouching) {
			controller.collider.size = new Vector2(controller.collider.size.x, 1f);
			controller.collider.offset = new Vector2(controller.collider.offset.x, -0.5f);
			controller.UpdateRaycastOrigins();
			controller.CalculateRaySpacing();
			crouching = true;
		} else if (directionalInput.y != -1 && crouching && !tilemap.HasTile(mapGrid.WorldToCell(transform.position + 0.5f * new Vector3(1, 1, 0)))) {
			controller.collider.size = new Vector2(controller.collider.size.x, 2f);
			controller.collider.offset = new Vector2(controller.collider.offset.x, 0f);
			controller.UpdateRaycastOrigins();
			controller.CalculateRaySpacing();
			crouching = false;
		}

		CalculateVelocity ();
		HandleWallSliding ();


		if (controller.collisions.climbingLedge) {
			Vector2 pos = Vector2.LerpUnclamped(new Vector2(transform.position.x, transform.position.y), controller.collisions.ledgeFinalPos, moveSpeed / 2 * Time.fixedDeltaTime);
			transform.position = pos;
			Vector2 distance = controller.collisions.ledgeFinalPos - new Vector2(transform.position.x, transform.position.y);
			dirX = (int) Mathf.Sign(distance.x);
			if (distance.magnitude < 0.2f) {
				transform.position = controller.collisions.ledgeFinalPos;
				controller.collisions.climbingLedge = false;
				controller.collisions.grabLedge = false;
				if (tilemap.HasTile(mapGrid.WorldToCell(transform.position + 0.5f * new Vector3(1, 1, 0)))) {
					controller.collider.size = new Vector2(controller.collider.size.x, 1f);
					controller.collider.offset = new Vector2(controller.collider.offset.x, -0.5f);
					controller.UpdateRaycastOrigins();
					controller.CalculateRaySpacing();
					crouching = true;
				}
			}
		}

		if(controller.collisions.grabLedge) {
			velocity.y = 0;
		}
		if (controller.collisions.climbingLedge) {
			velocity.y = 0;
			velocity.x = 0;
		}

		if (directionalInput.x == -1) {
			dirX = -1;
		} else if(directionalInput.x == 1) {
			dirX = 1;
		}

		if (wallSliding && wallDirX == 1) {
			dirX = -1;
		} else if (wallSliding && wallDirX == -1) {
			dirX = 1;
		}

		GetComponent<SpriteRenderer>().flipX = dirX == -1;

		animator.SetBool("onGround", controller.collisions.below);
		animator.SetFloat("velX", Mathf.Abs(velocity.x));
		animator.SetFloat("velY", velocity.y);
		animator.SetBool("wallSliding", wallSliding);
		animator.SetBool("crouching", crouching);

		controller.Move (velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above || controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
		}
	}
	public void SetDirectionalInput (Vector2 input) {
		directionalInput = input;
	}

	public void OnJumpInputDown() {
		if (wallSliding) {
			if (wallDirX == directionalInput.x) {
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
				if (controller.collisions.grabLedge)
					controller.collisions.climbingLedge = true;
			}
			else if (directionalInput.x == 0) {
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
				
				if (controller.collisions.grabLedge)
					controller.collisions.climbingLedge = true;
			}
			else {
				velocity.x = -wallDirX * wallLeap.x;
				velocity.y = wallLeap.y;

				controller.collisions.grabLedge = false;
			}
		}
		if (controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				if (directionalInput.x != -Mathf.Sign (controller.collisions.slopeNormal.x)) { // not jumping against max slope
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			} else {
				velocity.y = maxJumpVelocity;
			}
		}
	}

	public void OnJumpInputUp() {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}
		

	List<Vector3> availablePlaces = new List<Vector3>();

	private void OnDrawGizmos() {
		Vector3 pos = transform.position + Vector3.down * 1.5f;
		Gizmos.DrawLine(pos + Vector3.up * 0.5f, pos - Vector3.up * 0.5f);
		Gizmos.DrawLine(pos + Vector3.left * 0.5f, pos - Vector3.left * 0.5f);
	}
	void HandleWallSliding() {
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0 || controller.collisions.grabLedge) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (directionalInput.x != wallDirX && directionalInput.x != 0) {
					timeToWallUnstick -= Time.deltaTime;

				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
			else {
				timeToWallUnstick = wallStickTime;
			}

		}

	}

	void CalculateVelocity() {
		float targetVelocityX = directionalInput.x * moveSpeed * (crouching ? crouchingModifier : 1);
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}
}