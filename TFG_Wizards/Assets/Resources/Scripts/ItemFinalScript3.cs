using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemFinalScript3 : MonoBehaviour
{
    public string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int currentCount = PlayerPrefs.GetInt("Final3", 0) + 1;
            PlayerPrefs.SetInt("Final3", currentCount);
            PlayerPrefs.Save();

            Destroy(gameObject);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
