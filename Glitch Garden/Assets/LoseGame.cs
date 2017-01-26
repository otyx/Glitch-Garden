using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseGame : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag.Equals(Constants.ATTACKER)) {
			// game is over!
			SceneManager.LoadScene(Constants.SCN_LOSE);
		}
	}
}
