using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject primarySpellPrefab; // Prefab del disparo primario
    public GameObject secondarySpellPrefab; // Prefab del disparo secundario
    public Transform shootPoint; // Punto de disparo del jugador

    [Header("Energy System")]
    public int currentEnergy;
    public int secondarySpellCost = 20; // Energ�a que cuesta el segundo ataque

    [Header("UI Elements")]
    public TextMeshProUGUI shootingModeText;
    public TextMeshProUGUI energyText;

    private bool isUsingPrimaryAttack = true;
    private Animator animator; // Referencia al Animator

    private void Start()
    {
        animator = GetComponent<Animator>(); // Obtener el Animator del jugador

        // Cargar energ�a guardada o iniciar con 100 si es la primera vez
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
        // Alternar entre los disparos con la barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isUsingPrimaryAttack = !isUsingPrimaryAttack;
            shootingModeText.text = isUsingPrimaryAttack ? "Modo: Disparo Primario" : "Modo: Disparo Secundario";
        }

        // Disparar con las flechas sin afectar el movimiento del jugador
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Shoot(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Shoot(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Shoot(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Shoot(Vector2.right);
        }
    }

    private void Shoot(Vector2 direction)
    {
        if (isUsingPrimaryAttack)
        {
            Instantiate(primarySpellPrefab, shootPoint.position, Quaternion.identity)
                .GetComponent<SpellPlayerScript>().Initialize(direction, 10);
        }
        else
        {
            if (currentEnergy >= secondarySpellCost)
            {
                Instantiate(secondarySpellPrefab, shootPoint.position, Quaternion.identity)
                    .GetComponent<SpellPlayerSecondary>().Initialize(direction, 50);

                currentEnergy -= secondarySpellCost;
                SaveEnergy();
                UpdateUI();
            }
            else
            {
                Debug.Log("No tienes suficiente energ�a para disparar el ataque secundario.");
                return;
            }
        }

        // Activar la animaci�n de ataque
        StartCoroutine(TriggerAttackAnimation());
    }

    private IEnumerator TriggerAttackAnimation()
    {
        animator.SetBool("isAttacking", true);
        yield return new WaitForSeconds(0.1f); // Espera un instante para que la animaci�n se reproduzca
        animator.SetBool("isAttacking", false);
    }

    // M�todo corregido para a�adir energ�a SIN L�MITE
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
        energyText.text = $"Energ�a: {currentEnergy}";
    }

    private void OnApplicationQuit()
    {
        SaveEnergy();
    }
}
