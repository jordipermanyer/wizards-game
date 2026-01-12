using UnityEngine;
using UnityEngine.SceneManagement;

public class SwordScript : MonoBehaviour
{
    [Header("Sword Settings")]
    public int cost = 10; // Cost in coins (store only)
    public float damageMultiplierBonus = 0.5f; // 0.5 = +50 percent damage
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
                Debug.Log("Not enough coins to acquire the Sword");
                return;
            }

            int newCoins = currentCoins - cost;
            PlayerPrefs.SetInt("Coins", newCoins);
            PlayerPrefs.Save();

            Debug.Log("Sword purchased. Coins left: " + newCoins);
        }
        else
        {
            Debug.Log("Sword picked up for free (not in store)");
        }

        // Apply damage bonus
        shooting.damageMultiplier *= (1f + damageMultiplierBonus);

        Debug.Log("Sword applied. New damage multiplier: " + shooting.damageMultiplier);

        Destroy(gameObject);
    }
}
