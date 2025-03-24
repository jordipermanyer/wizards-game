using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverSceneControllerScript : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton; // Bot�n de vuelta al men� principal
    [SerializeField] private Button quitButton; // Bot�n de salir

    [Header("Texts")]
    [SerializeField] private TMP_Text winningText; // Texto mostrado al ganar
    [SerializeField] private TMP_Text deathText; // Texto mostrado al morir

    [Header("Game Over Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene"; // Nombre de la escena del men� principal

    private void Awake()
    {
        // Verificar que los botones y textos est�n asignados
        if (mainMenuButton == null || quitButton == null || winningText == null || deathText == null)
        {
            Debug.LogError("One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // Configurar listeners para los botones
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        quitButton.onClick.AddListener(QuitGame);

        // Configurar los textos en funci�n del estado del jugador
        SetupGameOverText();
    }

    private void SetupGameOverText()
    {
        // Obtener el estado del juego de PlayerPrefs
        bool completedAllLevels =
            PlayerPrefs.GetInt("Level1Completed", 0) == 1 &&
            PlayerPrefs.GetInt("Level2Completed", 0) == 1 &&
            PlayerPrefs.GetInt("Level3Completed", 0) == 1 &&
            PlayerPrefs.GetInt("Level4Completed", 0) == 1;

        // Mostrar el texto apropiado
        if (completedAllLevels)
        {
            winningText.gameObject.SetActive(true);
            deathText.gameObject.SetActive(false);
        }
        else
        {
            winningText.gameObject.SetActive(false);
            deathText.gameObject.SetActive(true);
        }
    }

    private void GoToMainMenu()
    {
        Debug.Log("Returning to Main Menu...");

        // Reiniciar todos los datos del jugador
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Cargar la escena del men� principal
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void QuitGame()
    {
        Debug.Log("Exiting game...");

        // Reiniciar los datos antes de salir, por seguridad
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Application.Quit();
    }
}
