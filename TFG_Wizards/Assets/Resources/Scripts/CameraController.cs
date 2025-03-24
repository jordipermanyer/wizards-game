using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player; // Referencia al jugador
    public float smoothSpeed = 0.125f; // Velocidad de movimiento suave de la cámara
    public Vector3 offset = new Vector3(0, 0, -1); // Offset predeterminado de la cámara respecto al jugador

    private void Start()
    {
        // Asegúrate de que la cámara principal está habilitada
        if (!Camera.main.enabled)
        {
            Camera.main.enabled = true;
        }

        // Forzar el offset en el eje Z a -1
        if (offset.z != -1)
        {
            offset.z = -1;
            Debug.Log("Offset Z corregido automáticamente a -1.");
        }
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference not assigned to CameraController!");
            return;
        }

        // Calcula la posición objetivo de la cámara con el offset
        Vector3 desiredPosition = player.position + offset;

        // Interpola suavemente hacia la posición objetivo
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Asigna la posición suavizada a la cámara
        transform.position = smoothedPosition;
    }

    public void InstantMoveToPlayer()
    {
        // Mueve la cámara instantáneamente al jugador (útil al teletransportarse)
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }

    // Método para cambiar dinámicamente el offset, asegurando que Z siempre sea -1
    public void SetOffset(Vector3 newOffset)
    {
        offset = new Vector3(newOffset.x, newOffset.y, -1);
        Debug.Log("Offset actualizado dinámicamente, Z fijado en -1.");
    }
}
