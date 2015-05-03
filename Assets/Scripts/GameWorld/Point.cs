using UnityEngine;
using System.Collections;

public class Point : MonoBehaviour {

	public float _timeToDie = 0.5f;

	// Use this for initialization
	void Start () {

		Invoke("Die", _timeToDie);
	}
	
	void Die() {

		Destroy(this.gameObject);
	}
}
