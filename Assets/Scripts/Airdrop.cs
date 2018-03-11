using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airdrop : MonoBehaviour {
//	string payload;
	Animator anim;
	public GameObject pickupPrefab;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	void OnCollisionEnter (Collision coll) {
		if (coll.gameObject.layer == 10) {
			Deploy ();
		}
	}

	void Deploy () {
		anim.SetTrigger ("Open");
	}

	void SpawnPayload () {
		Data data = DataManager<Data>.GetAnyRandomData ();
		GameObject pickup = Instantiate (pickupPrefab, transform.TransformPoint(Vector3.up * 0.2f), Quaternion.identity);
		pickup.GetComponent<Pickup> ().Init (data, true);
	}

	void Despawn () {
		if (Spawner.spawners.ContainsKey("AirdropSpawner")) {
			Spawner.spawners["AirdropSpawner"].SpawnerObjectDespawn ();
		}

		Destroy (gameObject);
	}
}
