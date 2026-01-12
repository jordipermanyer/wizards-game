using UnityEngine;
using UnityEngine.SceneManagement;

public class BowScript : MonoBehaviour
{
    [Header("Bow Settings")]
    public int cost = 10; // Cost in coins (store only)
    public float cooldownMultiplier = 0.5f; // 0.5 = 50 percent less cooldown
    public string storeSceneName = "StoreScene";

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.CompareTag("Player"))
            return;

        bool isInStore = SceneManager.GetActiveScene().name == storeSceneName;

        PlayerShooting shooting = collider.GetComponent<PlayerShooting>();
        if (shooting == null)
            return;

        // If in store, charge coins
        if (isInStore)
        {
            int currentCoins = PlayerPrefs.GetInt("Coins", 0);

            if (currentCoins < cost)
            {
                Debug.Log("Not enough coins to acquire the Bow");
                return;
            }

            int newCoins = currentCoins - cost;
            PlayerPrefs.SetInt("Coins", newCoins);
            PlayerPrefs.Save();

            Debug.Log("Bow purchased. Coins left: " + newCoins);
        }
        else
        {
            Debug.Log("Bow picked up for free (not in store)");
        }

        // Apply attack speed bonus (reduce cooldown)
        shooting.shootCooldown *= cooldownMultiplier;

        Debug.Log("Bow applied. New shoot cooldown: " + shooting.shootCooldown);

        Destroy(gameObject);
    }
}
