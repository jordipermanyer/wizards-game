using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHp = 1000;
    public int contactDamage = 10;
    public float detectionDistance = 15f;
    public float speed = 2f;
    public Slider healthBar;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public int bulletDamage = 15;
    public float shootInterval = 1.5f;

    [Header("Enemy Spawning")]
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;
    public Transform[] spawnPoints;

    [Tooltip("Spawn after X percent damage accumulated since last spawn. 5 = 5 percent.")]
    public int spawnStepPercent = 5;

    [Tooltip("Minimum seconds between spawns.")]
    public float spawnCooldownSeconds = 5f;

    [Tooltip("Chance to spawn enemyPrefab1 (0..1). Example: 0.3 = 30 percent.")]
    [Range(0f, 1f)]
    public float enemyPrefab1Chance = 0.3f;

    [Tooltip("Chance to spawn enemyPrefab2 (0..1). Example: 0.6 = 60 percent.")]
    [Range(0f, 1f)]
    public float enemyPrefab2Chance = 0.6f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Audio")]
    public AudioClip idleClip;
    public AudioClip moveClip;
    private AudioSource audioSource;
    private bool isMoving = false;

    [Header("UI")]
    public GameObject panelJefes;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;

    private Animator animator;
    private Vector2 lastMoveDirection = Vector2.down;

    // Spawn control
    private float lastSpawnTime = -999f;
    private int damageAccumulatedSinceLastSpawn = 0;

    private void Start()
    {
        currentHp = maxHp;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = maxHp;
        }

        if (spawnStepPercent <= 0) spawnStepPercent = 5;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        DetectRoomBounds();

        StartCoroutine(ShootAtPlayer());
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        if (animator != null)
            animator.SetBool("Move", isPlayerDetected);

        if (isPlayerDetected && !isMoving)
        {
            isMoving = true;
            PlaySound(moveClip);
        }
        else if (!isPlayerDetected && isMoving)
        {
            isMoving = false;
            PlaySound(idleClip);
        }

        if (isPlayerDetected)
        {
            ChasePlayer();
        }
        else
        {
            if (animator != null)
            {
                animator.SetFloat("IdleX", lastMoveDirection.x);
                animator.SetFloat("IdleY", lastMoveDirection.y);
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void DetectRoomBounds()
    {
        Collider2D roomBoundsCollider = Physics2D.OverlapCircle(transform.position, 0.1f, roomBoundsLayer);
        if (roomBoundsCollider != null)
        {
            roomBounds = roomBoundsCollider.bounds;
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

        if (animator != null)
        {
            animator.SetFloat("MovimientoX", directionToPlayer.x);
            animator.SetFloat("MovimientoY", directionToPlayer.y);
        }

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
            if (isPlayerDetected && playerTransform != null && bulletPrefab != null && currentHp > 0)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                Bullet b = bullet.GetComponent<Bullet>();
                if (b != null)
                {
                    b.Initialize(directionToPlayer, bulletDamage);
                }
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
        if (currentHp <= 0) return;
        if (damage <= 0) return;

        int hpBefore = currentHp;

        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;

        if (healthBar != null)
            healthBar.value = currentHp;

        // Accumulate real applied damage (clamped by hpBefore)
        int appliedDamage = Mathf.Min(damage, hpBefore);
        damageAccumulatedSinceLastSpawn += appliedDamage;

        // Step is based on current HP AFTER the hit (as requested)
        TrySpawnByDamageStep(currentHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void TrySpawnByDamageStep(int hpReferenceForStep)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (enemyPrefab1 == null && enemyPrefab2 == null) return;

        int stepDamage = Mathf.RoundToInt(hpReferenceForStep * (spawnStepPercent / 100f));
        if (stepDamage < 1) stepDamage = 1;

        bool cooldownReady = (Time.time - lastSpawnTime) >= spawnCooldownSeconds;
        if (!cooldownReady) return;

        if (damageAccumulatedSinceLastSpawn < stepDamage) return;

        // Spawn 1 enemy per spawn point (3 points -> 3 enemies)
        SpawnEnemiesAtAllSpawnPoints();

        lastSpawnTime = Time.time;

        damageAccumulatedSinceLastSpawn -= stepDamage;
        if (damageAccumulatedSinceLastSpawn < 0) damageAccumulatedSinceLastSpawn = 0;
    }

    private void SpawnEnemiesAtAllSpawnPoints()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform sp = spawnPoints[i];
            if (sp == null) continue;

            GameObject chosen = ChooseEnemyPrefab();
            if (chosen == null) continue;

            Instantiate(chosen, sp.position, Quaternion.identity);
        }
    }

    private GameObject ChooseEnemyPrefab()
    {
        // If one is missing, use the other
        if (enemyPrefab1 == null && enemyPrefab2 == null) return null;
        if (enemyPrefab1 == null) return enemyPrefab2;
        if (enemyPrefab2 == null) return enemyPrefab1;

        float r = Random.value;

        // 30% prefab1
        if (r < enemyPrefab1Chance) return enemyPrefab1;

        // next 60% prefab2
        if (r < enemyPrefab1Chance + enemyPrefab2Chance) return enemyPrefab2;

        // remaining 10% -> no spawn
        return null;
    }


    private void Die()
    {
        Debug.Log("Boss defeated.");

        // Disable boss UI panel
        if (panelJefes != null)
        {
            panelJefes.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Boss: panelJefes not assigned.");
        }

        Destroy(gameObject);
    }
}
