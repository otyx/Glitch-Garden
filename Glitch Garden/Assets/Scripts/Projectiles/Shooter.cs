using UnityEngine;
using System.Collections;

public class Shooter : MonoBehaviour {

	public GameObject projectile;
	
	public float shotsPerSecond;
	
	private static GameObject PROJECTILES;
	
	private GameObject launcher;
	public float timer, timeInterval;
	
	public void Start() {
		// initialise the Projectiles parent if necessary.
		if (PROJECTILES == null) {
			PROJECTILES = GameObject.Find(Constants.OBJ_PROJECTILES);
		}
		
		// find the launcher object
		Transform launcherTransform = gameObject.transform.FindChild(Constants.OBJ_LAUNCHER);
		launcher = launcherTransform.gameObject;
		if (launcher == null) {
			Debug.LogError("Shooter: Launcher gameObject is null!");
		}

		// the timers
		timeInterval = 1/shotsPerSecond;
	}

	public void Update() {
		timer = timer + Time.deltaTime;
	}

	private void Launch() {
//		Commented out to make conformant with solution in course material.
//		 if (timer < timeInterval) {
//			// still in cooldown
//			return;
//		} else {
//			// restart counting
//			timer = 0;
//		}
		if (launcher == null) {
			Debug.LogError("Shooter: Can't Launch with a null Launcher! (Object: " + gameObject.name + ")");
		}
		GameObject newProjectile = Instantiate(projectile, launcher.transform.position, Quaternion.identity) as GameObject;
		newProjectile.transform.SetParent(PROJECTILES.transform);
		newProjectile.name = projectile.name;
	}
}
