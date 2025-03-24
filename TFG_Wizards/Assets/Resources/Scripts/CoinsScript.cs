using UnityEngine;

public class CoinsScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Obtén el script del jugador
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Añade una moneda al jugador
                player.AddCoins(1);

                // Destruye la moneda después de recogerla
                Destroy(gameObject);

                Debug.Log("Coin collected! Total coins: " + PlayerPrefs.GetInt("Coins", 0));
            }
            else
            {
                Debug.LogWarning("PlayerController script not found on the Player object!");
            }
        }
    }
}
