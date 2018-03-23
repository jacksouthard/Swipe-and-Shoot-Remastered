using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour {
	public bool requiresGround;

	float minPlayerDistance;
	BoxCollider box;
	Vector3 halfExtents;
	Transform player;
	bool needsToCheckForPlayer;

	void Start() {
		minPlayerDistance = GetComponentInParent<Spawner> ().minSpawnRange;
		box = GetComponent<BoxCollider> ();
		halfExtents = box.size / 2;

		//if the box is bigger than the player range anyways, we will never have to check if the player is too close
		needsToCheckForPlayer = (minPlayerDistance > 0) && ((halfExtents.x + 2 < minPlayerDistance) || (halfExtents.z + 2 < minPlayerDistance)); //+2 to give extra room

		if (minPlayerDistance > 0) {
			player = GameObject.FindObjectOfType<PlayerController> ().transform;
		}
	}

	public bool shouldSpawn {
		get {
			return (!needsToCheckForPlayer || !PlayerIsInRange (transform.position));
		}
	}

	public Vector3 FindRandomSpawnPoint() {
		int attempts = 0;
		while(true) {
			attempts++;
			if (attempts > 30) {
				Debug.Log("Too many attempts on " + transform.parent.name + " - " + gameObject.name);
				return Vector3.up;
			}

			Vector3 randomPoint = transform.position + transform.rotation * (box.center + new Vector3 (Random.Range(-halfExtents.x, halfExtents.x), halfExtents.y, Random.Range(-halfExtents.z, halfExtents.z)));

			if (minPlayerDistance > 0 && PlayerIsInRange (randomPoint)) {
				continue;
			}

			if (requiresGround) {
				RaycastHit hitInfo;
				Physics.Raycast (randomPoint, Vector3.down, out hitInfo, box.size.y);

				if (hitInfo.collider != null && hitInfo.collider.gameObject.layer == 10) { //if we hit ground
					return hitInfo.point;
				}
			} else {
				return randomPoint;
			}
		}

	}

	Vector3 ZeroedPosition(Vector3 position) {
		return new Vector3 (position.x, 0f, position.z);
	}

	bool PlayerIsInRange(Vector3 position) {
		return Vector3.Distance (ZeroedPosition (position), ZeroedPosition (player.position)) < minPlayerDistance;
	}
}
