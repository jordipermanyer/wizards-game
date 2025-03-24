using UnityEngine;

public class BowScript : MonoBehaviour
{
    [Header("Bow Settings")]
    public int cost = 10; // Cost in coins to acquire the Bow

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // Obtén las monedas actuales del jugador desde PlayerPrefs
            int currentCoins = PlayerPrefs.GetInt("Coins", 0);

            // Comprueba si el jugador tiene suficientes monedas
            if (currentCoins >= cost)
            {
                // Resta las monedas del jugador y actualiza PlayerPrefs
                int newCoins = currentCoins - cost;
                PlayerPrefs.SetInt("Coins", newCoins);

                // Reduce el tiempo de enfriamiento del disparo del jugador
                PlayerController player = collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.ReduceShootCooldown();
                }

                // Destruye el objeto del arco
                Destroy(gameObject);

                Debug.Log($"Bow acquired! Shoot cooldown reduced. Coins left: {newCoins}");
            }
            else
            {
                Debug.Log("Not enough coins to acquire the Bow!");
            }
        }
    }
}
