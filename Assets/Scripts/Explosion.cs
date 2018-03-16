using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
	static GameObject prefab;
	public AnimationCurve forceCurve;

	public static void Create(Vector3 position, float range, float force, float damage) {
		if (prefab == null) {
			prefab = Resources.Load ("Explosion") as GameObject;
		}

		GameObject explosion = (GameObject) Instantiate (prefab, position, Quaternion.identity);
		Explosion script = explosion.GetComponent<Explosion> ();
		script.StartCoroutine (script.Initiate(range, force, damage));
		GameObject.Destroy (explosion, 2f);
	}

	public IEnumerator Initiate(float range, float force, float damage) {
		yield return new WaitForEndOfFrame();

		List<Rigidbody> hitRbs = new List<Rigidbody> ();

		Collider[] hitColliders = Physics.OverlapSphere (transform.position, range);
		foreach (Collider col in hitColliders) {
			Rigidbody rb = col.gameObject.GetComponentInParent<Rigidbody> ();
			if (rb != null && !hitRbs.Contains (rb) && rb.gameObject.layer != 11) {
				float dist = Vector3.Distance (rb.transform.position, transform.position);
				float distFactor = forceCurve.Evaluate (dist / range);

				Health health = col.gameObject.GetComponentInParent<Health> ();
				if (health != null) {
					health.TakeDamage (damage * distFactor, Health.DamageType.Explosions);
				}

				Vector3 dir = (rb.transform.position - transform.position).normalized;
				rb.AddForce(dir * force * distFactor); //apply force to all rigidbodies in range
				//alternate formula: (-Mathf.Pow(dist / range, n) + 1) * force
				//where n is the falloff range

				hitRbs.Add (rb);
			}
		}
	}
}
