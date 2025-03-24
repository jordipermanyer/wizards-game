using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToGameMapScript : MonoBehaviour
{
    [Header("Scene Name")]
    public string gameMapSceneName = "GameMapScene"; // Nombre de la escena del mapa del juego

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica si el objeto que colisiona tiene el tag "Player"
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger. Returning to GameMapScene...");
            SceneManager.LoadScene(gameMapSceneName);
        }
    }
}
