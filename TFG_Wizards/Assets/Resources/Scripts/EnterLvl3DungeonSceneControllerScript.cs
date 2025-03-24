using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterLvl3DungeonSceneControllerScript : MonoBehaviour
{
    [Header("Scene Name")]
    public string lvl3SceneName = "Level3DungeonScene"; // Nombre de la escena del nivel 3

    private void Start()
    {
        // Validar que el nombre de la escena esté configurado
        if (string.IsNullOrEmpty(lvl3SceneName))
        {
            Debug.LogError("Lvl3SceneName is not set. Ensure it's configured in the Inspector.", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger for Level 3 Dungeon.");
            LoadLevel3Scene();
        }
    }

    private void LoadLevel3Scene()
    {
        if (!string.IsNullOrEmpty(lvl3SceneName))
        {
            Debug.Log($"Loading Level 3 Dungeon Scene: {lvl3SceneName}");
            SceneManager.LoadScene(lvl3SceneName);
        }
        else
        {
            Debug.LogError("Lvl3SceneName is not set. Cannot load the scene.");
        }
    }
}
