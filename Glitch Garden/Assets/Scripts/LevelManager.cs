using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

	public float autoLoadLevelInterval;

	// No need to keep between levels since only using the prefab for level changes.
	// Also, by default, autoloadlevel is 0 so we don't get automatic level loading
	// Therefore, to move from splash to start,change the value to 3 seconds (for example)
	//
	//void Awake() {
	//	DontDestroyOnLoad (gameObject);
	//}

	// Use this for initialization
	void Start () {
		if (autoLoadLevelInterval == 0) {
			Debug.Log ("LM: Level Auto Load - disabled");
		} else {
			Invoke ("LoadStartScene", autoLoadLevelInterval);
		}
	}

	public void LoadStartScene() {
		LoadScene (Constants.LVL_STARTMENU);
	}

	public void LoadScene(string sceneName) {
		SceneManager.LoadScene (sceneName);
	}

	public void Quit() {
		Debug.Log ("LM: Quit called");
		Application.Quit ();
	}
}
