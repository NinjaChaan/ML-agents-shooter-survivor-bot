using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingAgent : Agent
{
	public float timeSinceLastShot = 0f;

	public Transform shotSpot;

	private RayPerception rayPer;

	private GameObject bulletPrefab;

	public ArenaScript arena;

	public MovingAgent movingAgent;

	public bool canShoot = true;

	public bool aimingAtEnemy = false;

	public float angleToPosition;

	public override void InitializeAgent() {
		bulletPrefab = Resources.Load("Bullet") as GameObject;
		base.InitializeAgent();
		rayPer = GetComponent<RayPerception>();
	}

	public override void AgentReset() {
		canShoot = true;

		aimingAtEnemy = false;
	}

	public override void AgentAction(float[] vectorAction, string textAction) {
		RaycastHit hit;
		// Does the ray intersect any objects excluding the player layer
		if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10f)) {
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

			if (hit.transform.CompareTag("enemy")) {
				if (hit.transform.GetComponent<Zombie>().arena == this.arena) {
					aimingAtEnemy = true;
				} else {
					aimingAtEnemy = false;
				}
			} else {
				aimingAtEnemy = false;
			}
		}

		if (aimingAtEnemy) {
			AddReward(0.001f);
		}

		MoveAgent(vectorAction);
		timeSinceLastShot += Time.deltaTime;
		AddReward(-timeSinceLastShot / 1000f);

		if(angleToPosition < 45f) {
			AddReward(angleToPosition / 100000f);
		} else {
			AddReward(-angleToPosition / 1000000f);
		}
	}

	public void MoveAgent(float[] act) {
		var rotateDir = Vector3.zero;

		var action = Mathf.FloorToInt(act[0]);
		var rotateAction = Mathf.FloorToInt(act[1]);

		switch (action) {
			case 1:
				if (canShoot) {
					Shoot();
				}
				break;
		}

		switch (rotateAction) {
			case 1:
				rotateDir = transform.up * 1f;
				break;
			case 2:
				rotateDir = transform.up * -1f;
				break;
		}

		transform.Rotate(rotateDir, Time.deltaTime * 200f);
	}

	public override void CollectObservations() {
		AddVectorObs(canShoot);
		AddVectorObs(arena.timeSinceLastKill / 60f);
		AddVectorObs(timeSinceLastShot / 10f);
		AddVectorObs(aimingAtEnemy);

		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10);
		Transform closestEnemy = null;

		foreach (var collider in hitColliders) {
			Collider c = collider;
			if (c.transform.CompareTag("enemy")) {
				if (c.transform.GetComponent<Zombie>().arena == this.arena) {
					if(closestEnemy == null) {
						closestEnemy = c.transform;
					} else {
						if(Vector3.Distance(closestEnemy.position, transform.position) > Vector3.Distance(c.transform.position, transform.position)) {
							closestEnemy = c.transform;
						}
					}
				}
			}
		}
		if (closestEnemy != null) {
			Vector3 toPosition = (closestEnemy.position - transform.position).normalized;
			angleToPosition = Vector3.Angle(transform.forward, toPosition);
			movingAgent.distanceFromEnemy = Vector3.Distance(closestEnemy.position, transform.position);

			AddVectorObs(angleToPosition / 180f);
		} else {
			AddVectorObs(0);
		}

		const float rayDistance = 5f;
		float[] rayAngles = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180,
							190, 200,210, 220, 230, 240, 250, 260, 270, 280, 290, 300, 310, 320, 330, 340, 350};

		string[] detectableObjects = { "enemy", "wall" };
		AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
	}

	public void Shoot() {
		timeSinceLastShot = 0f;
		StartCoroutine(ShootTimer());
		GameObject bullet = Instantiate(bulletPrefab, shotSpot.position, transform.rotation);
		bullet.GetComponent<Bullet>().SetOwner(this);
	}

	IEnumerator ShootTimer() {
		canShoot = false;
		yield return new WaitForSeconds(0.1f);
		canShoot = true;
	}
}
