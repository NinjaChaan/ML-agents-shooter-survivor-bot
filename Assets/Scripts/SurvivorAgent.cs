using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SurvivorAgent : Agent
{
	public int health = 100;
	public int maxHealth = 100;

	public float timeSinceLastShot = 0f;

	public Transform startLocation;
	private Rigidbody rb;
	public Transform shotSpot;

	private RayPerception rayPer;

	private GameObject bulletPrefab;

	public List<GameObject> closestEnemies = new List<GameObject>();
	public GameObject closestMedkit;

	public ArenaScript arena;

	public bool canShoot = true;

	public bool aimingAtEnemy = false;

	public override void InitializeAgent() {
		bulletPrefab = Resources.Load("Bullet") as GameObject;
		base.InitializeAgent();
		rb = GetComponent<Rigidbody>();
		rayPer = GetComponent<RayPerception>();
		StartCoroutine(ScanForObjects());
	}

	public override void AgentReset() {
		health = maxHealth;

		transform.position = startLocation.position;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		transform.rotation = startLocation.rotation;

		arena.RespawnZombies();
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
			AddReward(0.0001f);
		}

		MoveAgent(vectorAction);
		AddReward(0.00001f); // reward survival time
		timeSinceLastShot += Time.deltaTime;
		AddReward(-timeSinceLastShot / 1000f);
	}

	public void MoveAgent(float[] act) {
		var dirToGo = Vector3.zero;
		var rotateDir = Vector3.zero;

		var action = Mathf.FloorToInt(act[0]);
		var rotateAction = Mathf.FloorToInt(act[1]);

		switch (action) {
			case 1:
				dirToGo = transform.forward * 1f;
				break;
			case 2:
				dirToGo = transform.forward * -1f;
				break;
			case 3:
				dirToGo = transform.right * 1f;
				break;
			case 4:
				dirToGo = transform.right * -1f;
				break;
			case 5:
				if (canShoot) {
					Shoot();
				}
				break;
			case 6:
				dirToGo = Vector3.zero;
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

		transform.Rotate(rotateDir, Time.deltaTime * 50f);

		float speed = 0.25f;
		rb.velocity = new Vector3(rb.velocity.x + dirToGo.x * speed, rb.velocity.y, rb.velocity.z + dirToGo.z * speed);

		if (transform.position.y < -10) {
			TakeDamage();
		}
	}

	public override void CollectObservations() {
		AddVectorObs(canShoot);
		AddVectorObs((float)health/maxHealth);
		AddVectorObs(arena.timeSinceLastKill / 60f);
		AddVectorObs(timeSinceLastShot / 10f);
		AddVectorObs(aimingAtEnemy);

		const float rayDistance = 5f;
		float[] rayAngles = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180,
							190, 200,210, 220, 230, 240, 250, 260, 270, 280, 290, 300, 310, 320, 330, 340, 350};

		string[] detectableObjects = { "enemy", "medkit", "wall" };
		AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));

		//int i = 0;
		//foreach (var enemy in closestEnemies.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).Take(3)) {
		//	i++;
		//	AddVectorObs(enemy.transform.position - transform.position);
		//}

		//for (; i < 3; i++) {
		//	AddVectorObs(Vector3.zero);
		//}


		//if (closestMedkit == null) {
		//	AddVectorObs(Vector3.zero);
		//} else {
		//	AddVectorObs(closestMedkit.transform.position - transform.position);
		//}
	}

	private void OnTriggerStay(Collider other) {
		if (other.transform.CompareTag("enemy")) {
			TakeDamage();
		}
	}

	//private void OnCollisionStay(Collision collision) {

	//}

	private void OnTriggerEnter(Collider other) {
		if (other.transform.CompareTag("medkit")) {
			Destroy(other.gameObject);
			AddReward(0.001f);
		}
	}

	public override void AgentOnDone() {

	}

	public void Shoot() {
		timeSinceLastShot = 0f;
		StartCoroutine(ShootTimer());
		GameObject bullet = Instantiate(bulletPrefab, shotSpot.position, transform.rotation);
		//bullet.GetComponent<Bullet>().SetOwner(this);
	}

	IEnumerator ShootTimer() {
		canShoot = false;
		yield return new WaitForSeconds(0.1f);
		canShoot = true;
	}

	public void TakeDamage() {
		if (health > 0) {
			health--;
			AddReward(-0.01f);
		}
		if (health == 0) {
			AddReward(-2f);
			Done();
		}
	}

	IEnumerator ScanForObjects() {
		while (true) {
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10);

			closestEnemies.Clear();
			closestMedkit = null;

			foreach (var collider in hitColliders) {
				Collider c = collider;
				if (c.transform.CompareTag("enemy")) {
					if (c.transform.GetComponent<Zombie>().arena == this.arena) {
						closestEnemies.Add(c.gameObject);
					}
				} else if (c.transform.CompareTag("medkit")) {
					if (closestMedkit == null) {
						closestMedkit = c.gameObject;
					} else if (Vector3.Distance(c.transform.position, transform.position) <
						 Vector3.Distance(closestMedkit.transform.position, transform.position)) {
						closestMedkit = c.gameObject;
					}
				}
			}

			yield return new WaitForSeconds(0.1f);

			yield return null;
		}
	}
}
