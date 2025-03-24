using UnityEngine;
using TMPro;

public class SimpleNPCDialogueScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel; // Panel de di�logo
    [SerializeField] private TMP_Text dialogueText; // Texto de di�logo

    [Header("Dialogue Settings")]
    [SerializeField] private string dialogueLine = "Hello, traveler!"; // Frase del NPC

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false); // Aseg�rate de que el panel est� desactivado al inicio
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (dialoguePanel != null && dialogueText != null)
            {
                dialoguePanel.SetActive(true); // Muestra el panel de di�logo
                dialogueText.text = dialogueLine; // Muestra el texto de di�logo
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false); // Cierra el panel de di�logo
            }
        }
    }
}
