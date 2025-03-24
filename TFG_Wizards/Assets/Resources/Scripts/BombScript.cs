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
                player.Damage(player.maxHp); // Provoca da�o igual a la vida m�xima del jugador para garantizar su muerte
            }

            // Opcional: Destruye la bomba despu�s de activarse
            Destroy(gameObject);
        }
    }
}