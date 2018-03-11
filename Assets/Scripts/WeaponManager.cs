using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : DataManager<WeaponData> {
	public static WeaponManager instance;

	void Awake() {
		instance = this;
		SetPointers ();
	}

	//uses loot table
	public override WeaponData GetRandomData () {
		int random = Random.Range (0, 100);
		int randomTier;

		// hardcoded tier propabilities
		if (random < 45) {
			// tier 0
			randomTier = 0;
		} else if (random < 75) {
			// tier 1
			randomTier = 1;
		} else if (random < 95) {
			// tier 2
			randomTier = 2;
		} else {
			// tier 3
			randomTier = 3;
		}

		List<WeaponData> allDataOfTier = new List<WeaponData> ();
		foreach (WeaponData weapon in datas) {
			if (weapon.tier == randomTier) {
				allDataOfTier.Add (weapon);
			}
		}

		if (allDataOfTier.Count == 0) {
			// no weapons in tier
			Debug.Log ("No weapons found in tier " + randomTier);
			return null;
		} else {
			int index = Random.Range (0, allDataOfTier.Count);
			return allDataOfTier [index];
		}
	}
}

[System.Serializable]
public class WeaponData : Data {
	public int tier;

	public override string GetAssetType () {
		return "Weapon";
	}
}