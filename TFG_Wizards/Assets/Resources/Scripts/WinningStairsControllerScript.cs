using UnityEngine;
using UnityEngine.SceneManagement;

public class WinningStairsControllerScript : MonoBehaviour
{
    [Header("Scene Name")]
    public string winScene = "WinScene"; // Nombre de la escena del mapa del juego

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica si el objeto que colisiona tiene el tag "Player"
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger. Winning...");
            SceneManager.LoadScene(winScene);
        }
    }
}
