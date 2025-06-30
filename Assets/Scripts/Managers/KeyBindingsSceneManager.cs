using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class KeyBindingsSceneManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyBindingsText;
    [SerializeField] private float waitTime = 10f; // Time to show keybindings before auto-loading game

    private void Start()
    {
        // Ensure keyBindingsText field is assigned in Unity Inspector
        if (keyBindingsText == null)
        {
            Debug.LogError("KeyBindingsSceneManager: KeyBindingsText field not assigned in Inspector!");
            return;
        }

        // Set the keybindings text
        keyBindingsText.text = "Chargement du jeu dans " + waitTime + " secondes...";

        // Start loading the game after delay
        StartCoroutine(LoadGameAfterDelay());
    }

    private IEnumerator LoadGameAfterDelay()
    {
        Debug.Log("[KEY BINDING SCENE] Waiting " + waitTime + " seconds before loading game.");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("[KEY BINDING SCENE] Loading game scene.");
        SceneManager.LoadScene("Scenes/01_Game");
    }
}
