using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemFinalScript : MonoBehaviour
{
    public string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Aumentar el contador del ítem
            int currentCount = PlayerPrefs.GetInt("Final", 0) + 1;
            PlayerPrefs.SetInt("Final", currentCount);
            PlayerPrefs.Save(); // Guardar los datos permanentemente

            // Destruir el ítem y cargar la nueva escena
            Destroy(gameObject);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
