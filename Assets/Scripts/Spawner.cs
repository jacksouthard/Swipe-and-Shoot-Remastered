using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	public static Dictionary<string, Spawner> spawners = new Dictionary<string, Spawner>();

	public GameObject prefab;
	public int maxObjects;
	public float spawnRate;
	public float minSpawnRange;

	int count = 0;
	float spawnTimer;

	Transform player;
	List<Vector3> spawnPoints = new List<Vector3>();

	void Awake () {
		if (spawners.Count == 0) {
			LoadSpawners ();
		}

		player = GameObject.FindObjectOfType<PlayerController> ().transform;
	}

	void LoadSpawners() {
		Spawner[] allSpawners = GameObject.FindObjectsOfType<Spawner> ();
		foreach (Spawner spawner in allSpawners) {
			spawners.Add (spawner.gameObject.name, spawner);
		}
	}

	void Start () {
		for (int i = 0; i < transform.childCount; i++) {
			Transform child = transform.GetChild (i);
			if (child.name.Contains ("SpawnPoint")) {
				spawnPoints.Add (child.position);
			}
		}

		spawnTimer = spawnRate;
	}
	
	void Update () {
		if (count < maxObjects) {
			spawnTimer -= Time.deltaTime;
			if (spawnTimer <= 0f) {
				spawnTimer = spawnRate;

				SpawnEnemy ();
			}
		}
	}

	public void SpawnerObjectDespawn () {
		count--;
	}

	void SpawnEnemy () {
		Vector3 spawnPoint = FindValidSpawnPoint ();

		GameObject enemy = Instantiate (prefab, spawnPoint, Quaternion.identity, transform);

		count++;
	}

	Vector3 FindValidSpawnPoint() {
		Vector3 spawnPoint;
		do {
			int index = Random.Range (0, spawnPoints.Count);
			spawnPoint = spawnPoints [index];
		} while (Vector3.Distance(spawnPoint, player.position) < minSpawnRange);

		return spawnPoint;
	}
}
