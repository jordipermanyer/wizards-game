using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneControllerScript : MonoBehaviour
{
    // Public fields for Inspector
    public string newGameSceneName = "GameMapScene"; // Nombre de la escena para nueva partida
    public string resumeGameSceneName = "StoreScene"; // Nombre de la escena para reanudar
    public Button newGameButton; // Botón de nueva partida
    public Button resumeGameButton; // Botón de reanudar partida
    public Button optionsButton; // Botón de opciones (muteo)
    public Button quitButton; // Botón de salir
    public Slider volumeSlider; // Slider para ajustar volumen
    public Button decreaseVolumeButton; // Botón para disminuir volumen
    public Button increaseVolumeButton; // Botón para aumentar volumen

    private float volumeStep = 0.1f; // Incremento/decremento del volumen

    private void Awake()
    {
        // Verificar que todos los elementos están asignados
        if (newGameButton == null || resumeGameButton == null || optionsButton == null || quitButton == null || volumeSlider == null || decreaseVolumeButton == null || increaseVolumeButton == null)
        {
            Debug.LogError("One or more UI elements are not assigned in the Inspector!");
        }

        // Asignar listeners a los botones
        newGameButton.onClick.AddListener(NewGame);
        resumeGameButton.onClick.AddListener(ResumeGame);
        optionsButton.onClick.AddListener(ToggleAudio);
        quitButton.onClick.AddListener(QuitGame);
        volumeSlider.onValueChanged.AddListener(UpdateVolumeFromSlider);
        decreaseVolumeButton.onClick.AddListener(DecreaseVolume);
        increaseVolumeButton.onClick.AddListener(IncreaseVolume);

        // Cargar el volumen guardado al iniciar el menú
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.value = savedVolume;
        FullGameController.Instance.SetVolume(savedVolume);
    }

    // Iniciar nueva partida
    public void NewGame()
    {
        // Resetear los valores de la partida
        PlayerPrefs.SetInt("PlayerHealth", 100);
        PlayerPrefs.SetInt("PlayerEnergy", 100); // Reiniciar la energía a 100
        PlayerPrefs.SetString("CurrentItem", "");
        PlayerPrefs.SetInt("Coins", 0);
        PlayerPrefs.SetInt("Level1Completed", 0);
        PlayerPrefs.SetInt("Level2Completed", 0);
        PlayerPrefs.SetInt("Level3Completed", 0);
        PlayerPrefs.SetInt("FinalBossUnlocked", 0);

        // Resetear los contadores de los ítems Final, Final2 y Final3
        PlayerPrefs.SetInt("Final", 0);
        PlayerPrefs.SetInt("Final2", 0);
        PlayerPrefs.SetInt("Final3", 0);

        // Resetear el dato del NPC a 0
        PlayerPrefs.SetInt("NPC", 0);

        PlayerPrefs.Save(); // Guardar los cambios

        Debug.Log("Nueva partida iniciada. Todos los datos han sido reseteados.");
        FullGameController.Instance.LoadScene(newGameSceneName);
    }

    // Reanudar partida
    public void ResumeGame()
    {
        if (PlayerPrefs.HasKey("PlayerHealth"))
        {
            Debug.Log("Game loaded from PlayerPrefs.");
            FullGameController.Instance.LoadScene(resumeGameSceneName);
        }
        else
        {
            Debug.LogWarning("No save data found! Starting a new game.");
            NewGame();
        }
    }

    // Alternar muteo del audio con el botón
    public void ToggleAudio()
    {
        float currentVolume = volumeSlider.value;
        float newVolume = currentVolume > 0 ? 0f : 1f;

        volumeSlider.value = newVolume; // Actualizar el slider al nuevo valor
        PlayerPrefs.SetFloat("GameVolume", newVolume);
        PlayerPrefs.Save();

        FullGameController.Instance.SetVolume(newVolume);
        Debug.Log($"Audio toggled. Is Audio On: {newVolume > 0}");
    }

    // Ajustar volumen con el Slider
    public void UpdateVolumeFromSlider(float volume)
    {
        PlayerPrefs.SetFloat("GameVolume", volume);
        PlayerPrefs.Save();
        FullGameController.Instance.SetVolume(volume);
        Debug.Log($"Volume adjusted to: {volume}");
    }

    // Disminuir volumen con el botón de la izquierda
    public void DecreaseVolume()
    {
        float newVolume = Mathf.Max(0f, volumeSlider.value - volumeStep);
        volumeSlider.value = newVolume; // Actualizar el slider
        UpdateVolumeFromSlider(newVolume);
    }

    // Aumentar volumen con el botón de la derecha
    public void IncreaseVolume()
    {
        float newVolume = Mathf.Min(1f, volumeSlider.value + volumeStep);
        volumeSlider.value = newVolume; // Actualizar el slider
        UpdateVolumeFromSlider(newVolume);
    }

    // Salir del juego
    public void QuitGame()
    {
        Debug.Log("Quitted app");
        Application.Quit();
    }
}
