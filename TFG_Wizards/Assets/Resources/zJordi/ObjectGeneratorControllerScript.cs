using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGeneratorControllerScript : MonoBehaviour
{
    public List<GameObject> itemPrefabs; // Llista pública de prefabs per assignar des de l'inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Comprova si el que ha entrat al trigger és el jugador
        {
            GenerateRandomItem();
            Destroy(gameObject); // Destrueix el cofre després de generar l'objecte
        }
    }

    void GenerateRandomItem()
    {
        if (itemPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, itemPrefabs.Count); // Selecciona un prefab aleatori
            Instantiate(itemPrefabs[randomIndex], transform.position, Quaternion.identity); // Genera l'objecte en la posició del cofre
        }
    }
}

// Script per generar un objecte aleatori entre els prefabs assignats a una llista pública en un trigger 2D.