using UnityEngine;
using TMPro;

public class StoreControllerScript : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text coinCounterText; // Texto del contador de monedas

    private int lastCoinCount;

    private void Start()
    {
        // Inicializa el contador con el valor actual de monedas
        UpdateCoinCounter();
    }

    private void Update()
    {
        // Verifica si las monedas han cambiado
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        if (currentCoins != lastCoinCount)
        {
            UpdateCoinCounter();
        }
    }

    private void UpdateCoinCounter()
    {
        // Actualiza el contador de monedas y guarda el último valor
        lastCoinCount = PlayerPrefs.GetInt("Coins", 0);
        if (coinCounterText != null)
        {
            coinCounterText.text = $"{lastCoinCount}";
        }
        else
        {
            Debug.LogWarning("Coin counter text is not assigned in the Inspector.");
        }
    }
}
