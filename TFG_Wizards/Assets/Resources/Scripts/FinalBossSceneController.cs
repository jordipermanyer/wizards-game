using UnityEngine;

public class FinalBossSceneController : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject finalBoss; // Referencia al jefe final
    public GameObject levelCompletionObject; // Objeto que se activa al derrotar al jefe

    private bool bossDefeated = false;

    void Update()
    {
        // Comprobar si el jefe ha sido derrotado
        if (finalBoss == null && !bossDefeated)
        {
            bossDefeated = true;
            HandleBossDefeat();
        }
    }

    private void HandleBossDefeat()
    {
        if (levelCompletionObject != null)
        {
            levelCompletionObject.SetActive(true); // Activa el objeto de finalización
        }

        // Marcar el nivel 4 como completado en PlayerPrefs
        PlayerPrefs.SetInt("Level4Completed", 1);
        PlayerPrefs.Save();

        Debug.Log("Boss defeated! Level 4 completed.");
    }
}
