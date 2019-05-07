using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingAgent : Agent
{
	public int health = 100;
	public int maxHealth = 100;

	public Transform startLocation;
	private Rigidbody rb;

	private RayPerception rayPer;

	public ArenaScript arena;

	public ShootingAgent shootingAgent;

	public float distanceFromEnemy;

	public HitFlash hitFlash;

	public override void InitializeAgent() {
		base.InitializeAgent();
		rb = GetComponent<Rigidbody>();
		rayPer = GetComponent<RayPerception>();
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
		MoveAgent(vectorAction);
		AddReward(0.00001f); // reward survival time
		AddReward(-distanceFromEnemy / 10000f);
	}

	public void MoveAgent(float[] act) {
		var dirToGo = Vector3.zero;

		var action = Mathf.FloorToInt(act[0]);

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
				dirToGo = Vector3.zero;
				break;
		}

		float speed = 5f;
		rb.velocity = Vector3.MoveTowards(rb.velocity, new Vector3(dirToGo.x * speed, rb.velocity.y, dirToGo.z * speed), Time.deltaTime * speed);

		if (transform.position.y < -10) {
			TakeDamage();
		}
	}

	public override void CollectObservations() {
		AddVectorObs(distanceFromEnemy);
		AddVectorObs((float)health / maxHealth);
		AddVectorObs(transform.rotation.y);

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

	public void TakeDamage() {
		hitFlash.Flash();
		if (health > 0) {
			health--;
			AddReward(-0.01f);
		}
		if (health == 0) {
			AddReward(-1f);
			shootingAgent.Done();
			Done();
		}
	}
}
