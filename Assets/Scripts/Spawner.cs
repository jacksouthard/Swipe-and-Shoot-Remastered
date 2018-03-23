using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	public static Dictionary<string, Spawner> spawners = new Dictionary<string, Spawner>();

	public enum SpawnMode
	{
		Random,
		RoundRobin
	}
	public SpawnMode spawnMode;
	int curIndex = 0;

	public GameObject prefab;
	public int maxObjects;
	public float spawnRate;
	public float minSpawnRange;

	int count = 0;
	float spawnTimer;

	List<SpawnZone> spawnZones = new List<SpawnZone>();

	void Awake () {
		if (spawners.Count == 0) {
			LoadSpawners ();
		}

		spawnZones = new List<SpawnZone> (transform.GetComponentsInChildren<SpawnZone>());
	}

	void LoadSpawners() {
		Spawner[] allSpawners = GameObject.FindObjectsOfType<Spawner> ();
		foreach (Spawner spawner in allSpawners) {
			spawners.Add (spawner.gameObject.name, spawner);
		}
	}

	void Start () {
		spawnTimer = spawnRate;

		if (spawnMode == SpawnMode.RoundRobin) {
			curIndex = Random.Range (0, spawnZones.Count - 1);
		}
	}
	
	void Update () {
		if (count < maxObjects) {
			spawnTimer -= Time.deltaTime;
			if (spawnTimer <= 0f) {
				spawnTimer = spawnRate;

				SpawnObject ();
			}
		}
	}

	public void SpawnerObjectDespawn () {
		count--;
	}

	void SpawnObject () {
		SpawnZone chosenZone;
		int attempts = 0;
		do {
			attempts++;

			if(attempts > 30) {
				Debug.Log("Too many attempts on " + gameObject.name);
				return;
			}

			if (spawnMode == SpawnMode.Random) {
				chosenZone = spawnZones[Random.Range(0, spawnZones.Count)];
			} else {
				int newIndex = (curIndex + 1) % spawnZones.Count;
				chosenZone = spawnZones [newIndex];
				curIndex = newIndex;
			}
		} while(!chosenZone.shouldSpawn);

		Vector3 spawnPoint = chosenZone.FindRandomSpawnPoint ();
		if (spawnPoint != Vector3.up) { //terminating condition in FindRandomSpawnPoint
			Instantiate (prefab, spawnPoint, Quaternion.identity, transform);
			count++;
		}
	}
}
