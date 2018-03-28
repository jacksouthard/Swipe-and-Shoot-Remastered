using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialSpawn : MonoBehaviour{
	public virtual void Init () {
		Destroy (gameObject);
	}
}
