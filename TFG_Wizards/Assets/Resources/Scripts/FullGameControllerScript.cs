using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class FullGameController : MonoBehaviour
{
    public static FullGameController Instance { get; private set; }

    [Header("Save Settings")]
    public string saveFileName = "gameData.json";
    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        LoadGame();
    }

    private void Start()
    {
        ApplyVolume();
    }

    // ------------------------- NUEVA PARTIDA -------------------------
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
        PlayerPrefs.SetFloat("GameVolume", 1f);
        PlayerPrefs.Save();
    }

    // ------------------------- ACTUALIZAR DATOS -------------------------
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

    // ------------------------- CONTROL DE AUDIO -------------------------
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

    // ------------------------- TRANSICIÓN DE ESCENAS -------------------------
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    // ------------------------- GUARDAR Y CARGAR -------------------------
    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            playerHealth = PlayerPrefs.GetInt("PlayerHealth", 100),
            playerEnergy = PlayerPrefs.GetInt("PlayerEnergy", 100),
            currentItem = PlayerPrefs.GetString("CurrentItem", ""),
            coins = PlayerPrefs.GetInt("Coins", 0),
            level1 = PlayerPrefs.GetInt("Level1Completed", 0),
            level2 = PlayerPrefs.GetInt("Level2Completed", 0),
            level3 = PlayerPrefs.GetInt("Level3Completed", 0),
            finalBossUnlocked = PlayerPrefs.GetInt("FinalBossUnlocked", 0),
            final = PlayerPrefs.GetInt("Final", 0),
            final2 = PlayerPrefs.GetInt("Final2", 0),
            final3 = PlayerPrefs.GetInt("Final3", 0),
            npc = PlayerPrefs.GetInt("NPC", 0),
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
                PlayerPrefs.SetInt("PlayerEnergy", data.playerEnergy);
                PlayerPrefs.SetString("CurrentItem", data.currentItem);
                PlayerPrefs.SetInt("Coins", data.coins);
                PlayerPrefs.SetInt("Level1Completed", data.level1);
                PlayerPrefs.SetInt("Level2Completed", data.level2);
                PlayerPrefs.SetInt("Level3Completed", data.level3);
                PlayerPrefs.SetInt("FinalBossUnlocked", data.finalBossUnlocked);
                PlayerPrefs.SetInt("Final", data.final);
                PlayerPrefs.SetInt("Final2", data.final2);
                PlayerPrefs.SetInt("Final3", data.final3);
                PlayerPrefs.SetInt("NPC", data.npc);
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

    // ------------------------- GUARDADO CON SLOT Y NOMBRE -------------------------
    public void SaveGameToFile(string fileName, string customName)
    {
        BedSaveSystem.SaveSlotData data = new BedSaveSystem.SaveSlotData
        {
            slotName = customName,
            playerHealth = PlayerPrefs.GetInt("PlayerHealth", 100),
            playerEnergy = PlayerPrefs.GetInt("PlayerEnergy", 100),
            currentItem = PlayerPrefs.GetString("CurrentItem", ""),
            coins = PlayerPrefs.GetInt("Coins", 0),
            level1 = PlayerPrefs.GetInt("Level1Completed", 0),
            level2 = PlayerPrefs.GetInt("Level2Completed", 0),
            level3 = PlayerPrefs.GetInt("Level3Completed", 0),
            finalBossUnlocked = PlayerPrefs.GetInt("FinalBossUnlocked", 0),
            final = PlayerPrefs.GetInt("Final", 0),
            final2 = PlayerPrefs.GetInt("Final2", 0),
            final3 = PlayerPrefs.GetInt("Final3", 0),
            npc = PlayerPrefs.GetInt("NPC", 0),
            gameVolume = PlayerPrefs.GetFloat("GameVolume", 1f)
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(path, json);
            Debug.Log($"Partida guardada en archivo {fileName} con nombre {customName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar partida en {fileName}: {e.Message}");
        }
    }

    public void LoadGameFromFile(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                BedSaveSystem.SaveSlotData data = JsonUtility.FromJson<BedSaveSystem.SaveSlotData>(json);

                PlayerPrefs.SetInt("PlayerHealth", data.playerHealth);
                PlayerPrefs.SetInt("PlayerEnergy", data.playerEnergy);
                PlayerPrefs.SetString("CurrentItem", data.currentItem);
                PlayerPrefs.SetInt("Coins", data.coins);
                PlayerPrefs.SetInt("Level1Completed", data.level1);
                PlayerPrefs.SetInt("Level2Completed", data.level2);
                PlayerPrefs.SetInt("Level3Completed", data.level3);
                PlayerPrefs.SetInt("FinalBossUnlocked", data.finalBossUnlocked);
                PlayerPrefs.SetInt("Final", data.final);
                PlayerPrefs.SetInt("Final2", data.final2);
                PlayerPrefs.SetInt("Final3", data.final3);
                PlayerPrefs.SetInt("NPC", data.npc);
                PlayerPrefs.SetFloat("GameVolume", data.gameVolume);
                PlayerPrefs.Save();

                Debug.Log($"Partida cargada desde {fileName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error al cargar partida desde {fileName}: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"El archivo de guardado {fileName} no existe.");
        }
    }

    // ------------------------- CLASE INTERNA DE GUARDADO -------------------------
    [System.Serializable]
    private class SaveData
    {
        public int playerHealth;
        public int playerEnergy;
        public string currentItem;
        public int coins;
        public int level1;
        public int level2;
        public int level3;
        public int finalBossUnlocked;
        public int final;
        public int final2;
        public int final3;
        public int npc;
        public float gameVolume;
    }
}
