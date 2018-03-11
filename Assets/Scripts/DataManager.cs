using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//base manager class
public class DataManager<T> : MonoBehaviour {
	public List<T> datas = new List<T>();
	protected Dictionary<string, int> dataPointers = new Dictionary<string, int>(); //points to an index in a given list

	public static Dictionary<string, Data> allData = new Dictionary<string, Data> (); //generic list of all data

	//initializes data pointer list (called on Awake)
	protected void SetPointers() {
		for(int i = 0; i < datas.Count; i++) {
			Data newData = (Data)(object)datas[i];
			dataPointers.Add (newData.name, i);

			if (!DataManager<Data>.allData.ContainsKey (newData.name)) {
				DataManager<Data>.allData.Add (newData.name, newData);
			}
		}
	}

	//gets data of specified type with a given name
	public T GetDataFromName(string _name) {
		if (_name == "Random") {
			return GetRandomData ();
		}

		return datas [dataPointers [_name]];
	}

	//returns random data of specified type
	public virtual T GetRandomData() {
		return datas [Random.Range (0, datas.Count)]; //default behavior
	}

	//returns generic data with a given name
	public static Data GetAnyDataFromName(string _name) {
		if (_name == "Random") {
			return GetAnyRandomData ();
		}

		return allData [_name];
	}

	//returns random generic data with a specified type
	public static Data GetAnyRandomData (string dataType) {
		switch (dataType) {
			case "Weapon":
				return WeaponManager.instance.GetRandomData ().ToAssetData();
			case "Equipment":
				return EquipmentManager.instance.GetRandomData ().ToAssetData();
			default:
				return null;
		}
	}

	//returns random generic data
	public static Data GetAnyRandomData() {
		int random = Random.Range (0, 2);
		if (random == 0) {
			return GetAnyRandomData ("Weapon");
		} else if (random == 1) {
			return GetAnyRandomData ("Equipment");
		}

		return null;
	}
}

[System.Serializable]
public abstract class Data {
	public string name;
	public GameObject prefab;
	public Mesh mesh;

	public Data ToAssetData() {
		return (this as Data);
	}

	public abstract string GetAssetType();
}
