using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	ShootingAgent agent;
	Rigidbody rb;

    private void OnEnable() {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.AddForce(transform.forward * 300f);
    }

    public void SetOwner(ShootingAgent agent) {
		this.agent = agent;
	}

	private void OnCollisionEnter(Collision collision) {
		//Debug.Log("coll + " + collision.transform.tag);
		if (collision.transform.CompareTag("wall")) {
			agent.AddReward(-0.1f);
            SimplePool.Despawn(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.transform.CompareTag("enemy")) {
			other.gameObject.GetComponent<Zombie>().TakeDamage();
            SimplePool.Despawn(gameObject);
        }
	}
}
