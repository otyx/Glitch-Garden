using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Attacker))]
public class Fox : MonoBehaviour {

	private Attacker attacker;
	private Animator animator;
	
	// Use this for initialization
	void Start () {
		attacker = GetComponent<Attacker>();
		if (attacker == null) {
			Debug.Log("Attacker component is null in Fox");
		}
		animator = GetComponent<Animator>();
		if (animator == null) {
			Debug.Log("animator component is null in Fox");
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
		}
		
		if (other.GetComponent<Gravestone>()) {
			animator.SetTrigger(Constants.FOX_JUMP_TRIGGER);
		} else {
			// it's a non-gravestone defender!
			attacker.Attack(other.gameObject);
		}
	}
}
