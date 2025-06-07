using System.Collections;
using UnityEngine;

public class EnemyMeleControllerScript : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int maxHp = 20;
    public int contactDamage = 10;
    public float detectionDistance = 5f;
    public float speed = 2f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected = false;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    private bool isReturningToOrigin = false;

    // Animator
    private Animator animator;

    // Variables para animación de movimiento
    private Vector2 lastMoveDirection = Vector2.down; // Dirección idle inicial

    // Sonidos
    [Header("Sound Settings")]
    public AudioClip idleSound;
    public AudioClip walkingSound;
    private AudioSource audioSource;

    private bool wasMovingLastFrame = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        currentHp = maxHp;
        initialPosition = transform.position;

        // Obtener referencia al Animator
        animator = GetComponent<Animator>();

        // Obtener referencia al AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
        }

        DetectRoomBounds();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        if (isReturningToOrigin)
        {
            animator.SetBool("Move", true);
            PlayMovementSound(true);
            ReturnToOrigin();
            return;
        }

        animator.SetBool("Move", isPlayerDetected);
        PlayMovementSound(isPlayerDetected);

        if (isPlayerDetected)
        {
            ChasePlayer();
        }
        else
        {
            // Si no se está moviendo, actualizar idleX e idleY con la última dirección de movimiento
            animator.SetFloat("IdleX", lastMoveDirection.x);
            animator.SetFloat("IdleY", lastMoveDirection.y);
        }
    }

    private void PlayMovementSound(bool isMoving)
    {
        if (isMoving != wasMovingLastFrame)
        {
            wasMovingLastFrame = isMoving;

            if (isMoving)
            {
                if (walkingSound != null)
                {
                    audioSource.clip = walkingSound;
                    audioSource.Play();
                }
            }
            else
            {
                if (idleSound != null)
                {
                    audioSource.clip = idleSound;
                    audioSource.Play();
                }
            }
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

        // Actualizar animación de movimiento
        animator.SetFloat("MovimientoX", directionToPlayer.x);
        animator.SetFloat("MovimientoY", directionToPlayer.y);

        // Guardar última dirección de movimiento para Idle
        if (directionToPlayer != Vector2.zero)
        {
            lastMoveDirection = directionToPlayer;
        }

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

        // Actualizar animación de movimiento
        animator.SetFloat("MovimientoX", directionToOrigin.x);
        animator.SetFloat("MovimientoY", directionToOrigin.y);

        // Guardar última dirección de movimiento para Idle
        if (directionToOrigin != Vector2.zero)
        {
            lastMoveDirection = directionToOrigin;
        }

        if (distance > 0.1f)
        {
            transform.position += (Vector3)(directionToOrigin * speed * Time.deltaTime);
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;

            // Cuando llega al origen, pasar a Idle
            animator.SetBool("Move", false);
            PlayMovementSound(false);
            animator.SetFloat("IdleX", lastMoveDirection.x);
            animator.SetFloat("IdleY", lastMoveDirection.y);
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
                Die();
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
        Debug.Log("Enemy defeated.");
        Destroy(gameObject);
    }
}
