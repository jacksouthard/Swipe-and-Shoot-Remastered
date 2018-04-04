using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingTerrain : MonoBehaviour {
	public List<GameObject> terrainPrefabs;
	public float spawnFrequency;
	public float scrollSpeed;

	[Header("Options")]
	public bool useRandomZ;
	public bool useRandomRot;
	public Transform childContainer;

	float timer;
	List<GameObject> scrollingObjects = new List<GameObject>();

	BoxCollider zone;

	// all local
	float spawnX;
	float despawnX;
	float zSpread;

	void Start () {
		if (childContainer == null) {
			childContainer = this.transform;
		}

		zone = GetComponent<BoxCollider> ();
		InterpretZone ();
		Prewarm ();
		timer = spawnFrequency;
		StartCoroutine (Scroll());
	}

	void SpawnObject (float x) { // hack
		int random = Random.Range (0, terrainPrefabs.Count - 1);
		GameObject prefab = terrainPrefabs [random];
		float z = 0f;
		if (useRandomZ) {
			z = Random.Range (-zSpread, zSpread);
		}
		Vector3 spawnPos = new Vector3 (x, 0f, z);
		Quaternion newRot;
		if (useRandomRot) {
			newRot = Quaternion.Euler(new Vector3 (0f, Random.Range (0f, 360f), 0f));
		} else {
			newRot = Quaternion.identity;
		}
		GameObject newObject = (GameObject)Instantiate (prefab, transform.TransformPoint(spawnPos), newRot, childContainer);
		scrollingObjects.Add (newObject);
	}

	void MoveObjects () {
		List<GameObject> objectsToRemove = new List<GameObject> ();
		foreach (var obj in scrollingObjects) {
			Vector3 newPos = obj.transform.position + (Vector3.left * scrollSpeed * Time.fixedDeltaTime);
			if (newPos.x < despawnX) {
				objectsToRemove.Add (obj);
			} else {
				obj.transform.position = newPos;
			}
		}

		foreach (var obj in objectsToRemove) {
			scrollingObjects.Remove (obj);
			Destroy (obj);
		}
	}

	void Prewarm () {
		float curX = despawnX;
		float xStep = spawnFrequency * scrollSpeed;
		while (curX < spawnX) {
			curX += xStep;
			SpawnObject (x: curX);
		}
	}

	void InterpretZone () {
		spawnX = zone.size.x / 2f;
		despawnX = -spawnX;
		zSpread = zone.size.z / 2f;
	}

	IEnumerator Scroll() {
		while (true) {
			timer -= Time.fixedDeltaTime;
			if (timer < 0f) {
				SpawnObject (spawnX);
				timer = spawnFrequency;
			}

			yield return new WaitForEndOfFrame ();

			MoveObjects ();

			yield return new WaitForSecondsRealtime (Time.fixedDeltaTime);
		}
	}
}
