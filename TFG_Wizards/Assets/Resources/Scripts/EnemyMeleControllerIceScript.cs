using System.Collections;
using UnityEngine;

public class EnemyMeleControllerIceScript : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int maxHp = 40; // Vida m�xima del enemigo (el doble del enemigo normal)
    public int contactDamage = 15; // Da�o infligido al tocar al jugador
    public float detectionDistance = 5f; // Distancia a la que detecta al jugador
    public float speed = 2.5f; // Velocidad de movimiento (ligeramente m�s r�pida)

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer; // Capa para detectar los l�mites de la sala

    private Transform playerTransform; // Referencia al jugador
    private int currentHp; // Vida actual
    private bool isPlayerDetected = false; // Estado de detecci�n del jugador
    private Bounds roomBounds; // L�mites de la sala detectados autom�ticamente

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        currentHp = maxHp;
        DetectRoomBounds();
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
        }
    }

    private void ChasePlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + directionToPlayer * speed * Time.deltaTime;

        if (roomBounds.size != Vector3.zero)
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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(contactDamage);
                player.ApplySlowdown(0.5f, 3f);
                Die();
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
        Destroy(gameObject);
    }
}
