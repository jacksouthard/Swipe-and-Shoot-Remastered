using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
	public float fireRate;
	public float damage;
	public float range;
	public float accuracy;
	public GameObject bulletRayPrefab;

	bool canFire;
	float fireRateTimer = 0f;
	List<Transform> bulletSpawns = new List<Transform> ();

	void Start () {
		for (int i = 0; i < transform.childCount; i++) {
			bulletSpawns.Add (transform.GetChild(i));
		}
	}
	
	void Update () {
		if (canFire) {
			if (Input.GetKey (KeyCode.Space)) {
				Shoot ();
			}
		}

		if (!canFire) {
			fireRateTimer -= Time.deltaTime;
			if (fireRateTimer <= 0f) {
				canFire = true;
			}
		}
	}

	void Shoot () {
		fireRateTimer = fireRate;
		canFire = false;

		Vector3 start = bulletSpawns[0].position;
		Vector3 end;
		Vector3 direction = transform.rotation * Vector3.left;
		direction = Quaternion.Euler(new Vector3 (Random.Range(-accuracy, accuracy), Random.Range(-accuracy, accuracy), 0f)) * direction;

		RaycastHit hit;
		Physics.Raycast(start, direction, out hit, range);

//		if (true) {
		if (hit.collider == null) {
			// bullet misses
			end = bulletSpawns[0].position + (direction.normalized * range);

		} else {
			// bullet hit
			end = hit.point;
		}
			

		GameObject bulletRay = Instantiate (bulletRayPrefab, transform.position, Quaternion.identity, transform);
		bulletRay.GetComponent<BulletRay> ().Init (start, end, 0.05f);
	}
}
