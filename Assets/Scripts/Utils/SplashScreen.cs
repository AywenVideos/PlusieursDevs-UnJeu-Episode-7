using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private float waitTime = 1f;

    private void Start()
    {
        // Check if this is the first time loading the splash screen
        bool isFirstTime = PlayerPrefs.GetInt("HasSeenSplash", 0) == 0;
        
        if (isFirstTime)
        {
            // First time: show splash and load keybindings
            PlayerPrefs.SetInt("HasSeenSplash", 1);
            PlayerPrefs.Save();
            StartCoroutine(LoadKeyBindingsAfterDelay());
        }
        else
        {
            // Not first time: load game directly
            StartCoroutine(LoadGameAfterDelay());
        }
    }

    private IEnumerator LoadKeyBindingsAfterDelay()
    {
        Debug.Log("[SPLASH SCREEN] First time loading, going to keybindings.");
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("Scenes/02_KeyBindingsScene");
    }

    private IEnumerator LoadGameAfterDelay()
    {
        Debug.Log("[SPLASH SCREEN] Not first time, loading game directly.");
        yield return new WaitForSeconds(waitTime);
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Scenes/01_Game");
    }
}
