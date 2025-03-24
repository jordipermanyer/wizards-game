using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player; // Referencia al jugador
    public float smoothSpeed = 0.125f; // Velocidad de movimiento suave de la c�mara
    public Vector3 offset = new Vector3(0, 0, -1); // Offset predeterminado de la c�mara respecto al jugador

    private void Start()
    {
        // Aseg�rate de que la c�mara principal est� habilitada
        if (!Camera.main.enabled)
        {
            Camera.main.enabled = true;
        }

        // Forzar el offset en el eje Z a -1
        if (offset.z != -1)
        {
            offset.z = -1;
            Debug.Log("Offset Z corregido autom�ticamente a -1.");
        }
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference not assigned to CameraController!");
            return;
        }

        // Calcula la posici�n objetivo de la c�mara con el offset
        Vector3 desiredPosition = player.position + offset;

        // Interpola suavemente hacia la posici�n objetivo
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Asigna la posici�n suavizada a la c�mara
        transform.position = smoothedPosition;
    }

    public void InstantMoveToPlayer()
    {
        // Mueve la c�mara instant�neamente al jugador (�til al teletransportarse)
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }

    // M�todo para cambiar din�micamente el offset, asegurando que Z siempre sea -1
    public void SetOffset(Vector3 newOffset)
    {
        offset = new Vector3(newOffset.x, newOffset.y, -1);
        Debug.Log("Offset actualizado din�micamente, Z fijado en -1.");
    }
}
