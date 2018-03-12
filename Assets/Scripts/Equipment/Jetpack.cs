using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : Equipment {
	public float launchTime;
	public float heightIncrease;
	public int jumps;

	float fuelPerJump { get { return (100f / jumps); } }

	ParticleSystem[] particles;

	public override void Init (GameObject _go, EquipmentData _data) {
		base.Init (_go, _data);
		particles = GetComponentsInChildren<ParticleSystem> ();
		player.verticalFactor += heightIncrease;
	}

	public override void OnJump () {
		StartCoroutine (Launch());
	}

	IEnumerator Launch() {
		foreach (ParticleSystem jet in particles) {
			jet.Play ();
		}

		yield return new WaitForSeconds (launchTime);

		foreach (ParticleSystem jet in particles) {
			jet.Stop ();
		}

		TakeDamage (fuelPerJump);
	}

	public override void Remove() {
		player.verticalFactor -= heightIncrease;
		base.Remove ();
	}
}
