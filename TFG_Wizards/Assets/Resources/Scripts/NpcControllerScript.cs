using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private GameObject dialogueMark;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject interactText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private float typingTime = 0.05f;
    [SerializeField] private PlayerController playerMovement;
    [SerializeField] private GameObject[] gameObjectsToDisable; // Objetos que se desactivan tras hablar con el NPC (3 niveles en gris, deshabilitados)


    [SerializeField] private BoxCollider2D[] collidersToActivate; // Los 3 colisionadores que se activarán

    public bool npcHasInteracted = false; // Bool público que controlará la activación de los colisionadores

    private int currentLine = 0;
    private bool isPlayerInRange = false;
    private bool isDialogueRunning = false;

    private void Start()
    {
        // Cargar el valor guardado en PlayerPrefs
        npcHasInteracted = PlayerPrefs.GetInt("NPC", 0) > 0;

        // Si el valor es mayor que cero, activar los colisionadores
        if (npcHasInteracted)
        {
            ActivateColliders();
            DisableGameObjects();
        }
        else
        {
            DeactivateColliders();
        }
    }

    void Update()
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
        interactText.SetActive(false);
        dialoguePanel.SetActive(true);
        currentLine = 0;
        isDialogueRunning = true;
        playerMovement.enabled = false;
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
        dialogueMark.SetActive(true);
        interactText.SetActive(true);
        isDialogueRunning = false;
        playerMovement.enabled = true;

        // Aumentar el valor del NPC en PlayerPrefs si no se ha interactuado antes
        int npcValue = PlayerPrefs.GetInt("NPC", 0) + 1;
        npcValue++;
        PlayerPrefs.SetInt("NPC", npcValue);
        PlayerPrefs.Save();

        // Si el valor es mayor a cero, activar los colisionadores
        if (npcValue > 0)
        {
            npcHasInteracted = true;
            ActivateColliders();
            DisableGameObjects();
        }

    }

    private void ActivateColliders()
    {
        // Activar los colisionadores
        foreach (BoxCollider2D col in collidersToActivate)
        {
            if (col != null)
                col.enabled = true;
        }
    }

    private void DeactivateColliders()
    {
        // Desactivar los colisionadores
        foreach (BoxCollider2D col in collidersToActivate)
        {
            if (col != null)
                col.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = true;
            dialogueMark.SetActive(true);
            interactText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = false;
            dialogueMark.SetActive(false);
            interactText.SetActive(false);
            dialoguePanel.SetActive(false);
            if (isDialogueRunning)
            {
                EndDialogue();
            }
        }
    }
    private void DisableGameObjects()
    {
        if (gameObjectsToDisable == null) return;

        foreach (GameObject go in gameObjectsToDisable)
        {
            if (go != null)
                go.SetActive(false);
        }
    }

}
