using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Zombie : MonoBehaviour
{
	public ArenaScript arena;
	public Transform player;

	public ShootingAgent agent;

	public int health = 50;

	public HitFlash hitFlash;

	// Update is called once per frame
	void FixedUpdate() {
		Collider[] colls = Physics.OverlapSphere(transform.position, 1);
		Collider col = colls.Where(x => x.gameObject != this.gameObject && x.CompareTag("enemy")).FirstOrDefault();
		if(col != null) {
			Vector3 dirToOther = (col.transform.position - transform.position).normalized;
			transform.position -= dirToOther * Time.deltaTime;
		}
		transform.position = Vector3.MoveTowards(transform.position, player.position, Time.deltaTime * 1.5f);
		transform.LookAt(player.position);
	}

	public void SetAgent(Transform agentT) {
		player = agentT;
		agent = player.GetComponent<ShootingAgent>();
	}

	public void TakeDamage() {
		hitFlash.Flash();
		//Debug.Log("damaag");
		if (health > 0) {
			agent.AddReward(0.01f);
			health -= 10;
			if (health <= 0) {
				agent.AddReward(0.5f);
				Die();
			}
		}
	}

	public void Die() {
		arena.ZombieDied(gameObject);
	}

	public void Respawn() {
		health = 50;
	}
}
