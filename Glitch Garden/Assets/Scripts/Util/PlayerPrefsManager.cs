using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerPrefsManager : MonoBehaviour {

	const string MASTER_VOLUME_KEY 		= "master_volume"	;
	const string DIFFICULTY_KEY			= "difficulty"		;
	const string LEVEL_KEY				= "level_unlocked_"	;

	public static void SetMasterVolume(float volume) {
		if (volume < 0f || volume > 1f) {
			Debug.LogError("PlayerPrefsManager: Master Volume '" + volume + "' is out of range (0 < x <= 1)");
		} else {
			PlayerPrefs.SetFloat (MASTER_VOLUME_KEY, volume);
		}
	}

	public static float GetMasterVolume() {
		return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY);
	}

	public static void SetLevelUnlock(int level) {
		if (level < 0 || level >= SceneManager.sceneCountInBuildSettings) {
			Debug.LogError ("PlayerPrefsManager: LevelUnlock requested '" + level + "' is out of range (0 <= x < " + SceneManager.sceneCountInBuildSettings + ")");
		} else {
			PlayerPrefs.SetInt(LEVEL_KEY + level, 1);
		}
	}

	public static bool IsLevelUnlocked(int level) {
		if (level < 0 || level >= SceneManager.sceneCountInBuildSettings) {
			Debug.LogError ("PlayerPrefsManager: LevelUnlock requested '" + level + "' is out of range (0 <= x < " + SceneManager.sceneCountInBuildSettings + ")");
			return false;
		}
		return PlayerPrefs.GetInt(LEVEL_KEY + level) == 1f;
	}

	public static void SetDifficulty(float diff) {
		if (diff < 1f || diff > 3f) {
			Debug.LogError("PlayerPrefsManager: Difficulty setting '" + diff + "' is out of range (1 <= x <= 3)");
		} else {
			PlayerPrefs.SetFloat (DIFFICULTY_KEY, diff);
		}
	}

	public static float GetDifficulty() {
		return PlayerPrefs.GetFloat (DIFFICULTY_KEY);
	}
}
