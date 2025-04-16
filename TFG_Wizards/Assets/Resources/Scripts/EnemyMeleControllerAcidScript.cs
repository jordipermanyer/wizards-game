using System.Collections;
using UnityEngine;

public class EnemyMeleControllerAcidScript : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int maxHp = 40;
    public int contactDamage = 15;
    public int damageOverTime = 5;
    public float damageDuration = 3f;
    public float detectionDistance = 5f;
    public float speed = 2.5f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected = false;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    private bool isReturningToOrigin = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        currentHp = maxHp;
        initialPosition = transform.position;

        DetectRoomBounds();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        if (isReturningToOrigin)
        {
            ReturnToOrigin();
            return;
        }

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

    private void ReturnToOrigin()
    {
        Vector2 directionToOrigin = (initialPosition - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, initialPosition);

        if (distance > 0.1f)
        {
            transform.position += (Vector3)(directionToOrigin * speed * Time.deltaTime);
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;
        }
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
                player.ApplyDamageOverTime(damageOverTime, damageDuration);
                Die(); // El enemigo se autodestruye tras atacar
            }
        }

        if (collider.CompareTag("Pared"))
        {
            isReturningToOrigin = true;
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
