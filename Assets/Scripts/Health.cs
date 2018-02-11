using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Health : MonoBehaviour {
	public enum State
	{
		Alive,
		Dying,
		Decaying
	};
	public State state = State.Alive;

	[Header("Stats")]
	public float maxHealth;

	[Header("Regen")]
	public float regenSpeed;
	public float regenWait;

	[Header("Hit")]
	public float hitWait;
	public Color hitColor;

	[Header("Death")]
	public float dyingTimer;
	public float decayTimer;

	[Header("Debug")]
	public float waitTimer;
	public float health;

	MeshRenderer[] mrs;
	List<Color[]> originalColors;

	void Start () {
		health = maxHealth;
		UpdateRenderers ();
	}

	public void UpdateRenderers() {
		mrs = GetComponentsInChildren<MeshRenderer>();
		originalColors = new List<Color[]> ();
		for(int i = 0; i < mrs.Length; i++) {
			Color[] colors = new Color[mrs[i].materials.Length];
			for (int j = 0; j < colors.Length; j++) {
				colors [j] = mrs [i].materials [j].color;
			}
			originalColors.Add(colors);
		}
	}
	
	void Update () {
		if (state == State.Alive) {
			if (health < maxHealth) {
				if (waitTimer <= 0f) {
					// start regening
					health += regenSpeed * Time.deltaTime;
					if (health > maxHealth) {
						health = maxHealth;
					}
				} else {
					// incriment timer
					waitTimer -= Time.deltaTime;
				}
			}
		}

		if (state == State.Dying) {
			dyingTimer -= Time.deltaTime;
			if (dyingTimer <= 0f) {
				Decay ();
			}
		}

		if (state == State.Decaying) {
			decayTimer -= Time.deltaTime;
			if (decayTimer <= 0f) {
				Destroy (gameObject);
			}
		}
	}

	public void TakeDamage (float damage) {
		if (state == State.Alive) {
			health -= damage;
			if (health <= 0f) {
				Die ();
			} else {
				StartCoroutine (HitAnimation ());
			}
		}

		waitTimer = regenWait;
	}

	public void Die () {
		if (GetComponent<PlayerController> () != null) {
			// handel player death
			print ("Player Death");
			return;
		}

		state = State.Dying;

		// if enemy
		if (GetComponent<EnemyController>() != null) {
			gameObject.GetComponent<Rigidbody> ().isKinematic = false;

			Destroy(GetComponent<NavMeshAgent> ());
			Destroy(GetComponent<EnemyController> ());
			Destroy(GetComponentInChildren<ShootingController> ());
			Destroy (GetComponentInChildren<Weapon> ());
		}
	}

	void Decay () {
		state = State.Decaying;

		// disable colliders
		if (GetComponent<Collider> () != null) {
			GetComponent<Collider> ().enabled = false;
		}

		// children colliders
		Collider[] childColls = GetComponentsInChildren<Collider>();
		foreach (var coll in childColls) {
			coll.enabled = false;
		}

		// set color to black
		ChangeToColor(Color.black);
	}

	IEnumerator HitAnimation() {
		ChangeToColor (hitColor);
		yield return new WaitForSeconds (hitWait);
		if (state == State.Alive) {
			ResetColor ();
		}
	}

	void ChangeToColor(Color color) {
		foreach (var mr in mrs) {
			foreach (var mat in mr.materials) {
				mat.color = color;
			}
		}
	}

	void ResetColor() {
		for (int i = 0; i < mrs.Length; i++) {
			int colorCount = originalColors [i].Length;
			for (int j = 0; j < colorCount; j++) {
				mrs [i].materials [j].color = originalColors [i] [j];
			}
		}
	}
}
