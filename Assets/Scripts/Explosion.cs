using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
	static GameObject prefab;
	public AnimationCurve forceCurve;

	public static Explosion Create(Vector3 position) {
		if (prefab == null) {
			prefab = Resources.Load ("Explosion") as GameObject;
		}

		GameObject explosion = (GameObject) Instantiate (prefab, position, Quaternion.identity);
		return explosion.GetComponent<Explosion> ();
	}

	public void Initiate(float range, float force) {
		List<Rigidbody> hitRbs = new List<Rigidbody> ();

		Collider[] hitColliders = Physics.OverlapSphere (transform.position, range);
		foreach (Collider col in hitColliders) {
			Rigidbody rb = col.gameObject.GetComponentInParent<Rigidbody> ();
			if (rb != null && !hitRbs.Contains (rb)) {
				float dist = Vector3.Distance (rb.transform.position, transform.position);
				Vector3 dir = (rb.transform.position - transform.position).normalized;
				rb.AddForce(dir * force * forceCurve.Evaluate (dist / range)); //apply force to all rigidbodies in range
				//alternate formula: (-Mathf.Pow(dist / range, n) + 1) * force
				//where n is the falloff range

				hitRbs.Add (rb);
			}
		}
	}
}
