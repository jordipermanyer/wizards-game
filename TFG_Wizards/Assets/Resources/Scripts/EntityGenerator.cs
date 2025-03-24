using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGenerator : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject enemyPrefab1; // Primer tipo de enemigo
    public GameObject enemyPrefab2; // Segundo tipo de enemigo

    [Header("Generation Settings")]
    public int minEnemies = 1; // Mínimo de enemigos a generar
    public int maxEnemies = 4; // Máximo de enemigos a generar
    public float spawnOffset = 1.5f; // Separación entre enemigos

    [Header("References")]
    public Transform enemyGroup; // Grupo donde se parentan los enemigos generados

    private void Start()
    {
        GenerateEnemies();
    }

    private void GenerateEnemies()
    {
        if (enemyPrefab1 == null || enemyPrefab2 == null)
        {
            Debug.LogError("Enemy prefabs are not assigned in the Inspector.", gameObject);
            return;
        }

        if (enemyGroup == null)
        {
            Debug.LogError("Enemy group Transform is not assigned in the Inspector.", gameObject);
            return;
        }

        int enemyCount = Random.Range(minEnemies, maxEnemies + 1); // Número aleatorio de enemigos a generar
        Debug.Log($"Generating {enemyCount} enemies...");

        for (int i = 0; i < enemyCount; i++)
        {
            // Seleccionar aleatoriamente uno de los dos prefabs
            GameObject selectedPrefab = Random.value < 0.5f ? enemyPrefab1 : enemyPrefab2;

            // Calcular posición aleatoria con un desplazamiento
            Vector3 spawnPosition = transform.position + new Vector3(
                Random.Range(-spawnOffset, spawnOffset),
                Random.Range(-spawnOffset, spawnOffset),
                0
            );

            // Generar enemigo y parentarlo al grupo de enemigos
            GameObject enemy = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, enemyGroup);

            Debug.Log($"Enemy generated at {spawnPosition}");
        }
    }
}
