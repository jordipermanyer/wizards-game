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

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Drop Item")]
    public GameObject dropPrefab;

    [Header("Audio")]
    public AudioClip idleClip;
    public AudioClip moveClip;
    private AudioSource audioSource;
    private bool isMoving = false; // Controlar el canvi de so

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;
    private float enemySpawnInterval = 5f;
    private float enemyPrefab1Chance = 0.7f;

    private Animator animator;
    private Vector2 lastMoveDirection = Vector2.down;

    private void Start()
    {
        currentHp = maxHp;
        healthBar.maxValue = maxHp;
        healthBar.value = maxHp;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        DetectRoomBounds();

        StartCoroutine(ShootAtPlayer());
        StartCoroutine(SpawnEnemies());
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        animator.SetBool("Move", isPlayerDetected);

        // Canvi de so si canvia estat a Move
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
            animator.SetFloat("IdleX", lastMoveDirection.x);
            animator.SetFloat("IdleY", lastMoveDirection.y);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
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
            Debug.Log($"Room bounds detected: {roomBounds}");
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

        animator.SetFloat("MovimientoX", directionToPlayer.x);
        animator.SetFloat("MovimientoY", directionToPlayer.y);

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
            if (isPlayerDetected && playerTransform != null && bulletPrefab != null)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<Bullet>().Initialize(directionToPlayer, bulletDamage);
            }
            yield return new WaitForSeconds(shootInterval);
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (currentHp > 0)
        {
            if (currentHp <= 750) enemySpawnInterval = 5f;
            if (currentHp <= 500) enemySpawnInterval = 2f;
            if (currentHp <= 350) enemySpawnInterval = 2f;

            float spawnChance = Random.value;
            GameObject enemyToSpawn = spawnChance <= enemyPrefab1Chance ? enemyPrefab1 : enemyPrefab2;
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(enemyToSpawn, spawnPoint.position, Quaternion.identity);

            if (currentHp <= 500) enemyPrefab1Chance = 0.5f;
            if (currentHp <= 350)
            {
                enemyPrefab1Chance = 0.3f;
                StartCoroutine(ShootInAllDirections());
            }

            yield return new WaitForSeconds(enemySpawnInterval);
        }
    }

    private IEnumerator ShootInAllDirections()
    {
        while (currentHp <= 350 && currentHp > 0)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<Bullet>().Initialize(direction, bulletDamage);
            }
            yield return new WaitForSeconds(1f);
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
        healthBar.value = currentHp;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Boss defeated.");

        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
