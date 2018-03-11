using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
	public string customType = "None";
	public string assetType;

	bool inited = false;

	public Data data;

	void Start () {
		if (customType != "None") {
			data = DataManager<Data>.GetAnyDataFromName(customType);
			UpdateRendering ();
		} else if (!inited) {
			data = DataManager<Data>.GetAnyRandomData(assetType);
			UpdateRendering ();
		}
	}

	public void Init (Data _data) {
		data = _data;
		assetType = data.GetAssetType ();

		UpdateRendering ();

		inited = true;
	}

	void UpdateRendering () {
		GetComponent<MeshFilter> ().mesh = data.mesh;
	}
}
