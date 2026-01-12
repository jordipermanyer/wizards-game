using UnityEngine;

public class ActivarMapa : MonoBehaviour
{
    public GameObject panelMapa;            // Panel del mapa
    public bool congelarMovimiento;         // Si está activado, el jugador no se moverá al abrir el mapa
    public GameObject player;               // Referencia al jugador
    public bool habilitarMapa = false;       // Si está en false, el panel no se puede activar

    private bool panelActivo = false;
    private PlayerController playerController;
    private Rigidbody2D playerRb;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p;
        }

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (!habilitarMapa) return; // Si está desactivado, no hace nada

        if (Input.GetKeyDown(KeyCode.M))
        {
            panelActivo = !panelActivo;
            panelMapa.SetActive(panelActivo);

            if (congelarMovimiento && panelActivo)
            {
                if (playerController != null)
                    playerController.enabled = false;

                if (playerRb != null)
                    playerRb.velocity = Vector2.zero;
            }
            else
            {
                if (playerController != null)
                    playerController.enabled = true;
            }
        }
    }
}
