using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BedSaveSystem : MonoBehaviour
{
    // Dialogue system
    [SerializeField] private GameObject dialogueMark;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private float typingTime = 0.05f;
    [SerializeField] private PlayerController playerController;

    // UI for save/exit panel
    [SerializeField] private GameObject buttonsPanel; // Panel de botones
    [SerializeField] private Button saveButton; // Botón de guardar
    [SerializeField] private Button exitButton; // Botón de salir

    private int currentLine = 0;
    private bool isPlayerInRange = false;
    private bool isDialogueRunning = false;

    private void Awake()
    {
        // Ensure the references are not null
        if (dialogueMark == null || dialoguePanel == null || dialogueText == null || buttonsPanel == null || saveButton == null || exitButton == null)
        {
            Debug.LogError("Missing references! Ensure all UI elements are assigned in the Inspector.");
        }

        buttonsPanel.SetActive(false); // Asegúrate de que el panel de botones esté desactivado al inicio

        // Asigna listeners a los botones
        saveButton.onClick.AddListener(SaveGame);
        exitButton.onClick.AddListener(ExitToMainMenu);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isDialogueRunning)
            {
                StartDialogue();
            }
            else if (dialogueText.text != dialogueLines[currentLine])
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[currentLine];
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    private void StartDialogue()
    {
        dialogueMark.SetActive(false);
        dialoguePanel.SetActive(true);
        currentLine = 0;
        isDialogueRunning = true;
        playerController.enabled = false; // Desactiva el movimiento del jugador
        StartCoroutine(ShowLine());
    }

    private void DisplayNextLine()
    {
        currentLine++;
        if (currentLine < dialogueLines.Length)
        {
            StartCoroutine(ShowLine());
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator ShowLine()
    {
        dialogueText.text = string.Empty;
        foreach (char ch in dialogueLines[currentLine])
        {
            dialogueText.text += ch;
            yield return new WaitForSeconds(typingTime);
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        buttonsPanel.SetActive(true); // Activa el panel de botones
        isDialogueRunning = false;
        playerController.enabled = true; // Reactiva el movimiento del jugador
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            isPlayerInRange = true;
            dialogueMark.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            isPlayerInRange = false;
            dialogueMark.SetActive(false);
            dialoguePanel.SetActive(false);
            buttonsPanel.SetActive(false);

            if (isDialogueRunning)
            {
                EndDialogue();
            }
        }
    }

    // Guardar partida
    private void SaveGame()
    {
        FullGameController.Instance.SaveGame();
        Debug.Log("Game saved through the bed system.");
    }

    // Salir al menú principal
    private void ExitToMainMenu()
    {
        FullGameController.Instance.LoadScene("MainMenuScene");
    }
}
