using UnityEngine;

public class ActivateObjectOnLoad : MonoBehaviour
{
    [Header("Estado de Recolección de Ítems")]
    public bool hasFinal = false;
    public bool hasFinal2 = false;
    public bool hasFinal3 = false;

    [Header("Objeto a Activar")]
    public GameObject objectToActivate; // Casilla para arrastrar el objeto a activar

    private void Start()
    {
        CheckActivation();
    }

    private void CheckActivation()
    {
        // Obtener los valores almacenados en PlayerPrefs
        int finalCount = PlayerPrefs.GetInt("Final", 0);
        int final2Count = PlayerPrefs.GetInt("Final2", 0);
        int final3Count = PlayerPrefs.GetInt("Final3", 0);

        // Se marcan true solo si el jugador ha recogido al menos un ítem de cada tipo
        hasFinal = finalCount > 0;
        hasFinal2 = final2Count > 0;
        hasFinal3 = final3Count > 0;

        // Activar el objeto si las tres condiciones se cumplen
        if (hasFinal && hasFinal2 && hasFinal3)
        {
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
        }
    }
}
