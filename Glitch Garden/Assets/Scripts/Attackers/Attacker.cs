using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody2D))]

public class Attacker : MonoBehaviour {
	[Tooltip ("The average number of seconds between spawns of this type")]
	public float secondsBetweenSpawns;
	
	private float currentSpeed;
	private GameObject currentTarget;
	private Animator animator;
	private Health health;
	
	// Use this for initialization
	void Start () {
		if (gameObject.GetComponent<Rigidbody2D>() == null) {
			Rigidbody2D myRigidBody = gameObject.AddComponent<Rigidbody2D> ();
			myRigidBody.isKinematic = true;
		}
		
		animator = GetComponent<Animator>();
		if (animator == null) {
			Debug.Log("animator component is null in Attack for " + gameObject.name);
		}
		
		health = GetComponent<Health>();
		if (health == null) {
			Debug.Log("Health Component is missing in " + gameObject.name);
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.GetComponent<Rigidbody2D>().transform.position += (Vector3.left * currentSpeed * Time.deltaTime);
	}

	void OnTriggerEnter2D() {
		//Debug.Log (name + " trigger entered");
	}
	
	public void SetSpeed(float speed) {
		currentSpeed = speed;
	}
	
	public GameObject getCurrentTarget() {
		return currentTarget;	
	}
	
	public void Attack(GameObject obj) {
		currentTarget = obj;
		animator.SetBool(Constants.BOOL_IS_ATTACKING, true);
		Animator anim = obj.GetComponent<Animator>();
		if (anim) {
			anim.SetBool(Constants.BOOL_IS_ATTACKED, true);
		}
	}
	
	// if we need to stop attacking, for some reason, trigger that here
	public void StopAttack() {
		if (currentTarget) {
			Animator anim = currentTarget.GetComponent<Animator>();
			if (anim) {
				anim.SetBool(Constants.BOOL_IS_ATTACKED, false);
			}
		}
	}
	
	// called from Animator by animation event
	public void StrikeCurrentTarget(float damage) {
		if (currentTarget) {
			Health h = currentTarget.GetComponent<Health>();
			if (h) {
				h.TakeHit(damage);
				//Debug.Log(name	+ " is striking for " + damage + " damage! "
				//	+ currentTarget.gameObject.name + " now has health: " 
				//	+ h.GetHealth() + " (is dead? " + h.IsDestroyed() + ")");
				if (h.IsDestroyed()) {
					h.Die();
					animator.SetBool(Constants.BOOL_IS_ATTACKING, false);
				}
			} // end if h
		} // end if currenttarget
	}
}
