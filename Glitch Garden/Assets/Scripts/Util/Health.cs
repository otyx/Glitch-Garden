using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

	// the health for this entity
	public float health;
	
	public void TakeHit(float dmg) {
		health -= dmg;
	}
	
	public bool IsDestroyed() {
		return health <= 0;
	}
	
	public void AddHealth(float bonus) {
		health += bonus;
	}
	
	public float GetHealth() {
		return health;
	}
	
	public void Die() {
		//Debug.Log(gameObject.name + " is dying!");
		
		// if this is an attacker and it dies, we need to stop the attack mode
		// on the current Target...
		Attacker attacker = gameObject.GetComponent<Attacker>();
		if (attacker != null) {
			attacker.StopAttack();
		}
		// Now destroy the game object
		Destroy(gameObject);
	}
}
