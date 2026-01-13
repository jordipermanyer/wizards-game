using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShooterRadioControllerScript : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHp = 100;
    public int contactDamage = 5;
    public float detectionDistance = 10f;
    public float speed = 3f;
    public float wanderSpeed = 1f;
    public float wanderInterval = 2f;

    [Header("Health Bar")]
    public Slider healthBar;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public int bulletDamage = 10;
    public float shootInterval = 2f;
    public float spreadAngle = 15f;

    [Header("Shield")]
    public GameObject shieldPrefab;
    public int shieldCount = 7;
    public float shieldRadius = 1f;
    public float shieldRotationSpeed = 150f;

    [Header("Explosion")]
    public int explosionBulletCount = 20;
    public float explosionDelay = 1f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Animation")]
    public Animator animator;

    [Header("Audio")]
    public AudioSource idleAudioSource;
    public AudioSource walkAudioSource;

    [Header("Drop")]
    public GameObject objetoADropear;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    private bool isReturningToOrigin = false;
    private GameObject[] shields;

    private bool isDying = false;

    private void Start()
    {
        currentHp = maxHp;
        initialPosition = transform.position;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = maxHp;
        }
        else
        {
            Debug.LogWarning("EnemyShooterRadioControllerScript: healthBar is not assigned.");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        DetectRoomBounds();

        CreateShields();
        StartCoroutine(Wander());
        StartCoroutine(ShootAtPlayer());
    }

    private void Update()
    {
        if (playerTransform == null) return;
        if (isDying) return;

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
            UpdateIdleAnimation();
        }

        RotateShields();
    }

    private void DetectRoomBounds()
    {
        Collider2D roomBoundsCollider = Physics2D.OverlapCircle(transform.position, 0.1f, roomBoundsLayer);
        if (roomBoundsCollider != null)
        {
            roomBounds = roomBoundsCollider.bounds;
            Debug.Log("Room bounds detected: " + roomBounds);
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

        if (animator != null)
        {
            animator.SetBool("Move", true);
            animator.SetFloat("MovimientoX", directionToPlayer.x);
            animator.SetFloat("MovimientoY", directionToPlayer.y);
        }

        if (walkAudioSource != null && !walkAudioSource.isPlaying)
            walkAudioSource.Play();
        if (idleAudioSource != null && idleAudioSource.isPlaying)
            idleAudioSource.Stop();
    }

    private void UpdateIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("Move", false);

            Vector2 directionToOrigin = (initialPosition - transform.position).normalized;
            animator.SetFloat("IdleX", directionToOrigin.x);
            animator.SetFloat("IdleY", directionToOrigin.y);
        }

        if (idleAudioSource != null && !idleAudioSource.isPlaying)
            idleAudioSource.Play();
        if (walkAudioSource != null && walkAudioSource.isPlaying)
            walkAudioSource.Stop();
    }

    private void ReturnToOrigin()
    {
        Vector2 directionToOrigin = (initialPosition - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, initialPosition);

        if (distance > 0.1f)
        {
            transform.position += (Vector3)(directionToOrigin * speed * Time.deltaTime);

            if (animator != null)
            {
                animator.SetBool("Move", true);
                animator.SetFloat("MovimientoX", directionToOrigin.x);
                animator.SetFloat("MovimientoY", directionToOrigin.y);
            }

            if (walkAudioSource != null && !walkAudioSource.isPlaying)
                walkAudioSource.Play();
            if (idleAudioSource != null && idleAudioSource.isPlaying)
                idleAudioSource.Stop();
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;

            UpdateIdleAnimation();
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
            if (!isDying && isPlayerDetected && playerTransform != null && bulletPrefab != null)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

                SpawnBullet(baseAngle);
                SpawnBullet(baseAngle + spreadAngle);
                SpawnBullet(baseAngle - spreadAngle);
            }

            yield return new WaitForSeconds(shootInterval);
        }
    }

    private void SpawnBullet(float angle)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject bullet = Instantiate(bulletPrefab, transform.position, rotation);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
        {
            b.Initialize(rotation * Vector2.right, bulletDamage);
        }
    }

    private void CreateShields()
    {
        shields = new GameObject[shieldCount];

        for (int i = 0; i < shieldCount; i++)
        {
            GameObject shield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);
            shield.transform.parent = transform;

            Shield shieldScript = shield.GetComponent<Shield>();
            if (shieldScript != null)
            {
                shieldScript.Setup(this, 1);
            }

            shields[i] = shield;
        }
    }

    private void RotateShields()
    {
        if (shields == null) return;

        for (int i = 0; i < shields.Length; i++)
        {
            if (shields[i] == null) continue;

            float angle = (360f / shieldCount) * i + Time.time * shieldRotationSpeed;
            Vector2 shieldPosition = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * shieldRadius;

            shields[i].transform.localPosition = shieldPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (isDying) return;

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
        if (isDying) return;

        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;

        if (healthBar != null)
            healthBar.value = currentHp;

        if (currentHp <= 0)
        {
            isDying = true;
            StartCoroutine(DieWithExplosion());
        }
    }

    private IEnumerator DieWithExplosion()
    {
        Debug.Log("Enemy defeated.");

        Explode();
        yield return new WaitForSeconds(explosionDelay);
        Explode();

        if (objetoADropear != null)
        {
            Instantiate(objetoADropear, transform.position, Quaternion.identity);
            Debug.Log("Drop spawned at: " + transform.position);
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        if (bulletPrefab == null) return;

        for (int i = 0; i < explosionBulletCount; i++)
        {
            float angle = (360f / explosionBulletCount) * i;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject bullet = Instantiate(bulletPrefab, transform.position, rotation);

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null)
            {
                b.Initialize(rotation * Vector2.right, bulletDamage);
            }
        }
    }
}
