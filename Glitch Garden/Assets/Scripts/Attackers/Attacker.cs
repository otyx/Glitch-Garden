using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody2D))]

public class Attacker : MonoBehaviour {

	private float currentSpeed;
	private GameObject currentTarget;
	
	private Animator animator;
	
	// Use this for initialization
	void Start () {
		if (gameObject.GetComponent<Rigidbody2D>() == null) {
			Rigidbody2D myRigidBody = gameObject.AddComponent<Rigidbody2D> ();
			myRigidBody.isKinematic = true;
		}
		animator = GetComponent<Animator>();
		if (animator == null) {
			Debug.Log("animator component is null in Attack");
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (Vector3.left * currentSpeed * Time.deltaTime);
	}

	void OnTriggerEnter2D() {
		Debug.Log (name + " trigger entered");
	}
	
	public void SetSpeed(float speed) {
		currentSpeed = speed;
	}
	
	public void Attack(GameObject obj) {
		currentTarget = obj;
		animator.SetBool(Constants.BOOL_IS_ATTACKING, true);
	}
	
	// called from Animator by animation event
	public void StrikeCurrentTarget(float damage) {
		Debug.Log(name + " is striking for " + damage + " damage!");
	}
}
