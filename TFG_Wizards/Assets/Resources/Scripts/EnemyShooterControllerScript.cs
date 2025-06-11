using System.Collections;
using UnityEngine;

public class EnemyShooterControllerScript : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHp = 10;
    public int contactDamage = 5;
    public float detectionDistance = 10f;
    public float speed = 3f;
    public float wanderSpeed = 1f;
    public float wanderInterval = 2f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public int bulletDamage = 10;
    public float shootInterval = 2f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkingClip;
    public AudioClip idleClip;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    private bool isReturningToOrigin = false;

    private Animator animator;
    private bool isMoving = false;
    private AudioClip lastPlayedClip;

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
        audioSource = GetComponent<AudioSource>();

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
        }
        else
        {
            UpdateAnimation(Vector2.zero);
        }

        HandleSound();
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

        UpdateAnimation(directionToPlayer);
    }

    private void ReturnToOrigin()
    {
        Vector2 directionToOrigin = (initialPosition - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, initialPosition);

        if (distance > 0.1f)
        {
            transform.position += (Vector3)(directionToOrigin * speed * Time.deltaTime);
            UpdateAnimation(directionToOrigin);
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;
            UpdateAnimation(Vector2.zero);
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
        Destroy(gameObject);
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (animator == null) return;

        if (direction != Vector2.zero)
        {
            animator.SetFloat("MovimientoX", direction.x);
            animator.SetFloat("MovimientoY", direction.y);
            animator.SetBool("Move", true);
            isMoving = true;
        }
        else
        {
            animator.SetFloat("IdleX", animator.GetFloat("MovimientoX"));
            animator.SetFloat("IdleY", animator.GetFloat("MovimientoY"));
            animator.SetBool("Move", false);
            isMoving = false;
        }
    }

    private void HandleSound()
    {
        if (audioSource == null || (walkingClip == null && idleClip == null)) return;

        AudioClip targetClip = isMoving ? walkingClip : idleClip;

        if (audioSource.clip != targetClip)
        {
            audioSource.clip = targetClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
