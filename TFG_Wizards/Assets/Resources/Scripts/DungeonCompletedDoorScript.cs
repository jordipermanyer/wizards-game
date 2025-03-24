using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonCompletedDoorScript : MonoBehaviour
{
    [Header("Level Completion")]
    public int levelToComplete = 1; // Nivel que se completará cuando el jugador atraviese esta puerta

    [Header("Scene Settings")]
    public string lobbySceneName = "GameMapScene"; // Nombre de la escena del lobby

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // Marca el nivel como completado
            MarkLevelAsCompleted();

            // Carga la escena del lobby
            SceneManager.LoadScene(lobbySceneName);
        }
    }

    private void MarkLevelAsCompleted()
    {
        if (levelToComplete >= 1 && levelToComplete <= 4)
        {
            string levelKey = $"Level{levelToComplete}Completed";

            // Actualiza PlayerPrefs
            PlayerPrefs.SetInt(levelKey, 1);
            PlayerPrefs.Save();

            // Sincroniza con el sistema de guardado JSON
            FullGameController.Instance.MarkLevelCompleted(levelToComplete);

            Debug.Log($"Level {levelToComplete} marked as completed.");
        }
        else
        {
            Debug.LogWarning("Invalid levelToComplete value. Ensure it is between 1 and 4.");
        }
    }
}
