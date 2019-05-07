using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	ShootingAgent agent;
	Rigidbody rb;

	private void Start() {
		rb = GetComponent<Rigidbody>();
		rb.AddForce(transform.forward * 200f);
	}

	// Update is called once per frame
	void Update()
    {
		
    }

	public void SetOwner(ShootingAgent agent) {
		this.agent = agent;
	}

	private void OnCollisionEnter(Collision collision) {
		//Debug.Log("coll + " + collision.transform.tag);
		if (collision.transform.CompareTag("wall")) {
			agent.AddReward(-0.1f);
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.transform.CompareTag("enemy")) {
			other.gameObject.GetComponent<Zombie>().TakeDamage();
			Destroy(gameObject);
		}
	}
}
