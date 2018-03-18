using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : DataManager<WeaponData> {
	public static WeaponManager instance;

	int[] tierValues = {0, 45, 75, 95}; //hardcoded tier probabilities

	void Awake() {
		instance = this;
		SetPointers ();
	}

	//uses loot table
	public override WeaponData GetRandomData () {
		return GetRandomDataWithMinTier (0);
	}

	public WeaponData GetRandomDataWithMinTier(int minTier) {
		int random = Random.Range (tierValues[minTier], 100);
		int randomTier = minTier;

		while((randomTier + 1) < tierValues.Length && random >= tierValues[randomTier + 1]) {
			randomTier++;
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