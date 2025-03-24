using UnityEngine;
using TMPro;

public class SimpleNPCDialogueScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel; // Panel de diálogo
    [SerializeField] private TMP_Text dialogueText; // Texto de diálogo

    [Header("Dialogue Settings")]
    [SerializeField] private string dialogueLine = "Hello, traveler!"; // Frase del NPC

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false); // Asegúrate de que el panel esté desactivado al inicio
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (dialoguePanel != null && dialogueText != null)
            {
                dialoguePanel.SetActive(true); // Muestra el panel de diálogo
                dialogueText.text = dialogueLine; // Muestra el texto de diálogo
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false); // Cierra el panel de diálogo
            }
        }
    }
}
