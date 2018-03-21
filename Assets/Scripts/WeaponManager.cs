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

	public Weapon weapon { get { return prefab.GetComponent<Weapon> (); } }

	public override string GetAssetType () {
		return "Weapon";
	}

	public WeaponComparisonData Compare(WeaponData other) {
		WeaponComparisonData newData = new WeaponComparisonData ();
		Weapon a = weapon;
		Weapon b = other.weapon;

		newData.dpsPercentageDiff = b.dps / a.dps;
		newData.rangePercentageDiff = b.range / a.range;

		return newData;
	}
}

public class WeaponComparisonData {
	public float dpsPercentageDiff;
	public float rangePercentageDiff;

	public static Color GetColorFromPercentage(float percentage) {
		if (percentage > 1f) {
			return new Color (0f, 0.75f, 0f);
		} else if (percentage < 1f) {
			return new Color (1f, 0.25f, 0.25f);
		} else {
			return Color.white;
		}
	}
}