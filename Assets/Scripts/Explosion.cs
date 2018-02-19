using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
	public AnimationCurve forceCurve;

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
