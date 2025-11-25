using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGeneratorControllerScript : MonoBehaviour
{
    [Header("Items per probabilitat (ordre important)")]
    public List<GameObject> itemPrefabs;
    // 0: mapa   (50%)
    // 1: moneda (15%)
    // 2: pocio  (15%)
    // 3: baculo (10%)
    // 4: bow    (5%)
    // 5: sword  (5%)

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
        if (itemPrefabs == null || itemPrefabs.Count < 6)
        {
            Debug.LogWarning("Falten prefabs assignats a itemPrefabs!");
            return;
        }

        int rnd = Random.Range(1, 101); // 1–100
        int index;

        if (rnd <= 50) index = 0;          // Mapa 50%
        else if (rnd <= 65) index = 1;     // Moneda 15%
        else if (rnd <= 80) index = 2;     // Pocio 15%
        else if (rnd <= 90) index = 3;     // Baculo 10%
        else if (rnd <= 95) index = 4;     // Bow 5%
        else index = 5;                    // Sword 5%

        Instantiate(itemPrefabs[index], transform.position, Quaternion.identity);
    }
}
