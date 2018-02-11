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

	[System.Serializable]
	public class WeaponData {
		public string name;
		public GameObject prefab;
		public Mesh mesh;
	}
}
