using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour {
	public static WeaponManager instance;

	public WeaponData[] weaponDatas;

	void Awake () {
		instance = this;
	}

	public WeaponData GetRandomData () {
		int index = Random.Range (0, weaponDatas.Length);
		return weaponDatas [index];
	}

	public WeaponData GetWeaponDataFromLootTable () {
		int random = Random.Range (0, 100);
		int randomTier;

		// hardcoded tier propabilities
		if (random < 50) {
			// tier 0
			randomTier = 0;
		} else if (random < 80) {
			// tier 1
			randomTier = 1;
		} else if (random < 95) {
			// tier 2
			randomTier = 2;
		} else {
			// tier 3
			randomTier = 3;
		}
		return GetRandomDataOfTier (randomTier);
	}

	public WeaponData WeaponDataFromName (string _name) {
		foreach (var weaponData in weaponDatas) {
			if (weaponData.name == _name) {
				return weaponData;
			}
		}
		print ("Could not find weapon with name: " + _name);
		return weaponDatas [0];
	}

	WeaponData GetRandomDataOfTier (int tier) {
		List<WeaponData> allDataOfTier = new List<WeaponData> ();
		foreach (var weapon in weaponDatas) {
			if (weapon.tier == tier) {
				allDataOfTier.Add (weapon);
			}
		}
		if (allDataOfTier.Count == 0) {
			// no weapons in tier
			print ("No weapons found in tier " + tier);
			return weaponDatas [0];
		} else {
			int index = Random.Range (0, allDataOfTier.Count);
			return allDataOfTier [index];
		}
	}

	[System.Serializable]
	public class WeaponData {
		public int tier;
		public string name;
		public GameObject prefab;
		public Mesh mesh;
	}
}
