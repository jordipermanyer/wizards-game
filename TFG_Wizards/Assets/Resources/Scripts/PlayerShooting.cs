using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject primarySpellPrefab; // Prefab del disparo primario
    public GameObject secondarySpellPrefab; // Prefab del disparo secundario
    public Transform shootPoint; // Punto de disparo del jugador

    [Header("Cooldown")]
    [Tooltip("Tiempo entre disparos. 0.25 = 4 disparos/segundo")]
    public float shootCooldown = 0.25f;
    private float nextShootTime = 0f;

    [Header("Damage")]
    public float damageMultiplier = 1f; // 1 = normal, 1.5 = +50 percent

    [Header("Energy System")]
    public int currentEnergy;
    public int secondarySpellCost = 20; // Energia que cuesta el segundo ataque

    [Header("UI Elements")]
    public TextMeshProUGUI shootingModeText;
    public TextMeshProUGUI energyText;

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
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isUsingPrimaryAttack = !isUsingPrimaryAttack;
            if (shootingModeText != null)
                shootingModeText.text = isUsingPrimaryAttack ? "Modo: Disparo Primario" : "Modo: Disparo Secundario";
        }

        // Flechas: dispara SOLO si ha pasado el cooldown
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
        // Si aun no toca, no dispara
        if (Time.time < nextShootTime)
            return;

        // Reservamos el siguiente disparo
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
                Debug.Log("No tienes suficiente energia para disparar el ataque secundario.");

                // If no shot due to energy, do not punish the player with cooldown
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
        if (energyText != null)
            energyText.text = $"Energia: {currentEnergy}";
    }

    private void OnApplicationQuit()
    {
        SaveEnergy();
    }
}
