using System.Collections;
using UnityEngine;

public class EnemyMeleControllerAcidScript : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int maxHp = 60;
    public int contactDamage = 15;
    public int damageOverTime = 5;
    public float damageDuration = 3f;
    public float detectionDistance = 5f;
    public float speed = 2.5f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Sounds")]
    public AudioSource idleSoundSource; // Sonido de idle
    public AudioSource walkSoundSource; // Sonido de caminar

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected = false;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    private bool isReturningToOrigin = false;

    private Animator animator;
    private bool isPlayingWalkSound = false; // Control de sonido

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        currentHp = maxHp;
        initialPosition = transform.position;

        animator = GetComponent<Animator>();

        DetectRoomBounds();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        animator.SetBool("Move", isPlayerDetected);

        HandleSound(isPlayerDetected); // Manejar sonidos

        if (isReturningToOrigin)
        {
            ReturnToOrigin();
            return;
        }

        if (isPlayerDetected)
        {
            ChasePlayer();
        }
        else
        {
            SetIdleAnimation();
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

        // Actualizar parámetros de animación de movimiento
        animator.SetFloat("MovimientoX", directionToPlayer.x);
        animator.SetFloat("MovimientoY", directionToPlayer.y);
    }

    private void ReturnToOrigin()
    {
        Vector2 directionToOrigin = (initialPosition - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, initialPosition);

        if (distance > 0.1f)
        {
            transform.position += (Vector3)(directionToOrigin * speed * Time.deltaTime);

            animator.SetBool("Move", true);
            animator.SetFloat("MovimientoX", directionToOrigin.x);
            animator.SetFloat("MovimientoY", directionToOrigin.y);

            HandleSound(true); // Sonido de caminar
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;

            animator.SetBool("Move", false);
            SetIdleAnimation();

            HandleSound(false); // Sonido de idle
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

    private void SetIdleAnimation()
    {
        if (playerTransform == null) return;

        Vector2 idleDirection = (playerTransform.position - transform.position).normalized;

        animator.SetFloat("IdleX", idleDirection.x);
        animator.SetFloat("IdleY", idleDirection.y);
    }

    private void HandleSound(bool isWalking)
    {
        if (isWalking && !isPlayingWalkSound)
        {
            // Cambiar a sonido de caminar
            if (idleSoundSource != null && idleSoundSource.isPlaying)
                idleSoundSource.Stop();

            if (walkSoundSource != null && !walkSoundSource.isPlaying)
                walkSoundSource.Play();

            isPlayingWalkSound = true;
        }
        else if (!isWalking && isPlayingWalkSound)
        {
            // Cambiar a sonido de idle
            if (walkSoundSource != null && walkSoundSource.isPlaying)
                walkSoundSource.Stop();

            if (idleSoundSource != null && !idleSoundSource.isPlaying)
                idleSoundSource.Play();

            isPlayingWalkSound = false;
        }
    }
}
