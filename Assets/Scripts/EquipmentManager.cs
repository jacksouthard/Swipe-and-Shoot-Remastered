using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : DataManager<EquipmentData> {
	public static EquipmentManager instance;

	void Awake() {
		instance = this;
		SetPointers ();
	}
}

[System.Serializable]
public class EquipmentData : Data {
	public enum Type {
		Back = 0,
		Chest = 1,
		Head = 2
	}

	public Type type;

	public override string GetAssetType () {
		return "Equipment";
	}
}
