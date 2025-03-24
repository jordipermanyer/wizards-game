using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterLvl2DungeonSceneControllerScript : MonoBehaviour
{
    [Header("Scene Name")]
    public string lvl2SceneName = "Level2DungeonScene"; // Nombre de la escena del nivel 2

    private void Start()
    {
        // Validar que el nombre de la escena esté configurado
        if (string.IsNullOrEmpty(lvl2SceneName))
        {
            Debug.LogError("Lvl2SceneName is not set. Ensure it's configured in the Inspector.", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger for Level 2 Dungeon.");
            LoadLevel2Scene();
        }
    }

    private void LoadLevel2Scene()
    {
        if (!string.IsNullOrEmpty(lvl2SceneName))
        {
            Debug.Log($"Loading Level 2 Dungeon Scene: {lvl2SceneName}");
            SceneManager.LoadScene(lvl2SceneName);
        }
        else
        {
            Debug.LogError("Lvl2SceneName is not set. Cannot load the scene.");
        }
    }
}
