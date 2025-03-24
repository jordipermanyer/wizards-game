using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHp = 1000; // Vida máxima del boss
    public int contactDamage = 10; // Daño por contacto
    public float detectionDistance = 15f; // Distancia de detección del jugador
    public float speed = 2f; // Velocidad de movimiento
    public Slider healthBar; // Barra de vida del boss

    [Header("Shooting")]
    public GameObject bulletPrefab; // Prefab de la bala
    public int bulletDamage = 15; // Daño de la bala
    public float shootInterval = 1.5f; // Intervalo entre disparos

    [Header("Enemy Spawning")]
    public GameObject enemyPrefab1; // Primer tipo de enemigo
    public GameObject enemyPrefab2; // Segundo tipo de enemigo
    public Transform[] spawnPoints; // Puntos de referencia para el spawn

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer; // Capa para detectar los límites de la sala

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;
    private float enemySpawnInterval = 5f; // Intervalo inicial de spawn de enemigos
    private float enemyPrefab1Chance = 0.7f; // Probabilidad inicial de spawn del prefab 1

    private void Start()
    {
        currentHp = maxHp;
        healthBar.maxValue = maxHp;
        healthBar.value = maxHp;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        DetectRoomBounds();

        StartCoroutine(ShootAtPlayer());
        StartCoroutine(SpawnEnemies());
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        if (isPlayerDetected)
        {
            ChasePlayer();
        }
    }

    private void DetectRoomBounds()
    {
        Collider2D roomBoundsCollider = Physics2D.OverlapCircle(transform.position, 0.1f, roomBoundsLayer);
        if (roomBoundsCollider != null)
        {
            roomBounds = roomBoundsCollider.bounds;
            Debug.Log($"Room bounds detected: {roomBounds}");
        }
        else
        {
            Debug.LogWarning("Room bounds not detected. The boss might leave the intended area.");
        }
    }

    private void ChasePlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + directionToPlayer * speed * Time.deltaTime;

        if (roomBounds.size != Vector3.zero) // Solo aplicar límites si se detectaron
        {
            newPosition = ClampToRoomBounds(newPosition);
        }

        transform.position = newPosition;
    }

    private Vector2 ClampToRoomBounds(Vector2 position)
    {
        position.x = Mathf.Clamp(position.x, roomBounds.min.x, roomBounds.max.x);
        position.y = Mathf.Clamp(position.y, roomBounds.min.y, roomBounds.max.y);
        return position;
    }

    private IEnumerator ShootAtPlayer()
    {
        while (true)
        {
            if (isPlayerDetected && playerTransform != null && bulletPrefab != null)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<Bullet>().Initialize(directionToPlayer, bulletDamage);
            }
            yield return new WaitForSeconds(shootInterval);
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (currentHp > 0)
        {
            if (currentHp <= 750) enemySpawnInterval = 5f;
            if (currentHp <= 500) enemySpawnInterval = 2f;
            if (currentHp <= 350) enemySpawnInterval = 2f;

            float spawnChance = Random.value;
            GameObject enemyToSpawn = spawnChance <= enemyPrefab1Chance ? enemyPrefab1 : enemyPrefab2;
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(enemyToSpawn, spawnPoint.position, Quaternion.identity);

            if (currentHp <= 500) enemyPrefab1Chance = 0.5f;
            if (currentHp <= 350)
            {
                enemyPrefab1Chance = 0.3f;
                StartCoroutine(ShootInAllDirections());
            }

            yield return new WaitForSeconds(enemySpawnInterval);
        }
    }

    private IEnumerator ShootInAllDirections()
    {
        while (currentHp <= 350 && currentHp > 0)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<Bullet>().Initialize(direction, bulletDamage);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(contactDamage);
            }
        }
    }

    public void Damage(int damage)
    {
        currentHp -= damage;
        healthBar.value = currentHp;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Boss defeated.");
        Destroy(gameObject);
    }
}