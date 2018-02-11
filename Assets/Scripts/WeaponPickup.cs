using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {
	public WeaponManager.WeaponData weaponData;

	void Start () {
		weaponData = WeaponManager.instance.GetRandomData ();
		UpdateRendering ();
	}

	void UpdateRendering () {
		GetComponent<MeshFilter> ().mesh = weaponData.mesh;
	}
}
