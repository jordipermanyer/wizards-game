using UnityEngine;
using System.Collections;

public class EnemyDragonScript : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int baseHp = 200;
    public int currentHp;
    public int maxHp;
    public int regenStage = 0;

    public int contactDamage = 5;
    public float detectionDistance = 10f;
    public float speed = 3f;

    [Header("Flamethrower Attack")]
    public GameObject flamePrefab;
    public float flameDuration = 0.5f;
    public float flameRate = 0.1f;

    [Header("Regeneration Settings")]
    public float regenDuration = 3f;
    public SpriteRenderer targetSpriteRenderer;
    public Color flashColor = Color.red;
    public float flashDuration = 0.2f;

    [Header("Enemy Spawn During Regeneration")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public GameObject[] spawnableEnemies;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    private Transform playerTransform;
    private bool isPlayerDetected = false;
    private bool isRegenerating = false;
    private bool isIntangible = false;
    private bool canReenterRegen = true;

    private bool isReturningToOrigin = false;
    private Vector3 initialPosition;
    private Bounds roomBounds;

    private Coroutine regenCoroutine;
    private Coroutine flashCoroutine;
    private Color originalColor;

    private Animator animator; // <- Referencia al Animator

    private void Start()
    {
        maxHp = baseHp;
        currentHp = maxHp;
        initialPosition = transform.position;

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
    }

    private void Update()
    {
        if (isRegenerating || playerTransform == null) return;

        if (isReturningToOrigin)
        {
            ReturnToOrigin();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        animator.SetBool("Move", isPlayerDetected); // <- Cambia la animación base según si detecta o no

        if (isPlayerDetected)
            ChasePlayer();
        else
            SetIdleAnimation(); // <- Actualiza el idle hacia el jugador
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
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + direction * speed * Time.deltaTime;

        if (roomBounds.size != Vector3.zero)
        {
            newPosition = ClampToRoomBounds(newPosition);
        }

        transform.position = newPosition;

        // Animación de movimiento
        animator.SetFloat("MovimientoX", direction.x);
        animator.SetFloat("MovimientoY", direction.y);
    }

    private void SetIdleAnimation()
    {
        if (playerTransform == null) return;

        Vector2 idleDirection = (playerTransform.position - transform.position).normalized;

        animator.SetFloat("idleX", idleDirection.x);
        animator.SetFloat("idleY", idleDirection.y);
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
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;
            animator.SetBool("Move", false);
            SetIdleAnimation();
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
            if (isPlayerDetected && !isRegenerating)
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

    private void EnterRegenMode()
    {
        isRegenerating = true;
        isIntangible = true;
        canReenterRegen = false;

        animator.SetBool("Move", false); // <- Detener animación de movimiento

        if (targetSpriteRenderer != null)
        {
            Color c = targetSpriteRenderer.color;
            c.a = 0.4f;
            targetSpriteRenderer.color = c;
        }

        SpawnEnemies();
        regenCoroutine = StartCoroutine(RegenerateHealth());
    }

    private void SpawnEnemies()
    {
        if (spawnableEnemies.Length == 0) return;

        Instantiate(spawnableEnemies[Random.Range(0, spawnableEnemies.Length)], spawnPoint1.position, Quaternion.identity);
        Instantiate(spawnableEnemies[Random.Range(0, spawnableEnemies.Length)], spawnPoint2.position, Quaternion.identity);
    }

    private IEnumerator RegenerateHealth()
    {
        float elapsed = 0f;

        float regenPercent = GetRegenPercent(regenStage);
        int healAmount = Mathf.RoundToInt(maxHp * regenPercent);
        int startHp = currentHp;
        int targetHp = Mathf.Min(currentHp + healAmount, maxHp);

        while (elapsed < regenDuration)
        {
            if (!isRegenerating) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / regenDuration);
            currentHp = Mathf.RoundToInt(Mathf.Lerp(startHp, targetHp, t));
            yield return null;
        }

        regenStage++;
        ExitRegenMode();
    }

    private float GetRegenPercent(int stage)
    {
        if (stage == 0) return 1f;
        if (stage == 1) return 0.75f;
        if (stage == 2) return 0.5f;
        if (stage == 3) return 0.45f;
        return 0.4f;
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

        float cooldown = regenStage * 0.5f;
        StartCoroutine(EnableRegenAfterDelay(cooldown));
    }

    private IEnumerator EnableRegenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canReenterRegen = true;

        if (currentHp <= maxHp * 0.75f && !isRegenerating)
        {
            EnterRegenMode();
        }
    }

    public void Damage(int damage)
    {
        if (isIntangible) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
        else if (!isRegenerating && currentHp <= maxHp * 0.75f && canReenterRegen)
        {
            EnterRegenMode();
        }
    }

    private void Die()
    {
        Debug.Log("EnemyDragon defeated.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
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

