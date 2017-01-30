using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour {

	
	// the Music library
	[Tooltip ("Add the music clips in order of build index starting with the splash screen")]
	public AudioClip[] musicLibrary;
	
	// the Sound Effect library
	[Tooltip ("Add the clips in order of WIN, LOSE")]
	public AudioClip[] effectLibrary;
	
	// the music Audio source
	private AudioSource musicSource;
	private AudioSource effectsSource;

	void Awake() {
		DontDestroyOnLoad (gameObject);
	}

	// Use this for initialization
	void Start () {
		AudioSource[] srces = gameObject.GetComponentsInChildren<AudioSource> ();
		foreach (AudioSource src in srces) {
			if (src.name.Equals(Constants.MUSIC_AUDIOSRC_NAME)) {
				musicSource = src;
			} else if (src.name.Equals(Constants.EFFECTS_AUDIOSRC_NAME)) {
				effectsSource = src;
			}
		}
		
		// Register the music player as a delegate for the scenemanager
		SceneManager.sceneLoaded += OnSceneLoad;

		// play the splash screen sound - with the defalt volume
		PlayMusicClip (musicLibrary[0], false);
	}
	
	// Scene Management delegate code
	void OnSceneLoad(Scene scene, LoadSceneMode mode) {
		PlayMusicClip (musicLibrary[scene.buildIndex], true, PlayerPrefsManager.GetMasterVolume());
	}
		
	public void PlayMusicClip(AudioClip clip, bool looping = false, float volume = 0.25f) {
		if (clip) {
			if (clip != musicSource.clip) {
				PlayClip(clip, musicSource, looping, volume);
			}
		} else {
			Debug.LogError ("MP: PlayMusicClip called with null clip. Current scene: " + SceneManager.GetActiveScene().name);
		}
	}
	
	public void PlayEffectClip(AudioClip clip, bool looping = false, float volume = 0.5f) {
		Debug.Log("Player: playing clip: " + clip.name);
		if (clip) {
			PlayClip(clip, effectsSource, looping, volume);
		} else {
			Debug.LogError ("MP: PlayEffectClip called with null clip. Current scene: " + SceneManager.GetActiveScene().name);
		}
	}
	
	private void PlayClip(AudioClip clip, AudioSource src, bool looping = false, float volume = 0.25f) {
		src.clip = clip;
		src.loop = looping;
		src.volume = volume;
		src.PlayOneShot(clip); 
	}
	
	// Volumecontrol
	public void SetVolume(string src, float volume) {
		if (src.Equals(Constants.MUSIC_AUDIOSRC_NAME)) {
			musicSource.volume = volume;
		} else if (src.Equals(Constants.EFFECTS_AUDIOSRC_NAME)) {
			effectsSource.volume = volume;
		}
	}
}
