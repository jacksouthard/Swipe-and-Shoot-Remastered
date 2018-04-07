using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
	public string customType = "None";
	public string assetType;

	public bool isObjective = false;
	public bool destoryLastPickup;

	// despawning
	bool despawn;
	float despawnTimer = 20f;

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

	public void Init (Data _data, bool _despawn) {
		data = _data;
		assetType = data.GetAssetType ();

		UpdateRendering ();

		inited = true;
		despawn = _despawn;
	}

	void UpdateRendering () {
		GetComponent<MeshFilter> ().mesh = data.mesh;
	}

	void Update () {
		if (despawn && !CameraController.PositionIsInView(transform.position)) {
			despawnTimer -= Time.deltaTime;
			if (despawnTimer <= 0f) {
				Destroy (gameObject);
			}
		}
	}
}
