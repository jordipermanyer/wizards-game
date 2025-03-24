using UnityEngine;

public class GameMapSceneControllerScript : MonoBehaviour
{
    [Header("Final Boss Activation")]
    public GameObject finalBossScene; // GameObject que representa la entrada al jefe final
    public GameObject xFinalBoss; // GameObject adicional relacionado con el jefe final

    private void Start()
    {
        CheckLevelCompletion(); // Comprueba si los niveles están completados
    }

    private void CheckLevelCompletion()
    {
        // Obtén los datos de niveles completados desde PlayerPrefs (0 = no completado, 1 = completado)
        int level1Completed = PlayerPrefs.GetInt("Level1Completed", 0);
        int level2Completed = PlayerPrefs.GetInt("Level2Completed", 0);
        int level3Completed = PlayerPrefs.GetInt("Level3Completed", 0);

        Debug.Log($"Levels Completed: Level1: {level1Completed}, Level2: {level2Completed}, Level3: {level3Completed}");

        // Comprueba si los niveles 1, 2 y 3 están completados
        if (level1Completed == 1 && level2Completed == 1 && level3Completed == 1)
        {
            Debug.Log("All required levels completed. Activating Final Boss.");

            // Activa los objetos del jefe final
            if (finalBossScene != null)
                finalBossScene.SetActive(true);
            else
                Debug.LogWarning("FinalBossScene GameObject is not assigned.");

            if (xFinalBoss != null)
                xFinalBoss.SetActive(true);
            else
                Debug.LogWarning("XFinalBoss GameObject is not assigned.");
        }
        else
        {
            Debug.Log("Not all levels completed. Final Boss remains inactive.");

            // Desactiva los objetos del jefe final por seguridad
            if (finalBossScene != null)
                finalBossScene.SetActive(false);
            else
                Debug.LogWarning("FinalBossScene GameObject is not assigned.");

            if (xFinalBoss != null)
                xFinalBoss.SetActive(false);
            else
                Debug.LogWarning("XFinalBoss GameObject is not assigned.");
        }
    }
}
