using UnityEngine;

public class HealthPotionScript : MonoBehaviour
{
    [Header("Health Potion Settings")]
    public int cost = 5; // Costo de la poción en monedas
    public int healAmount = 25; // Cantidad de vida restaurada por la poción
    private string storeSceneName = "StoreScene"; // Nombre de la escena de la tienda

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            bool isInStore = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == storeSceneName;
            PlayerController player = collider.GetComponent<PlayerController>();

            if (player != null)
            {
                // Si está en la tienda, cobra monedas
                if (isInStore)
                {
                    int currentCoins = PlayerPrefs.GetInt("Coins", 0);

                    if (currentCoins >= cost)
                    {
                        // Cobra al jugador y cura
                        int newCoins = currentCoins - cost;
                        PlayerPrefs.SetInt("Coins", newCoins);
                        PlayerPrefs.Save();

                        player.Heal(healAmount);
                        Debug.Log($"Health Potion used! Player healed by {healAmount} HP. Coins left: {newCoins}");
                    }
                    else
                    {
                        Debug.Log("Not enough coins to acquire the Health Potion!");
                        return;
                    }
                }
                else
                {
                    // Fuera de la tienda, es gratis
                    player.Heal(healAmount);
                    Debug.Log($"Health Potion used for free! Player healed by {healAmount} HP.");
                }

                // Destruye el objeto de la poción
                Destroy(gameObject);
            }
        }
    }
}
