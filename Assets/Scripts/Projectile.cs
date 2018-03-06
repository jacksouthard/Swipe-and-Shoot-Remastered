using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	void OnCollisionEnter(Collision other) {
		Explosion.Create (transform.position, 5, 100000, 20);
		Destroy (gameObject);
	}
}
