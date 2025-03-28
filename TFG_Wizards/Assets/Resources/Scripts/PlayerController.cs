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

    // Health and Damage variables
    public int maxHp = 1000;
    private int currentHp;
    public float flickerInterval = 0.1f;
    public float invincibilityDuration = 1.0f;
    private bool isInvincible = false;
    private Coroutine invincibilityCoroutine;

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
        defaultMoveSpeed = moveSpeed;

        UpdateHealthUI();
        UpdateCoinsUI();
    }

    private void Update()
    {
        HandleMovement();
        HandleExit();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }

    private void HandleMovement()
    {
        // Reiniciar el movimiento
        movement = Vector2.zero;

        // Detectar entrada solo de WASD
        if (Input.GetKey(KeyCode.W))
            movement.y = 1;
        if (Input.GetKey(KeyCode.S))
            movement.y = -1;
        if (Input.GetKey(KeyCode.A))
            movement.x = -1;
        if (Input.GetKey(KeyCode.D))
            movement.x = 1;

        // Normalizar el movimiento para evitar velocidad diagonal mayor
        movement = movement.normalized;

        // Activar la animación si el personaje se mueve
        animator.SetBool("isWalking", movement != Vector2.zero);
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

        PlayerPrefs.SetInt("SpeedBoosts", speedBoosts + 1);
        PlayerPrefs.Save();

        Debug.Log("Shoot cooldown reduced.");
    }

    public void DoubleDamage()
    {
        int attackBoosts = PlayerPrefs.GetInt("AttackBoosts", 0);
        if (attackBoosts >= maxBoostPurchases)
        {
            Debug.Log("Maximum attack boosts reached!");
            return;
        }

        PlayerPrefs.SetInt("AttackBoosts", attackBoosts + 1);
        PlayerPrefs.Save();

        Debug.Log("Attack damage doubled.");
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("PlayerHealth", currentHp);
        PlayerPrefs.Save();
    }
}
