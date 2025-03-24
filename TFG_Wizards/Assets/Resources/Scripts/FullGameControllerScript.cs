using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class FullGameController : MonoBehaviour
{
    public static FullGameController Instance { get; private set; } // Singleton

    [Header("Save Settings")]
    public string saveFileName = "gameData.json"; // Nombre del archivo JSON
    private string saveFilePath; // Ruta completa al archivo de guardado

    private void Awake()
    {
        // Singleton: asegura que solo exista una instancia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        LoadGame(); // Carga los datos desde el JSON al inicio
    }

    private void Start()
    {
        ApplyVolume(); // Aplica el volumen configurado al inicio
    }

    // ----------------------------------
    // Métodos para Nueva Partida, Cargar Partida y Reiniciar
    // ----------------------------------

    public void NewGame()
    {
        ResetPlayerData();
        Debug.Log("New game initialized.");
        LoadScene("GameMapScene");
    }

    public void ResumeGame()
    {
        if (File.Exists(saveFilePath))
        {
            LoadGame();
            LoadScene("StoreScene");
        }
        else
        {
            Debug.LogWarning("No save file found! Starting a new game.");
            NewGame();
        }
    }

    public void HandlePlayerDeath()
    {
        Debug.Log("Player died. Resetting data and loading Game Over screen.");
        ResetPlayerData();
        LoadScene("GameOverScene");
    }

    private void ResetPlayerData()
    {
        PlayerPrefs.SetInt("PlayerHealth", 100);
        PlayerPrefs.SetString("CurrentItem", "");
        PlayerPrefs.SetInt("Coins", 0);
        PlayerPrefs.SetInt("Level1Completed", 0);
        PlayerPrefs.SetInt("Level2Completed", 0);
        PlayerPrefs.SetInt("Level3Completed", 0);
        PlayerPrefs.SetInt("FinalLevelCompleted", 0);
        PlayerPrefs.SetFloat("GameVolume", 1f);
        PlayerPrefs.Save();
    }

    // ----------------------------------
    // Métodos para Actualizar Datos
    // ----------------------------------

    public void UpdatePlayerHealth(int value)
    {
        int currentHealth = PlayerPrefs.GetInt("PlayerHealth", 100);
        currentHealth = Mathf.Clamp(currentHealth + value, 0, 100);
        PlayerPrefs.SetInt("PlayerHealth", currentHealth);
        PlayerPrefs.Save();

        if (currentHealth <= 0)
        {
            HandlePlayerDeath();
        }

        Debug.Log($"Player Health: {currentHealth}");
    }

    public void UpdateCoins(int value)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        currentCoins = Mathf.Clamp(currentCoins + value, 0, 999);
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.Save();
        Debug.Log($"Coins: {currentCoins}");
    }

    public void MarkLevelCompleted(int levelIndex)
    {
        string levelKey = $"Level{levelIndex + 1}Completed";
        PlayerPrefs.SetInt(levelKey, 1);
        PlayerPrefs.Save();
        Debug.Log($"Level {levelIndex + 1} marked as completed.");
    }

    public int GetCoins()
    {
        return PlayerPrefs.GetInt("Coins", 0);
    }

    public int GetPlayerHealth()
    {
        return PlayerPrefs.GetInt("PlayerHealth", 100);
    }

    // ----------------------------------
    // Métodos de Control de Audio
    // ----------------------------------

    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("GameVolume", value);
        ApplyVolume();
        Debug.Log($"Game Volume: {value}");
    }

    private void ApplyVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("GameVolume", 1f);
    }

    // ----------------------------------
    // Métodos de Transición de Escenas
    // ----------------------------------

    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    // ----------------------------------
    // Métodos de Guardado y Carga
    // ----------------------------------

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            playerHealth = PlayerPrefs.GetInt("PlayerHealth", 100),
            currentItem = PlayerPrefs.GetString("CurrentItem", ""),
            coins = PlayerPrefs.GetInt("Coins", 0),
            levelsCompleted = new int[] {
                PlayerPrefs.GetInt("Level1Completed", 0),
                PlayerPrefs.GetInt("Level2Completed", 0),
                PlayerPrefs.GetInt("Level3Completed", 0),
                PlayerPrefs.GetInt("FinalLevelCompleted", 0),
            },
            gameVolume = PlayerPrefs.GetFloat("GameVolume", 1f)
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Game saved successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                PlayerPrefs.SetInt("PlayerHealth", data.playerHealth);
                PlayerPrefs.SetString("CurrentItem", data.currentItem);
                PlayerPrefs.SetInt("Coins", data.coins);
                PlayerPrefs.SetInt("Level1Completed", data.levelsCompleted[0]);
                PlayerPrefs.SetInt("Level2Completed", data.levelsCompleted[1]);
                PlayerPrefs.SetInt("Level3Completed", data.levelsCompleted[2]);
                PlayerPrefs.SetInt("FinalLevelCompleted", data.levelsCompleted[3]);
                PlayerPrefs.SetFloat("GameVolume", data.gameVolume);
                PlayerPrefs.Save();

                Debug.Log("Game loaded successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading game: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("No save file found. Using default settings.");
        }
    }

    // ----------------------------------
    // Clase Interna para Datos del Juego
    // ----------------------------------

    [System.Serializable]
    private class SaveData
    {
        public int playerHealth;
        public string currentItem;
        public int coins;
        public int[] levelsCompleted;
        public float gameVolume;
    }
}
