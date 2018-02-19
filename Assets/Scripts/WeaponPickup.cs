using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {
	public string customType = "None";

	bool inited = false;

	public WeaponManager.WeaponData weaponData;

	void Start () {
		if (customType != "None") {
			weaponData = WeaponManager.instance.WeaponDataFromName(customType);
			UpdateRendering ();
		} else if (!inited) {
			weaponData = WeaponManager.instance.GetWeaponDataFromLootTable ();
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
