using UnityEngine;
using System.Collections;

public class Shredder : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D obj) {
		Destroy (obj.gameObject);
	}
}
