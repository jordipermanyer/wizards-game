using UnityEngine;

public class ActivarMapaPickup : MonoBehaviour
{
    public ActivarMapa scriptDelMapa;

    [Header("Auto Assign")]
    public bool autoAssignOnStart = true;
    public string mapControllerObjectName = "CameraMiniMapa";

    private void Start()
    {
        if (!autoAssignOnStart) return;

        // Try assigned reference first
        if (scriptDelMapa != null) return;

        // Option 1: find by object name (fast and controlled)
        GameObject obj = GameObject.Find(mapControllerObjectName);
        if (obj != null)
        {
            scriptDelMapa = obj.GetComponent<ActivarMapa>();
        }

        // Option 2: fallback find in scene
        if (scriptDelMapa == null)
        {
            scriptDelMapa = FindObjectOfType<ActivarMapa>();
        }

        if (scriptDelMapa == null)
        {
            Debug.LogWarning("ActivarMapaPickup: ActivarMapa not found in scene.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // Ensure we have the reference even if Start did not run as expected
        if (scriptDelMapa == null)
        {
            GameObject obj = GameObject.Find(mapControllerObjectName);
            if (obj != null)
                scriptDelMapa = obj.GetComponent<ActivarMapa>();

            if (scriptDelMapa == null)
                scriptDelMapa = FindObjectOfType<ActivarMapa>();
        }

        if (scriptDelMapa != null)
        {
            scriptDelMapa.habilitarMapa = true;
            Debug.Log("Map enabled.");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("ActivarMapaPickup: scriptDelMapa is null. Map not enabled.");
        }
    }
}
