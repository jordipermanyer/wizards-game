using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyDragonScript : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int baseHp = 200;
    public int currentHp;
    public int maxHp;
    public int regenStage = 0; // Kept for compatibility (not used now)

    public int contactDamage = 5;
    public float detectionDistance = 10f;
    public float speed = 3f;

    [Header("Health Bar")]
    public Slider healthBar;

    [Header("Flamethrower Attack")]
    public GameObject flamePrefab;
    public float flameDuration = 0.5f;
    public float flameRate = 0.1f;

    [Header("Regeneration Settings")]
    public float regenDuration = 3f;          // Must be 3 seconds (your request)
    public float regenInterval = 5f;          // Every 5 seconds (your request)
    [Range(0f, 1f)]
    public float regenHealFraction = 0.5f;    // Heal 50 percent of damage received
    public SpriteRenderer targetSpriteRenderer;
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;

    [Header("Enemy Spawn During Regeneration")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public GameObject[] spawnableEnemies;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Sounds")]
    public AudioSource idleSoundSource;
    public AudioSource walkSoundSource;

    [Header("Drop Settings")]
    public GameObject dropPrefab;

    private Transform playerTransform;
    private bool isPlayerDetected = false;
    private bool isRegenerating = false;
    private bool isIntangible = false;

    private bool isReturningToOrigin = false;
    private Vector3 initialPosition;
    private Bounds roomBounds;

    private Coroutine regenCoroutine;
    private Color originalColor;

    private Animator animator;

    private bool isPlayingWalkSound = false;
    private bool hasDied = false;

    // Tracks damage taken since last regeneration
    private int damageTakenSinceLastRegen = 0;

    private void Start()
    {
        maxHp = baseHp;
        currentHp = maxHp;
        initialPosition = transform.position;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = currentHp;
        }
        else
        {
            Debug.LogWarning("EnemyDragonScript: healthBar is not assigned.");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (targetSpriteRenderer == null)
            targetSpriteRenderer = GetComponent<SpriteRenderer>();

        if (targetSpriteRenderer != null)
            originalColor = targetSpriteRenderer.color;

        animator = GetComponent<Animator>();

        DetectRoomBounds();

        StartCoroutine(FlamethrowerAttack());

        // New regen behavior: periodic check every regenInterval seconds
        StartCoroutine(RegenerationLoop());
    }

    private void Update()
    {
        if (hasDied) return;
        if (isRegenerating || playerTransform == null) return;

        if (isReturningToOrigin)
        {
            ReturnToOrigin();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        if (animator != null)
            animator.SetBool("Move", isPlayerDetected);

        HandleSound(isPlayerDetected);

        if (isPlayerDetected)
            ChasePlayer();
        else
            SetIdleAnimation();
    }

    private void HandleSound(bool isWalking)
    {
        if (isWalking && !isPlayingWalkSound)
        {
            if (idleSoundSource != null && idleSoundSource.isPlaying)
                idleSoundSource.Stop();

            if (walkSoundSource != null && !walkSoundSource.isPlaying)
                walkSoundSource.Play();

            isPlayingWalkSound = true;
        }
        else if (!isWalking && isPlayingWalkSound)
        {
            if (walkSoundSource != null && walkSoundSource.isPlaying)
                walkSoundSource.Stop();

            if (idleSoundSource != null && !idleSoundSource.isPlaying)
                idleSoundSource.Play();

            isPlayingWalkSound = false;
        }
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
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + direction * speed * Time.deltaTime;

        if (roomBounds.size != Vector3.zero)
        {
            newPosition = ClampToRoomBounds(newPosition);
        }

        transform.position = newPosition;

        if (animator != null)
        {
            animator.SetFloat("MovimientoX", direction.x);
            animator.SetFloat("MovimientoY", direction.y);
        }
    }

    private void SetIdleAnimation()
    {
        if (playerTransform == null) return;

        Vector2 idleDirection = (playerTransform.position - transform.position).normalized;

        if (animator != null)
        {
            animator.SetFloat("IdleX", idleDirection.x);
            animator.SetFloat("IdleY", idleDirection.y);
        }
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

            HandleSound(true);
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;

            if (animator != null)
                animator.SetBool("Move", false);

            SetIdleAnimation();
            HandleSound(false);
        }
    }

    private Vector2 ClampToRoomBounds(Vector2 position)
    {
        position.x = Mathf.Clamp(position.x, roomBounds.min.x, roomBounds.max.x);
        position.y = Mathf.Clamp(position.y, roomBounds.min.y, roomBounds.max.y);
        return position;
    }

    private IEnumerator FlamethrowerAttack()
    {
        while (true)
        {
            if (!hasDied && isPlayerDetected && !isRegenerating)
            {
                GameObject flame = Instantiate(flamePrefab, transform.position, Quaternion.identity);
                Vector2 direction = (playerTransform.position - transform.position).normalized;

                Bullet bullet = flame.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.Initialize(direction, 10);
                }

                Destroy(flame, flameDuration);
            }

            yield return new WaitForSeconds(flameRate);
        }
    }

    // New: every regenInterval seconds, try to regen based on damage taken
    private IEnumerator RegenerationLoop()
    {
        while (!hasDied)
        {
            yield return new WaitForSeconds(regenInterval);

            if (hasDied) yield break;
            if (isRegenerating) continue;

            // Only regen if took damage since last regen and not at full health
            if (damageTakenSinceLastRegen <= 0) continue;
            if (currentHp >= maxHp) { damageTakenSinceLastRegen = 0; continue; }

            EnterRegenMode();
        }
    }

    private void EnterRegenMode()
    {
        isRegenerating = true;
        isIntangible = true;

        if (animator != null)
            animator.SetBool("Move", false);

        HandleSound(false);

        if (targetSpriteRenderer != null)
        {
            Color c = targetSpriteRenderer.color;
            c.a = 0.4f;
            targetSpriteRenderer.color = c;
        }

        SpawnEnemies();

        if (regenCoroutine != null)
            StopCoroutine(regenCoroutine);

        regenCoroutine = StartCoroutine(RegenerateHealth());
    }

    private void SpawnEnemies()
    {
        if (spawnableEnemies == null) return;
        if (spawnableEnemies.Length == 0) return;
        if (spawnPoint1 == null || spawnPoint2 == null) return;

        Instantiate(spawnableEnemies[Random.Range(0, spawnableEnemies.Length)], spawnPoint1.position, Quaternion.identity);
        Instantiate(spawnableEnemies[Random.Range(0, spawnableEnemies.Length)], spawnPoint2.position, Quaternion.identity);
    }

    private IEnumerator RegenerateHealth()
    {
        float elapsed = 0f;

        // Heal only 50 percent of damage received since last regen
        int healAmount = Mathf.RoundToInt(damageTakenSinceLastRegen * regenHealFraction);
        if (healAmount < 0) healAmount = 0;

        int startHp = currentHp;
        int targetHp = Mathf.Min(currentHp + healAmount, maxHp);

        // If nothing to heal, just exit quickly
        if (targetHp <= startHp)
        {
            damageTakenSinceLastRegen = 0;
            ExitRegenMode();
            yield break;
        }

        while (elapsed < regenDuration)
        {
            if (!isRegenerating) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / regenDuration);
            currentHp = Mathf.RoundToInt(Mathf.Lerp(startHp, targetHp, t));

            if (healthBar != null)
                healthBar.value = currentHp;

            yield return null;
        }

        // Reset damage buffer after regen
        damageTakenSinceLastRegen = 0;

        ExitRegenMode();
    }

    private void ExitRegenMode()
    {
        isRegenerating = false;
        isIntangible = false;

        if (targetSpriteRenderer != null)
        {
            Color c = targetSpriteRenderer.color;
            c.a = 1f;
            targetSpriteRenderer.color = c;
        }
    }

    public void Damage(int damage)
    {
        if (hasDied) return;
        if (isIntangible) return;
        if (damage <= 0) return;

        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;

        // Track damage since last regeneration
        damageTakenSinceLastRegen += damage;

        if (healthBar != null)
            healthBar.value = currentHp;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (hasDied) return;
        hasDied = true;

        Debug.Log("EnemyDragon defeated.");

        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (hasDied) return;

        if (collider.CompareTag("Player"))
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
                player.Damage(contactDamage);
        }
        else if (collider.CompareTag("Pared"))
        {
            isReturningToOrigin = true;
        }
    }
}
