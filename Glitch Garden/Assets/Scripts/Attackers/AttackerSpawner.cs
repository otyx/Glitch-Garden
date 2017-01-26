using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerSpawner : MonoBehaviour {
	
	public GameObject[] Attackers;
	
	// Update is called once per frame
	void Update () {
		foreach (GameObject attackerToSpawn in Attackers) {
			if (IsTimeToSpawn(attackerToSpawn)) {
				Spawn(attackerToSpawn);
			}	
		}
	}
	
	bool IsTimeToSpawn(GameObject attackerToSpawn) {
		bool doSpawn = false;
		Attacker attacker = attackerToSpawn.GetComponent<Attacker>();
		
		
		float meanTimeBetweenSpawns = attacker.secondsBetweenSpawns;
		
		// chance of spawn occurring in a second.
		float spawnsPerSecond = 1 / meanTimeBetweenSpawns;
		
		// adjust per second chance for the faction of a second between frames and divide by
		// 5 since there are 5 lanes.
		// so this is the chance that this lane will spawn an attacker this frame.
		float threshold = spawnsPerSecond * Time.deltaTime / 5;
		
		// frame rate cap
		if (Time.deltaTime > meanTimeBetweenSpawns  ) {
			Debug.Log("Spawn rate capped by Frame Rate!");
		}
		
		if (Random.value < threshold) {
			doSpawn = true;
		}
		return doSpawn;
	}
	
	void Spawn (GameObject prefab) {
		GameObject obj = GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
		obj.transform.parent = transform;
		obj.name = prefab.name;
		print ("Spawning " + obj.name);
	}
}
