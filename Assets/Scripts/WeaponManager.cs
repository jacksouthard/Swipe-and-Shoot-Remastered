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

	public WeaponData WeaponDataFromName (string _name) {
		foreach (var weaponData in weaponDatas) {
			if (weaponData.name == _name) {
				return weaponData;
			}
		}
		print ("Could not find weapon with name: " + _name);
		return weaponDatas [0];
	}

	[System.Serializable]
	public class WeaponData {
		public string name;
		public GameObject prefab;
		public Mesh mesh;
	}
}
