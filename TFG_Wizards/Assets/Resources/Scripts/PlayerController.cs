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
    public int maxHp = 250;
    public int startingHp = 100;

    private int currentHp;
    public float invincibilityDuration = 1.0f;
    private bool isInvincible = false;
    private Coroutine invincibilityCoroutine;

    // Damage over time
    private Coroutine damageOverTimeCoroutine;

    // Boost limits
    private const int maxBoostPurchases = 4;
    public int maxCoins = 99;

    // Player components
    public SpriteRenderer playerSprite;
    private Animator animator;

    // UI References
    public TMP_Text healthText;
    public TMP_Text coinsText;

    // Audio
    public AudioClip idleClip;
    public AudioClip walkClip;
    public AudioClip hitClip;
    private AudioSource audioSource;
    private bool wasWalking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        defaultMoveSpeed = moveSpeed;

        // Load initial values from PlayerPrefs
        if (!PlayerPrefs.HasKey("PlayerHealth"))
        {
            currentHp = Mathf.Clamp(startingHp, 0, maxHp);
            PlayerPrefs.SetInt("PlayerHealth", currentHp);
            PlayerPrefs.Save();
        }
        else
        {
            currentHp = PlayerPrefs.GetInt("PlayerHealth", maxHp);
            currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        }

        UpdateHealthUI();
        UpdateCoinsUI();
    }


    private void Update()
    {
        HandleMovement();
        HandleExit();
        FlipSprite();
        HandleFootsteps();

        // Developer shortcut: press 0 to set 1,000,000 HP
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            maxHp = 1000000;
            currentHp = 1000000;

            PlayerPrefs.SetInt("PlayerHealth", currentHp);
            PlayerPrefs.Save();

            UpdateHealthUI();
            Debug.Log("Developer shortcut: HP set to 1,000,000.");
        }
    }


    private void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }

    private void HandleMovement()
    {
        movement = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
            movement.y = 1;
        if (Input.GetKey(KeyCode.S))
            movement.y = -1;
        if (Input.GetKey(KeyCode.A))
            movement.x = -1;
        if (Input.GetKey(KeyCode.D))
            movement.x = 1;

        movement = movement.normalized;

        animator.SetBool("isWalking", movement != Vector2.zero);
    }

    private void HandleExit()
    {
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

    private void HandleFootsteps()
    {
        bool isWalking = movement != Vector2.zero;

        /*if (isWalking && !wasWalking)
        {
            PlayLoopingSound(walkClip);
        }
        else if (!isWalking && wasWalking)
        {
            PlayLoopingSound(idleClip);
        }*/

        wasWalking = isWalking;
    }

    private void PlayLoopingSound(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
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

        PlayHitSound();
        StartCoroutine(FlashRed());

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void PlayHitSound()
    {
        //audioSource.PlayOneShot(hitClip);
    }

    private IEnumerator FlashRed()
    {
        playerSprite.color = Color.red;
        animator.SetTrigger("isHit");
        yield return new WaitForSeconds(0.5f);
        playerSprite.color = Color.white;
        animator.ResetTrigger("isHit");
    }

    private void Die()
    {
        Debug.Log("Player has died.");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameOverScene");
    }

    public void AddCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        currentCoins += amount;
        currentCoins = Mathf.Clamp(currentCoins, 0, maxCoins);

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

    public void ApplyDamageOverTime(int damagePerSecond, float duration)
    {
        if (damageOverTimeCoroutine != null)
        {
            StopCoroutine(damageOverTimeCoroutine);
        }
        damageOverTimeCoroutine = StartCoroutine(DamageOverTimeRoutine(damagePerSecond, duration));
    }

    private IEnumerator DamageOverTimeRoutine(int damagePerSecond, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            yield return new WaitForSeconds(1f);
            Damage(damagePerSecond);
            elapsedTime += 1f;
        }
        damageOverTimeCoroutine = null;
    }

    public void ApplySlowdown(float factor, float duration)
    {
        moveSpeed *= factor;
        StartCoroutine(RestoreSpeedAfterDelay(duration));
    }

    private IEnumerator RestoreSpeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = defaultMoveSpeed;
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

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("PlayerHealth", currentHp);
        PlayerPrefs.Save();
    }
}
