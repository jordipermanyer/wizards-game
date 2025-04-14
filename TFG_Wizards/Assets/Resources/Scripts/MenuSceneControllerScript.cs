using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MenuSceneControllerScript : MonoBehaviour
{
    public string newGameSceneName = "GameMapScene";
    public string resumeGameSceneName = "StoreScene";

    public Button newGameButton;
    public Button resumeGameButton;
    public Button optionsButton;
    public Button quitButton;
    public Slider volumeSlider;
    public Button decreaseVolumeButton;
    public Button increaseVolumeButton;

    public GameObject cargarPartidaPanel;
    public Button regresarButton;

    public Button[] slotButtons; // Botones en el panel de cargarPartida
    public TMP_Text[] slotTexts; // Textos asociados a cada botón

    public GameObject panelGestor;
    public Button playButton;
    public Button deleteButton;

    private string[] slotFileNames = { "slot1.json", "slot2.json", "slot3.json" };
    private int selectedSlotIndex = -1;
    private float volumeStep = 0.1f;

    private void Awake()
    {
        // Verificar referencias asignadas
        if (newGameButton == null || resumeGameButton == null || optionsButton == null || quitButton == null ||
            volumeSlider == null || decreaseVolumeButton == null || increaseVolumeButton == null ||
            cargarPartidaPanel == null || regresarButton == null || panelGestor == null ||
            playButton == null || deleteButton == null || slotButtons.Length != 3 || slotTexts.Length != 3)
        {
            Debug.LogError("Faltan referencias en el Inspector.");
            return;
        }

        // Listeners para menú principal
        newGameButton.onClick.AddListener(NewGame);
        resumeGameButton.onClick.AddListener(OpenCargarPartidaPanel);
        optionsButton.onClick.AddListener(ToggleAudio);
        quitButton.onClick.AddListener(QuitGame);
        volumeSlider.onValueChanged.AddListener(UpdateVolumeFromSlider);
        decreaseVolumeButton.onClick.AddListener(DecreaseVolume);
        increaseVolumeButton.onClick.AddListener(IncreaseVolume);

        // Listeners de carga de partida
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            slotButtons[i].onClick.AddListener(() => SelectSlot(index));
        }

        regresarButton.onClick.AddListener(() =>
        {
            cargarPartidaPanel.SetActive(false);
            panelGestor.SetActive(false);
            selectedSlotIndex = -1;
        });

        deleteButton.onClick.AddListener(DeleteSave);
        playButton.onClick.AddListener(PlaySavedGame);

        // Inicializar volumen
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        volumeSlider.value = savedVolume;
        FullGameController.Instance.SetVolume(savedVolume);

        cargarPartidaPanel.SetActive(false);
        panelGestor.SetActive(false);
    }

    public void NewGame()
    {
        PlayerPrefs.SetInt("PlayerHealth", 100);
        PlayerPrefs.SetInt("PlayerEnergy", 100);
        PlayerPrefs.SetString("CurrentItem", "");
        PlayerPrefs.SetInt("Coins", 0);
        PlayerPrefs.SetInt("Level1Completed", 0);
        PlayerPrefs.SetInt("Level2Completed", 0);
        PlayerPrefs.SetInt("Level3Completed", 0);
        PlayerPrefs.SetInt("FinalBossUnlocked", 0);
        PlayerPrefs.SetInt("Final", 0);
        PlayerPrefs.SetInt("Final2", 0);
        PlayerPrefs.SetInt("Final3", 0);
        PlayerPrefs.SetInt("NPC", 0);
        PlayerPrefs.Save();

        Debug.Log("Nueva partida iniciada.");
        FullGameController.Instance.LoadScene(newGameSceneName);
    }

    public void OpenCargarPartidaPanel()
    {
        cargarPartidaPanel.SetActive(true);
        panelGestor.SetActive(false);
        selectedSlotIndex = -1;

        for (int i = 0; i < slotFileNames.Length; i++)
        {
            string path = Path.Combine(Application.persistentDataPath, slotFileNames[i]);
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                SaveSlotData data = JsonUtility.FromJson<SaveSlotData>(content);
                slotTexts[i].text = data.slotName;
                slotButtons[i].gameObject.SetActive(true);
            }
            else
            {
                slotButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        panelGestor.SetActive(true);
    }

    private void PlaySavedGame()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= slotFileNames.Length)
        {
            Debug.LogError("Slot inválido.");
            return;
        }

        FullGameController.Instance.LoadGameFromFile(slotFileNames[selectedSlotIndex]);
        FullGameController.Instance.LoadScene(resumeGameSceneName);
    }

    private void DeleteSave()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= slotFileNames.Length)
        {
            Debug.LogError("Slot inválido para borrar.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, slotFileNames[selectedSlotIndex]);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Partida eliminada del slot " + (selectedSlotIndex + 1));
        }

        panelGestor.SetActive(false);
        OpenCargarPartidaPanel(); // Recargar los botones de slot
    }

    public void ToggleAudio()
    {
        float currentVolume = volumeSlider.value;
        float newVolume = currentVolume > 0 ? 0f : 1f;
        volumeSlider.value = newVolume;
        PlayerPrefs.SetFloat("GameVolume", newVolume);
        PlayerPrefs.Save();
        FullGameController.Instance.SetVolume(newVolume);
        Debug.Log("Audio toggled. Is Audio On: " + (newVolume > 0));
    }

    public void UpdateVolumeFromSlider(float volume)
    {
        PlayerPrefs.SetFloat("GameVolume", volume);
        PlayerPrefs.Save();
        FullGameController.Instance.SetVolume(volume);
        Debug.Log("Volume adjusted to: " + volume);
    }

    public void DecreaseVolume()
    {
        float newVolume = Mathf.Max(0f, volumeSlider.value - volumeStep);
        volumeSlider.value = newVolume;
        UpdateVolumeFromSlider(newVolume);
    }

    public void IncreaseVolume()
    {
        float newVolume = Mathf.Min(1f, volumeSlider.value + volumeStep);
        volumeSlider.value = newVolume;
        UpdateVolumeFromSlider(newVolume);
    }

    public void QuitGame()
    {
        Debug.Log("Quitted app");
        Application.Quit();
    }

    [System.Serializable]
    public class SaveSlotData
    {
        public string slotName;
        public int playerHealth;
        public string currentItem;
        public int coins;
        public int[] levelsCompleted;
        public float gameVolume;
    }
}
