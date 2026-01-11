using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class EnemyShooterControllerDroopScript : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHp = 10;
    public int contactDamage = 5;
    public float detectionDistance = 10f;
    public float speed = 3f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public int bulletDamage = 10;
    public float shootInterval = 2f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Drop System (Always drops 1/3 each)")]
    public GameObject healthPickupPrefab;
    public GameObject attackReloadPrefab;
    public GameObject coinPrefab;

    [Header("Audio Clips")]
    public AudioClip idleClip;
    public AudioClip walkClip;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    private bool isReturningToOrigin = false;

    private Animator animator;
    private AudioSource audioSource;

    private bool isMoving;

    //Candado para evitar doble muerte/doble drop
    private bool hasDied = false;

    private void Start()
    {
        currentHp = maxHp;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        DetectRoomBounds();
        initialPosition = transform.position;

        StartCoroutine(ShootAtPlayer());
    }

    private void Update()
    {
        if (hasDied) return;

        if (playerTransform == null)
        {
            SetIdleAnimation();
            HandleFootstepSound();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionDistance;

        if (isReturningToOrigin)
        {
            ReturnToOrigin();
        }
        else if (isPlayerDetected)
        {
            ChasePlayer();
        }
        else
        {
            SetIdleAnimation();
        }

        HandleFootstepSound();
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
            newPosition = ClampToRoomBounds(newPosition);

        transform.position = newPosition;
        SetMovementAnimation(directionToPlayer);
    }

    private void ReturnToOrigin()
    {
        Vector2 directionToOrigin = (initialPosition - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, initialPosition);

        if (distance > 0.1f)
        {
            Vector2 newPosition = (Vector2)transform.position + directionToOrigin * speed * Time.deltaTime;

            if (roomBounds.size != Vector3.zero)
                newPosition = ClampToRoomBounds(newPosition);

            transform.position = newPosition;
            SetMovementAnimation(directionToOrigin);
        }
        else
        {
            transform.position = initialPosition;
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

    private IEnumerator ShootAtPlayer()
    {
        while (true)
        {
            if (!hasDied &&
                !isReturningToOrigin &&
                isPlayerDetected &&
                playerTransform != null &&
                bulletPrefab != null)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                    bulletScript.Initialize(directionToPlayer, bulletDamage);
                else
                    Debug.LogWarning("Bullet prefab has no Bullet script attached.");
            }

            yield return new WaitForSeconds(shootInterval);
        }
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

        if (collider.CompareTag("Pared"))
        {
            isReturningToOrigin = true;
        }
    }

    public void Damage(int damage)
    {
        if (hasDied) return;

        currentHp -= damage;
        if (currentHp <= 0)
            Die();
    }

    private void Die()
    {
        if (hasDied) return;   //evita doble ejecución
        hasDied = true;

        Debug.Log("Enemy defeated.");

        DropLoot();

        Destroy(gameObject);
    }

    private void DropLoot()
    {
        // 1/3 exacto cada uno: 0 = health, 1 = attack reload, 2 = coin
        int roll = Random.Range(0, 3);

        GameObject dropPrefab = null;
        switch (roll)
        {
            case 0: dropPrefab = healthPickupPrefab; break;
            case 1: dropPrefab = attackReloadPrefab; break;
            case 2: dropPrefab = coinPrefab; break;
        }

        // Si el prefab seleccionado está vacío, SIEMPRE usa HealthPotion (y solo ese)
        if (dropPrefab == null)
        {
            GameObject fallback = GameObject.Find("HealthPotion");

            if (fallback != null)
            {
                Instantiate(fallback, transform.position, Quaternion.identity);
                Debug.Log("Drop fallback used: HealthPotion");
            }
            else
            {
                Debug.LogWarning("Drop failed: selected prefab is null and HealthPotion was not found.");
            }
            return;
        }

        //Solo 1 Instantiate => solo 1 drop
        Instantiate(dropPrefab, transform.position, Quaternion.identity);
        Debug.Log($"Dropped: {dropPrefab.name}");
    }

    private void SetMovementAnimation(Vector2 direction)
    {
        isMoving = true;

        animator.SetFloat("MovimientoX", direction.x);
        animator.SetFloat("MovimientoY", direction.y);
        animator.SetBool("Move", true);
    }

    private void SetIdleAnimation()
    {
        isMoving = false;

        Vector2 direction = playerTransform != null
            ? (playerTransform.position - transform.position).normalized
            : Vector2.down;

        animator.SetFloat("IdleX", direction.x);
        animator.SetFloat("IdleY", direction.y);
        animator.SetBool("Move", false);
    }

    private void HandleFootstepSound()
    {
        if (isMoving)
        {
            if (walkClip != null && (!audioSource.isPlaying || audioSource.clip != walkClip))
            {
                audioSource.clip = walkClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (idleClip != null && (!audioSource.isPlaying || audioSource.clip != idleClip))
            {
                audioSource.clip = idleClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }
}
