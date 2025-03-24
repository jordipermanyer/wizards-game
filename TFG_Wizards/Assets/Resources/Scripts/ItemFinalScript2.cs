using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemFinalScript2 : MonoBehaviour
{
    public string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int currentCount = PlayerPrefs.GetInt("Final2", 0) + 1;
            PlayerPrefs.SetInt("Final2", currentCount);
            PlayerPrefs.Save();

            Destroy(gameObject);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
