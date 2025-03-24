using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterStoreSceneControllerScript : MonoBehaviour
{
    [Header("Scene Name")]
    public string storeSceneName = "StoreScene"; // Nombre de la escena de la tienda

    private void Start()
    {
        // Validar que el nombre de la escena esté configurado
        if (string.IsNullOrEmpty(storeSceneName))
        {
            Debug.LogError("StoreSceneName is not set. Ensure it's configured in the Inspector.", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger for Store.");
            LoadStoreScene();
        }
    }

    private void LoadStoreScene()
    {
        if (!string.IsNullOrEmpty(storeSceneName))
        {
            Debug.Log($"Loading Store Scene: {storeSceneName}");
            SceneManager.LoadScene(storeSceneName);
        }
        else
        {
            Debug.LogError("StoreSceneName is not set. Cannot load the scene.");
        }
    }
}
