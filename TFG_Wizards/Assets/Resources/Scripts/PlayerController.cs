using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Movement variables
    public float moveSpeed = 5f;
    private float defaultMoveSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 shootDirection;

    // Health and Damage variables
    public int maxHp = 100;
    private int currentHp;
    public float flickerInterval = 0.1f;
    public float invincibilityDuration = 1.0f;
    private bool isInvincible = false;
    private Coroutine invincibilityCoroutine;

    // Shooting variables
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    private bool canShoot = true;
    public float shootCooldown = 0.2f;
    private float defaultShootCooldown;
    private int attackDamage = 10;

    // Boost limits
    private const int maxBoostPurchases = 4;

    // Player components
    public SpriteRenderer playerSprite;
    private Animator animator;

    // UI References
    public TMP_Text healthText;
    public TMP_Text coinsText;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Load initial values from PlayerPrefs
        currentHp = PlayerPrefs.GetInt("PlayerHealth", maxHp);
        attackDamage = PlayerPrefs.GetInt("PlayerAttackDamage", attackDamage);
        shootCooldown = PlayerPrefs.GetFloat("PlayerShootCooldown", shootCooldown);
        defaultMoveSpeed = moveSpeed;
        defaultShootCooldown = shootCooldown;

        UpdateHealthUI();
        UpdateCoinsUI();
    }

    private void Update()
    {
        HandleMovement();
        HandleShooting();
        HandleExit(); // Check for the Escape key
        FlipSprite();
    }

    private void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }

    private void HandleMovement()
    {
        if (!IsShooting())
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;

            animator.SetBool("isWalking", movement != Vector2.zero);
        }
        else
        {
            movement = Vector2.zero;
            animator.SetBool("isWalking", false);
        }
    }

    private void HandleShooting()
    {
        shootDirection = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow)) shootDirection = Vector2.up;
        else if (Input.GetKey(KeyCode.DownArrow)) shootDirection = Vector2.down;
        else if (Input.GetKey(KeyCode.LeftArrow)) shootDirection = Vector2.left;
        else if (Input.GetKey(KeyCode.RightArrow)) shootDirection = Vector2.right;

        if (shootDirection != Vector2.zero && canShoot)
        {
            StartCoroutine(Shoot(shootDirection));
        }
    }

    private void HandleExit()
    {
        // Close the application when "Esc" is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Exiting the application...");
            Application.Quit();
        }
    }

    private IEnumerator Shoot(Vector2 direction)
    {
        canShoot = false;
        animator.SetBool("isAttacking", true);

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        SpellPlayerScript spell = bullet.GetComponent<SpellPlayerScript>();

        if (spell != null)
        {
            spell.Initialize(direction, attackDamage);
        }

        yield return new WaitForSeconds(shootCooldown);

        animator.SetBool("isAttacking", false);
        canShoot = true;
    }

    private void FlipSprite()
    {
        if (movement.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (movement.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private bool IsShooting()
    {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
    }

    public void Damage(int damage)
    {
        if (isInvincible || currentHp <= 0)
            return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        PlayerPrefs.SetInt("PlayerHealth", currentHp);
        PlayerPrefs.Save();
        UpdateHealthUI();

        animator.SetTrigger("isHit");

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            if (invincibilityCoroutine != null)
                StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = StartCoroutine(InvincibilityTimer());
        }
    }

    private IEnumerator InvincibilityTimer()
    {
        isInvincible = true;
        float elapsedTime = 0f;

        while (elapsedTime < invincibilityDuration)
        {
            playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSeconds(flickerInterval);
            elapsedTime += flickerInterval;
        }

        playerSprite.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player has died.");

        // Reset PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Go to GameOverScene with losing text
        SceneManager.LoadScene("GameOverScene");
    }

    public void AddCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        currentCoins += amount;
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.Save();
        UpdateCoinsUI();
    }

    public void Heal(int amount)
    {
        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        PlayerPrefs.SetInt("PlayerHealth", currentHp);
        PlayerPrefs.Save();
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = $"{currentHp}";
    }

    private void UpdateCoinsUI()
    {
        if (coinsText != null)
            coinsText.text = PlayerPrefs.GetInt("Coins", 0).ToString();
    }

    public void ReduceShootCooldown()
    {
        int speedBoosts = PlayerPrefs.GetInt("SpeedBoosts", 0);
        if (speedBoosts >= maxBoostPurchases)
        {
            Debug.Log("Maximum speed boosts reached!");
            return;
        }

        shootCooldown /= 2f;
        PlayerPrefs.SetInt("SpeedBoosts", speedBoosts + 1);
        PlayerPrefs.SetFloat("PlayerShootCooldown", shootCooldown);
        PlayerPrefs.Save();

        Debug.Log($"Shoot cooldown reduced to: {shootCooldown}");
    }

    public void DoubleDamage()
    {
        int attackBoosts = PlayerPrefs.GetInt("AttackBoosts", 0);
        if (attackBoosts >= maxBoostPurchases)
        {
            Debug.Log("Maximum attack boosts reached!");
            return;
        }

        attackDamage *= 2;
        PlayerPrefs.SetInt("AttackBoosts", attackBoosts + 1);
        PlayerPrefs.SetInt("PlayerAttackDamage", attackDamage);
        PlayerPrefs.Save();

        Debug.Log($"Attack damage doubled: {attackDamage}");
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("PlayerHealth", currentHp);
        PlayerPrefs.Save();
    }
}
