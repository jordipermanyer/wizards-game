using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class EnemyShooterControllerTeleportScript : MonoBehaviour
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

    [Header("Drop Item")]
    public GameObject teleportItemPrefab;

    [Header("Audio")]
    public AudioClip idleSound;
    public AudioClip walkSound;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    public bool isReturningToOrigin = false;

    private Animator animator;
    private AudioSource audioSource;

    private void Start()
    {
        currentHp = maxHp;
        initialPosition = transform.position;

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

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
            MoveTo(initialPosition);
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
        SetMoveAnimation(directionToPlayer);
    }

    private void MoveTo(Vector3 target)
    {
        Vector2 direction = (target - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, target);

        if (distance > 0.1f)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            SetMoveAnimation(direction);
        }
        else
        {
            transform.position = target;
            isReturningToOrigin = false;
            SetIdleAnimation();
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

    private void SetMoveAnimation(Vector2 direction)
    {
        animator.SetBool("Move", true);
        animator.SetFloat("MovimientoX", direction.x);
        animator.SetFloat("MovimientoY", direction.y);

        if (audioSource.clip != walkSound || !audioSource.isPlaying)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void SetIdleAnimation()
    {
        animator.SetBool("Move", false);

        // Usa la última dirección del movimiento como idleX/idleY
        float lastX = animator.GetFloat("MovimientoX");
        float lastY = animator.GetFloat("MovimientoY");
        animator.SetFloat("IdleX", lastX);
        animator.SetFloat("IdleY", lastY);

        if (audioSource.clip != idleSound || !audioSource.isPlaying)
        {
            audioSource.clip = idleSound;
            audioSource.loop = true;
            audioSource.Play();
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
        Debug.Log("Enemy defeated.");

        if (teleportItemPrefab != null)
        {
            Instantiate(teleportItemPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Teleport item prefab is not assigned!");
        }

        Destroy(gameObject);
    }
}
