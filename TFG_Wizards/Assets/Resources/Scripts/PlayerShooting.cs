using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject primarySpellPrefab;
    public GameObject secondarySpellPrefab;
    public Transform shootPoint;

    [Header("Cooldown")]
    [Tooltip("Tiempo entre disparos. 0.25 = 4 disparos/segundo")]
    public float shootCooldown = 0.25f;
    private float nextShootTime = 0f;

    [Header("Damage")]
    public float damageMultiplier = 1f;

    [Header("Energy System")]
    public int currentEnergy;
    public int secondarySpellCost = 20;

    [Header("UI Panel")]
    public GameObject shootPanel;

    [Header("UI Texts")]
    public TextMeshProUGUI attackLabelText;   // ModoText in your hierarchy
    public TextMeshProUGUI energyCounterText; // EnergiaCounter in your hierarchy

    [Header("UI Mode Images")]
    public GameObject modeNormalImg; // ModeNormalImg
    public GameObject modeStrongImg; // ModeStrongImg

    [Header("UI Settings")]
    public string attackLabel = "ATAQUE:";

    [Header("Audio Settings")]
    public AudioClip primaryShootClip;
    public AudioClip secondaryShootClip;
    private AudioSource audioSource;

    private bool isUsingPrimaryAttack = true;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (!PlayerPrefs.HasKey("PlayerEnergy"))
        {
            PlayerPrefs.SetInt("PlayerEnergy", 100);
            PlayerPrefs.Save();
        }

        currentEnergy = PlayerPrefs.GetInt("PlayerEnergy", 100);

        UpdateModeUI();
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isUsingPrimaryAttack = !isUsingPrimaryAttack;
            UpdateModeUI();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryShoot(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TryShoot(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryShoot(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryShoot(Vector2.right);
        }
    }

    private void TryShoot(Vector2 direction)
    {
        if (Time.time < nextShootTime)
            return;

        nextShootTime = Time.time + shootCooldown;

        Shoot(direction);
    }

    private void Shoot(Vector2 direction)
    {
        if (isUsingPrimaryAttack)
        {
            if (primarySpellPrefab == null || shootPoint == null) return;

            int baseDamage = 10;
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * damageMultiplier));

            Instantiate(primarySpellPrefab, shootPoint.position, Quaternion.identity)
                .GetComponent<SpellPlayerScript>()
                .Initialize(direction, finalDamage);

            PlayShootSound(primaryShootClip);
        }
        else
        {
            if (currentEnergy >= secondarySpellCost)
            {
                if (secondarySpellPrefab == null || shootPoint == null) return;

                int baseDamage = 50;
                int finalDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * damageMultiplier));

                Instantiate(secondarySpellPrefab, shootPoint.position, Quaternion.identity)
                    .GetComponent<SpellPlayerSecondary>()
                    .Initialize(direction, finalDamage);

                currentEnergy -= secondarySpellCost;
                SaveEnergy();
                UpdateUI();

                PlayShootSound(secondaryShootClip);
            }
            else
            {
                Debug.Log("Not enough energy for secondary attack.");

                nextShootTime = Time.time;
                return;
            }
        }

        StartCoroutine(TriggerAttackAnimation());
    }

    private void PlayShootSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
            yield return new WaitForSeconds(0.1f);
            animator.SetBool("isAttacking", false);
        }
        else
        {
            yield return null;
        }
    }

    public void AddEnergy(int amount)
    {
        currentEnergy += amount;
        SaveEnergy();
        UpdateUI();
    }

    private void SaveEnergy()
    {
        PlayerPrefs.SetInt("PlayerEnergy", currentEnergy);
        PlayerPrefs.Save();
    }

    private void UpdateUI()
    {
        if (attackLabelText != null)
            attackLabelText.text = attackLabel;

        if (energyCounterText != null)
            energyCounterText.text = currentEnergy.ToString();

        // Optional: hide panel if not assigned or you do not want auto behavior
        // if (shootPanel != null) shootPanel.SetActive(true);
    }

    private void UpdateModeUI()
    {
        if (modeNormalImg != null)
            modeNormalImg.SetActive(isUsingPrimaryAttack);

        if (modeStrongImg != null)
            modeStrongImg.SetActive(!isUsingPrimaryAttack);
    }

    private void OnApplicationQuit()
    {
        SaveEnergy();
    }
}
