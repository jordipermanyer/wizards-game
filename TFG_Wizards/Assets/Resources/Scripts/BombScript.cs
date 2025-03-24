using System.Collections;
using UnityEngine;

public class BombScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Verifica si el objeto que colisiona tiene el tag "Player"
        if (collider.CompareTag("Player"))
        {
            // Obtiene el script del jugador
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(player.maxHp); // Provoca daño igual a la vida máxima del jugador para garantizar su muerte
            }

            // Opcional: Destruye la bomba después de activarse
            Destroy(gameObject);
        }
    }
}