using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaScript : MonoBehaviour
{
	public Transform player;
	public ShootingAgent agent;
	public GameObject zombprefab;
	public List<Transform> zombieSpots = new List<Transform>();
	public List<GameObject> zombies = new List<GameObject>();
	public List<GameObject> deadZombies = new List<GameObject>();

	public float timeSinceLastKill;

	public int respawns = 0;

	public void Start() {
		agent = player.GetComponent<ShootingAgent>();
		foreach (var spot in zombieSpots) {
			GameObject z = Instantiate(zombprefab, spot.position, spot.rotation, spot);
			z.GetComponent<Zombie>().arena = this;
			z.GetComponent<Zombie>().SetAgent(player.transform);
			zombies.Add(z);
		}
	}

	private void FixedUpdate() {
		timeSinceLastKill += Time.deltaTime;

		if(timeSinceLastKill > 60f) {
			timeSinceLastKill = 0;
			agent.AddReward(-1f);
		}
	}

	public void ZombieDied(GameObject zombie) {
		timeSinceLastKill = 0;
		zombies.Remove(zombie);
		deadZombies.Add(zombie);
		zombie.SetActive(false);

		if(zombies.Count == 0) {
			Invoke("RespawnZombies", 1);
		}
	}


	public void RespawnZombies() {
		respawns++;
		zombies.AddRange(deadZombies);
		deadZombies.Clear();

		for (int i=0; i<zombieSpots.Count; i++) {
			zombies[i].SetActive(true);
			zombies[i].transform.position = zombieSpots[i].position;
			zombies[i].GetComponent<Zombie>().Respawn();
		}

		if(respawns == 3) {
			respawns = 0;
			//agent.movingAgent.Done();
			//agent.Done();
		}
	}
}
