using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterLvl1DungeonSceneControllerScript : MonoBehaviour
{
    [Header("Scene Name")]
    public string lvl1SceneName = "Level1DungeonScene"; // Nombre de la escena del nivel 1

    private void Start()
    {
        // Validar que el nombre de la escena esté configurado
        if (string.IsNullOrEmpty(lvl1SceneName))
        {
            Debug.LogError("Lvl1SceneName is not set. Ensure it's configured in the Inspector.", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger for Level 1 Dungeon.");
            LoadLevel1Scene();
        }
    }

    private void LoadLevel1Scene()
    {
        if (!string.IsNullOrEmpty(lvl1SceneName))
        {
            Debug.Log($"Loading Level 1 Dungeon Scene: {lvl1SceneName}");
            SceneManager.LoadScene(lvl1SceneName);
        }
        else
        {
            Debug.LogError("Lvl1SceneName is not set. Cannot load the scene.");
        }
    }
}
