using UnityEngine;

public class SwordScript : MonoBehaviour
{
    [Header("Sword Settings")]
    public int cost = 10; // Cost in coins to acquire the Sword

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Verifica si el objeto que colisiona es el jugador
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
                PlayerPrefs.Save(); // Asegúrate de guardar los cambios

                // Duplica el daño del jugador
                PlayerController player = collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.DoubleDamage();
                }

                // Destruye el objeto de la espada
                Destroy(gameObject);

                Debug.Log($"Sword acquired! Attack damage doubled. Coins left: {newCoins}");
            }
            else
            {
                Debug.Log("Not enough coins to acquire the Sword!");
            }
        }
    }
}
