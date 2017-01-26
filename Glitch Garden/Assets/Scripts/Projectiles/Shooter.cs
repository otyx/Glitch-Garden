using UnityEngine;
using System.Collections;

public class Shooter : MonoBehaviour {

	public GameObject projectile;
	
	public float shotsPerSecond;
	
	private static GameObject PROJECTILES;
	
	private GameObject launcher;
	public float timer, timeInterval;
	
	private Animator animator;
	private AttackerSpawner thisLaneSpawner;
	
	public void Start() {
		// initialise the Projectiles parent if necessary.
		if (PROJECTILES == null) {
			PROJECTILES = new GameObject(Constants.OBJ_PROJECTILES);
			PROJECTILES.transform.position = Vector3.zero;
			
		}
		// get the animator
		animator = gameObject.GetComponent<Animator>();
		
		// get the lane spawner
		SetThisLaneSpawner();
		
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
		
		if (IsEnemyAheadInLane()) {
			animator.SetBool(Constants.BOOL_IS_ATTACKING, true);
		} else {
			animator.SetBool(Constants.BOOL_IS_ATTACKING, false);
		}
	}
	
	void SetThisLaneSpawner() {
		AttackerSpawner[] spawners = GameObject.FindObjectsOfType<AttackerSpawner>();
		foreach(AttackerSpawner spawner in spawners) {
			if (spawner.transform.position.y == transform.position.y) {
				thisLaneSpawner = spawner;
				return;
			}
		}
		Debug.LogError(name + ": Spawner not found for Lane: " + transform.position.y);
	}
	
	bool IsEnemyAheadInLane() {
		bool enemyAhead = false;
		
		// check only if attackers in lane.
		if(thisLaneSpawner.transform.childCount > 0) {
			foreach (Transform attacker in thisLaneSpawner.transform) {
				if (attacker.transform.position.x >= transform.position.x) {
					enemyAhead = true;
					break;
				} 
			}
		}
		return enemyAhead;
	}
	
	private void Launch() {
		if (launcher == null) {
			Debug.LogError("Shooter: Can't Launch with a null Launcher! (Object: " + gameObject.name + ")");
		}
		GameObject newProjectile = Instantiate(projectile, launcher.transform.position, Quaternion.identity) as GameObject;
		newProjectile.transform.SetParent(PROJECTILES.transform);
		newProjectile.name = projectile.name;
	}
}
