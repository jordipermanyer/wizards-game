using System.Collections;
using UnityEngine;

public class EnemyShooterControllerDroopScript : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHp = 10; // Vida m�xima
    public int contactDamage = 5; // Da�o por contacto
    public float detectionDistance = 10f; // Distancia de detecci�n del jugador
    public float speed = 3f; // Velocidad de movimiento
    public float wanderSpeed = 1f; // Velocidad al deambular
    public float wanderInterval = 2f; // Intervalo para cambiar de direcci�n al deambular

    [Header("Shooting")]
    public GameObject bulletPrefab; // Prefab de la bala
    public int bulletDamage = 10; // Da�o de la bala
    public float shootInterval = 2f; // Intervalo entre disparos

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer; // Capa para detectar los l�mites de la sala

    [Header("Drop System")]
    public GameObject healthPickupPrefab; // Prefab de vida
    public GameObject attackReloadPrefab; // Prefab de recarga de ataque
    public GameObject coinPrefab; // Prefab de moneda

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds; // L�mites de la sala detectados autom�ticamente

    private void Start()
    {
        currentHp = maxHp;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Detectar los l�mites de la sala autom�ticamente
        DetectRoomBounds();

        StartCoroutine(Wander());
        StartCoroutine(ShootAtPlayer());
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
            Debug.LogWarning("Room bounds not detected. The enemy might leave the intended area.");
        }
    }

    private void ChasePlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + directionToPlayer * speed * Time.deltaTime;

        if (roomBounds.size != Vector3.zero) // Solo aplicar l�mites si se detectaron
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

    private IEnumerator Wander()
    {
        while (true)
        {
            yield return new WaitForSeconds(wanderInterval);
        }
    }

    private IEnumerator ShootAtPlayer()
    {
        while (true)
        {
            if (isPlayerDetected && playerTransform != null && bulletPrefab != null)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Instanciar la bala
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<Bullet>().Initialize(directionToPlayer, bulletDamage);
            }

            yield return new WaitForSeconds(shootInterval);
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

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy defeated.");
        DropLoot(); // Llamar al sistema de dropeo
        Destroy(gameObject);
    }

    private void DropLoot()
    {
        float dropChance = Random.value; // Genera un n�mero entre 0.0 y 1.0

        if (dropChance <= 0.2f && healthPickupPrefab != null)
        {
            Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
            Debug.Log("Dropped: Health Pickup");
        }
        else if (dropChance <= 0.4f && attackReloadPrefab != null)
        {
            Instantiate(attackReloadPrefab, transform.position, Quaternion.identity);
            Debug.Log("Dropped: Attack Reload");
        }
        else if (dropChance <= 0.5f && coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
            Debug.Log("Dropped: Coin");
        }
        else
        {
            Debug.Log("No drop.");
        }
    }
}
