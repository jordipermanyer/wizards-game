using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportItem : MonoBehaviour
{
    [Header("Scene Settings")]
    public string targetSceneName; // Nombre de la escena a la que se teletransportará el jugador

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning("No scene name assigned to TeleportItem!");
            }
        }
    }
}
