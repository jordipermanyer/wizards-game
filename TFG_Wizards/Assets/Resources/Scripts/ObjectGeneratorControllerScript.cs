using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGeneratorControllerScript : MonoBehaviour
{
    [Header("Items per probabilitat (ordre important)")]
    public List<GameObject> itemPrefabs;

    [Header("UI tipus bafarada")]
    public GameObject interactPanel; // la bafarada a sobre del cofre

    private bool playerInRange = false;
    private bool chestOpened = false;

    private void Start()
    {
        if (interactPanel != null)
            interactPanel.SetActive(false); // assegurem que està amagat al principi
    }

    private void Update()
    {
        if (playerInRange && !chestOpened)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenChest();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactPanel != null)
                interactPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactPanel != null)
                interactPanel.SetActive(false);
        }
    }

    void OpenChest()
    {
        chestOpened = true;

        if (interactPanel != null)
            interactPanel.SetActive(false);

        GenerateRandomItem();
        Destroy(gameObject); // Destrueix el cofre
    }

    void GenerateRandomItem()
    {
        if (itemPrefabs == null || itemPrefabs.Count < 4)
        {
            Debug.LogWarning("Falten prefabs assignats a itemPrefabs!");
            return;
        }

        int rnd = Random.Range(1, 101); // 1–100
        int index;

        if (rnd <= 25) index = 0;          // Mapa 25%
        else if (rnd <= 50) index = 1;     // Moneda 25%
        else if (rnd <= 75) index = 2;     // Pocio 25%
        else index = 3;                    // Baculo 25%

        Instantiate(itemPrefabs[index], transform.position, Quaternion.identity);
    }
}
