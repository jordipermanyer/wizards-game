using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterFinalBossDungeonSceneControllerScript : MonoBehaviour
{
    [Header("Scene Name")]
    public string finalBossSceneName = "FinalBossDungeonScene"; // Nombre de la escena del jefe final

    [Header("GameObject Activation")]
    public GameObject finalBossX; // Objeto que se activa al entrar

    private void Start()
    {
        // Validar referencias iniciales
        if (finalBossX == null)
        {
            Debug.LogWarning("FinalBossX GameObject is not assigned. Ensure it's set in the Inspector.", gameObject);
        }

        if (string.IsNullOrEmpty(finalBossSceneName))
        {
            Debug.LogError("FinalBossSceneName is not set. Ensure it's configured in the Inspector.", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger for Final Boss Scene.");

            ActivateFinalBossObject();
            LoadFinalBossScene();
        }
    }

    private void ActivateFinalBossObject()
    {
        if (finalBossX != null)
        {
            finalBossX.SetActive(true);
            Debug.Log($"Activated GameObject: {finalBossX.name}");
        }
        else
        {
            Debug.LogWarning("FinalBossX GameObject is not assigned. Skipping activation.");
        }
    }

    private void LoadFinalBossScene()
    {
        if (!string.IsNullOrEmpty(finalBossSceneName))
        {
            Debug.Log($"Loading Final Boss Scene: {finalBossSceneName}");
            SceneManager.LoadScene(finalBossSceneName);
        }
        else
        {
            Debug.LogError("FinalBossSceneName is not set. Cannot load the scene.");
        }
    }
}
