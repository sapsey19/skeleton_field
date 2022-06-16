using UnityEngine.SceneManagement;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    public AudioClip mainTheme;
    public AudioClip menuTheme;

    string sceneName;

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        string newSceneName = SceneManager.GetActiveScene().name;
        if (newSceneName != sceneName) {
            sceneName = newSceneName;
            Invoke("PlayMusic", .2f);
        }
    }

    void PlayMusic() {
        AudioClip clipToPlay = null;

        if (sceneName == "Menu")
            clipToPlay = menuTheme;
        else if (sceneName == "SampleScene")
            clipToPlay = mainTheme;

        if(clipToPlay != null) {
            AudioManager.instance.PlayMusic(clipToPlay, 2);
            Invoke("PlayMusic", clipToPlay.length);
        }
    }
}
