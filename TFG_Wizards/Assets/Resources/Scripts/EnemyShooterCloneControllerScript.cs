using System.Collections;
using UnityEngine;

public class EnemyShooterCloneControllerScript : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHp = 80;
    public int contactDamage = 5;
    public float detectionDistance = 10f;
    public float speed = 3f;
    public float wanderSpeed = 1f;
    public float wanderInterval = 2f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public int bulletDamage = 10;
    public float shootInterval = 2f;

    [Header("Explosion")]
    public int explosionProjectileCount = 8;
    public float explosionProjectileSpeed = 5f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Audio")]
    public AudioSource audioSourceIdle;
    public AudioSource audioSourceWalking;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    private bool isReturningToOrigin = false;

    private Animator animator;

    private void Start()
    {
        currentHp = maxHp;
        initialPosition = transform.position;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        animator = GetComponent<Animator>();

        DetectRoomBounds();

        StartCoroutine(Wander());
        StartCoroutine(ShootAtPlayer());
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
            UpdateAnimation(playerTransform.position - transform.position);
            PlayWalkingSound();
        }
        else
        {
            animator.SetBool("Move", false);
            PlayIdleSound();
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
            UpdateAnimation(initialPosition - transform.position);
            PlayWalkingSound();
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;
            animator.SetBool("Move", false);
            PlayIdleSound();
        }
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
        Debug.Log("Enemy defeated. Exploding!");

        Explode();

        Destroy(gameObject);
    }

    private void Explode()
    {
        if (bulletPrefab == null) return;

        for (int i = 0; i < explosionProjectileCount; i++)
        {
            float angle = i * (360f / explosionProjectileCount);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().Initialize(direction, bulletDamage);
        }
    }

    private void UpdateAnimation(Vector3 movementDirection)
    {
        Vector2 movementNormalized = movementDirection.normalized;

        if (isPlayerDetected || isReturningToOrigin)
        {
            animator.SetBool("Move", true);
            animator.SetFloat("MovimientoX", movementNormalized.x);
            animator.SetFloat("MovimientoY", movementNormalized.y);
        }
        else
        {
            animator.SetBool("Move", false);
            animator.SetFloat("idleX", movementNormalized.x);
            animator.SetFloat("idleY", movementNormalized.y);
        }
    }

    private void PlayIdleSound()
    {
        if (!audioSourceIdle.isPlaying)
        {
            audioSourceIdle.Play();
        }
        if (audioSourceWalking.isPlaying)
        {
            audioSourceWalking.Stop();
        }
    }

    private void PlayWalkingSound()
    {
        if (!audioSourceWalking.isPlaying)
        {
            audioSourceWalking.Play();
        }
        if (audioSourceIdle.isPlaying)
        {
            audioSourceIdle.Stop();
        }
    }
}

