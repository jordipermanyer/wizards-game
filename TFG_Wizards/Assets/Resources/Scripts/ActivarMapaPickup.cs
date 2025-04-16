using UnityEngine;

public class ActivarMapaPickup : MonoBehaviour
{
    public ActivarMapa scriptDelMapa; // Arrastra aquí el objeto con el script ActivarMapa

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (scriptDelMapa != null)
            {
                scriptDelMapa.habilitarMapa = true; // Habilita el mapa
                Debug.Log("¡Mapa activado!");
            }
            else
            {
                Debug.LogWarning("No se asignó el script de ActivarMapa.");
            }

            Destroy(gameObject); // Destruye el objeto que tiene este script
        }
    }
}
