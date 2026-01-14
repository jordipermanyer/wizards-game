using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinSceneControllerScript : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text winText;        // Main win message

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Header("Win Messages")]
    [TextArea(2, 6)]
    [SerializeField] private string winMessage = "You won!";

    private void Awake()
    {
        if (mainMenuButton == null || quitButton == null || winText == null)
        {
            Debug.LogError("WinSceneControllerScript: missing UI references in Inspector.");
            return;
        }

        mainMenuButton.onClick.AddListener(GoToMainMenu);
        quitButton.onClick.AddListener(QuitGame);

        SetupWinTexts();
    }

    private void SetupWinTexts()
    {
        winText.text = winMessage;

        bool completedAllLevels =
            PlayerPrefs.GetInt("Level1Completed", 0) == 1 &&
            PlayerPrefs.GetInt("Level2Completed", 0) == 1 &&
            PlayerPrefs.GetInt("Level3Completed", 0) == 1 &&
            PlayerPrefs.GetInt("Level4Completed", 0) == 1;
    }

    private void GoToMainMenu()
    {
        Debug.Log("Returning to Main Menu...");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void QuitGame()
    {
        Debug.Log("Exiting game...");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Application.Quit();
    }
}
