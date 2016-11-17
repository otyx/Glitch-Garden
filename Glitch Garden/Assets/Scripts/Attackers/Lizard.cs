using UnityEngine;
using System.Collections;

public class Lizard : MonoBehaviour {

	private Attacker attacker;
	private Animator animator;
	
	// Use this for initialization
	void Start () {
		attacker = GetComponent<Attacker>();
		if (attacker == null) {
			Debug.Log("Attacker component is null in Lizard");
		}
		animator = GetComponent<Animator>();
		if (animator == null) {
			Debug.Log("animator component is null in Lizard");
		}
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	// handle the trigger entry
	public void OnTriggerEnter2D(Collider2D other) {
		if (!other.GetComponent<Defender>()) {
			// not a defender so ignore for now
			return;
		} else {
			// it's a non-gravestone defender!
			attacker.Attack(other.gameObject);
		}
	}
}
