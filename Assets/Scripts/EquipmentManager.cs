using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour {
	public static EquipmentManager instance;

	public EquipmentData[] equipmentDatas;

	void Awake () {
		instance = this;
	}

	public EquipmentData EquipmentDataFromName (string _name) {
		foreach (var equipmentData in equipmentDatas) {
			if (equipmentData.name == _name) {
				return equipmentData;
			}
		}
		print ("Could not find equipment with name: " + _name);
		return equipmentDatas [0];
	}
}

[System.Serializable]
public class EquipmentData {
	public enum Type {
		Back = 0,
		Chest = 1,
		Head = 2
	}

	public string name;
	public Type type;
	public GameObject prefab;
	public Mesh mesh;
}
