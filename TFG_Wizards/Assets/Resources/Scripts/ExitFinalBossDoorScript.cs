using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitFinalBossDoorScript : MonoBehaviour
{
    [Header("Game Over Scene Settings")]
    public string gameOverSceneName = "GameOverScene"; // Nombre de la escena del Game Over

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // Marcar que el juego se ha completado (activar el texto de victoria)
            PlayerPrefs.SetInt("GameCompleted", 1); // Guardar el estado de victoria en PlayerPrefs
            PlayerPrefs.Save();

            // Cargar la escena de Game Over
            Debug.Log("Game Completed! Loading Game Over Scene...");
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
