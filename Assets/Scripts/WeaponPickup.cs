using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {
	bool inited = false;

	public WeaponManager.WeaponData weaponData;

	void Start () {
		if (!inited) {
			weaponData = WeaponManager.instance.GetRandomData ();
			UpdateRendering ();
		}
	}

	public void Init (WeaponManager.WeaponData _weaponData) {
		weaponData = _weaponData;
		UpdateRendering ();

		inited = true;
	}

	void UpdateRendering () {
		GetComponent<MeshFilter> ().mesh = weaponData.mesh;
	}
}
