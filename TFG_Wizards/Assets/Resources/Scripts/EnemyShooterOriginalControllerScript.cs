using System.Collections;
using UnityEngine;

public class EnemyShooterOriginalControllerScript : MonoBehaviour, ICloneBossUnit
{
    [Header("Enemy Stats")]
    public int maxHp = 40;
    public int contactDamage = 5;
    public float detectionDistance = 10f;
    public float speed = 2f;
    public float wanderSpeed = 1f;
    public float wanderInterval = 2f;

    [Header("Boss Manager")]
    public GestorClones gestorClones;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public int bulletDamage = 10;
    public float shootInterval = 2f;
    public float bulletOffset = 0.5f;

    [Header("Auto-detection")]
    public LayerMask roomBoundsLayer;

    [Header("Objects to Activate on Death")]
    public GameObject objectToActivate1;
    public GameObject objectToActivate2;

    [Header("Sound Settings")]
    public AudioClip idleSound;
    public AudioClip walkingSound;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected;
    private Bounds roomBounds;

    private Vector3 initialPosition;
    private bool isReturningToOrigin = false;

    private Animator animator;
    private Vector2 lastMoveDirection = Vector2.down;

    private AudioSource audioSource;
    private bool wasMovingLastFrame = false;

    private bool hasDied = false;

    public int CurrentHp { get { return currentHp; } }
    public int MaxHp { get { return maxHp; } }
    public bool IsDestroyed { get { return this == null || gameObject == null; } }
    public Transform TransformRef { get { return transform; } }

    private void Start()
    {
        currentHp = maxHp;
        initialPosition = transform.position;

        if (gestorClones == null)
            gestorClones = FindObjectOfType<GestorClones>();

        if (gestorClones != null)
            gestorClones.RegisterClone(this);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
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
            animator.SetBool("Move", true);
            PlayMovementSound(true);
            ReturnToOrigin();
            return;
        }

        animator.SetBool("Move", isPlayerDetected);
        PlayMovementSound(isPlayerDetected);

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

    private void PlayMovementSound(bool isMoving)
    {
        if (isMoving != wasMovingLastFrame)
        {
            wasMovingLastFrame = isMoving;

            if (isMoving)
            {
                if (walkingSound != null)
                {
                    audioSource.clip = walkingSound;
                    audioSource.Play();
                }
            }
            else
            {
                if (idleSound != null)
                {
                    audioSource.clip = idleSound;
                    audioSource.Play();
                }
            }
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
            Debug.LogWarning("Room bounds not detected. The enemy might leave the intended area.");
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

    private void ReturnToOrigin()
    {
        Vector2 directionToOrigin = (initialPosition - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, initialPosition);

        animator.SetFloat("MovimientoX", directionToOrigin.x);
        animator.SetFloat("MovimientoY", directionToOrigin.y);

        if (directionToOrigin != Vector2.zero)
        {
            lastMoveDirection = directionToOrigin;
        }

        if (distance > 0.1f)
        {
            transform.position += (Vector3)(directionToOrigin * speed * Time.deltaTime);
        }
        else
        {
            transform.position = initialPosition;
            isReturningToOrigin = false;

            animator.SetBool("Move", false);
            PlayMovementSound(false);
            animator.SetFloat("IdleX", lastMoveDirection.x);
            animator.SetFloat("IdleY", lastMoveDirection.y);
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
                Vector2 perpendicular = new Vector2(-directionToPlayer.y, directionToPlayer.x) * bulletOffset;

                GameObject bullet1 = Instantiate(bulletPrefab, (Vector2)transform.position + perpendicular, Quaternion.identity);
                bullet1.GetComponent<Bullet>().Initialize(directionToPlayer, bulletDamage);

                GameObject bullet2 = Instantiate(bulletPrefab, (Vector2)transform.position - perpendicular, Quaternion.identity);
                bullet2.GetComponent<Bullet>().Initialize(directionToPlayer, bulletDamage);
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
        if (hasDied) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
    }

    private void Die()
    {
        if (hasDied) return;
        hasDied = true;

        Vector3 spawnPosition = transform.position;
        float offset = 0.5f;

        ActivateAndRegisterChild(objectToActivate1, spawnPosition + Vector3.left * offset);
        ActivateAndRegisterChild(objectToActivate2, spawnPosition + Vector3.right * offset);

        Destroy(gameObject);
    }

    private void ActivateAndRegisterChild(GameObject childObj, Vector3 pos)
    {
        if (childObj == null) return;

        childObj.transform.position = pos;
        childObj.SetActive(true);

        // Child script name must stay EnemyShooterCloneControllerScript
        EnemyShooterCloneControllerScript child = childObj.GetComponent<EnemyShooterCloneControllerScript>();
        if (child != null)
        {
            if (child.gestorClones == null)
                child.gestorClones = gestorClones;

            if (gestorClones != null)
                gestorClones.RegisterClone(child);

            return;
        }

        Debug.LogWarning("Child object missing EnemyShooterCloneControllerScript.");
    }
}
