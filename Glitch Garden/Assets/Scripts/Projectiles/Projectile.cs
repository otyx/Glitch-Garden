using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public float speed;
	public float damage;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		GetComponent<Rigidbody2D>().transform.Translate(Vector3.right * speed * Time.deltaTime);
	}
	 
	// detect a collision with an attacker and deal damage.
	void OnTriggerEnter2D(Collider2D other) {
		//Debug.Log("Hit something: " + other.gameObject.tag);
		if (other.gameObject.tag.Equals(Constants.ATTACKER)){
			Health hlth = other.GetComponent<Health>();
			hlth.TakeHit(damage);
			if (hlth.IsDestroyed()) {
				hlth.Die();
			}
			Destroy(gameObject);
		}
	}

}
