using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airdrop : MonoBehaviour {
//	string payload;
	Animator anim;
	public GameObject pickupPrefab;
	EdgeView edgeView;
	float despawnTime = 20f;

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
		edgeView = EdgeView.Create(gameObject, true);

		StartCoroutine (Despawn());
	}

	void SpawnPayload () {
		Data data = EquipmentManager.instance.GetRandomData ().ToAssetData();
		GameObject pickup = Instantiate (pickupPrefab, transform.TransformPoint(Vector3.up * 0.2f), Quaternion.identity);
		pickup.GetComponent<Pickup> ().Init (data, true);
	}

	IEnumerator Despawn () {
		yield return new WaitForSeconds (despawnTime);

		if (Spawner.spawners.ContainsKey("AirdropSpawner")) {
			Spawner.spawners["AirdropSpawner"].SpawnerObjectDespawn ();
		}

		edgeView.Destroy ();

		Destroy (gameObject);
	}
}
