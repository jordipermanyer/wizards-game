using UnityEngine;

public class ActivarMapa : MonoBehaviour
{
    public GameObject panelMapa;            // Panel del mapa
    public bool congelarMovimiento;         // Si está activado, el jugador no se moverá al abrir el mapa
    public GameObject player;               // Referencia al jugador

    private bool panelActivo = false;
    private PlayerController playerController;
    private Rigidbody2D playerRb;

    void Start()
    {
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            panelActivo = !panelActivo;
            panelMapa.SetActive(panelActivo);

            if (congelarMovimiento && panelActivo)
            {
                if (playerController != null)
                    playerController.enabled = false;

                if (playerRb != null)
                    playerRb.velocity = Vector2.zero; // Detiene el movimiento completamente
            }
            else
            {
                if (playerController != null)
                    playerController.enabled = true;
            }
        }
    }
}
