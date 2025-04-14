using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class BedSaveSystem : MonoBehaviour
{
    // Sistema de diálogo
    [SerializeField] private GameObject dialogueMark;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private float typingTime = 0.05f;
    [SerializeField] private PlayerController playerController;

    // UI principal
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;

    // Panel de selección de slots
    [SerializeField] private GameObject slotSelectionPanel;
    [SerializeField] private Button[] slotButtons; // Botones Espacio 1, 2, 3
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject nameInputPanel;

    // Textos de los botones de slot asignados manualmente
    [SerializeField] private TMP_Text slot1Text;
    [SerializeField] private TMP_Text slot2Text;
    [SerializeField] private TMP_Text slot3Text;

    // Panel de opciones cuando ya hay una partida guardada
    [SerializeField] private GameObject overwritePanel;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button overwriteButton;

    // Botón para guardar el nombre y la partida
    [SerializeField] private Button saveNameButton;

    private int currentLine = 0;
    private bool isPlayerInRange = false;
    private bool isDialogueRunning = false;
    private bool isInteracting = false;  // Nueva variable para manejar la interacción
    private bool isInSaveMenu = false;  // Nueva variable para bloquear la tecla E cuando está en el menú de guardado
    private int selectedSlotIndex = -1;
    private string[] slotFileNames = { "slot1.json", "slot2.json", "slot3.json" };
    private TMP_Text[] slotTexts;

    private void Awake()
    {
        if (dialogueMark == null || dialoguePanel == null || dialogueText == null || buttonsPanel == null ||
            saveButton == null || exitButton == null || slotSelectionPanel == null || slotButtons.Length != 3 ||
            nameInputField == null || nameInputPanel == null || overwritePanel == null ||
            deleteButton == null || overwriteButton == null || saveNameButton == null ||
            slot1Text == null || slot2Text == null || slot3Text == null)
        {
            Debug.LogError("Faltan referencias en el inspector.");
        }

        buttonsPanel.SetActive(false);
        slotSelectionPanel.SetActive(false);
        nameInputPanel.SetActive(false);
        overwritePanel.SetActive(false);

        saveButton.onClick.AddListener(OpenSlotPanel);
        exitButton.onClick.AddListener(ExitToMainMenu);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            slotButtons[i].onClick.AddListener(() => HandleSlotSelection(index));
        }

        saveNameButton.onClick.AddListener(SaveNameAndGame);
        deleteButton.onClick.AddListener(DeleteSave);
        overwriteButton.onClick.AddListener(OverwriteSave);

        slotTexts = new TMP_Text[] { slot1Text, slot2Text, slot3Text };
    }

    private void Update()
    {
        // Desactiva el control mientras se escribe un nombre
        if (nameInputPanel.activeSelf && nameInputField.isFocused)
        {
            if (playerController.enabled)
                playerController.enabled = false;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                SaveNameAndGame();
            }

            return;
        }
        else if (!isDialogueRunning && !playerController.enabled)
        {
            playerController.enabled = true;
        }

        // Si estamos en el menú de guardado, bloqueamos la tecla E
        if (isInSaveMenu)
        {
            return;  // No hacer nada cuando estamos en el menú de guardado
        }

        // Verifica si está en rango y si la interacción aún no se ha iniciado
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isInteracting && !isInSaveMenu)
        {
            if (!isDialogueRunning)
            {
                isInteracting = true;  // Marca que se ha iniciado la interacción
                StartDialogue();
            }
            else if (dialogueText.text != dialogueLines[currentLine])
            {
                StopAllCoroutines();  // Detenemos la escritura
                dialogueText.text = dialogueLines[currentLine];  // Mostramos el texto completo inmediatamente
            }
            else
            {
                DisplayNextLine();  // Continuamos con la siguiente línea
            }
        }

        // Si ya estamos escribiendo, y presionamos E, avanzamos al siguiente texto rápidamente
        if (isDialogueRunning && Input.GetKeyDown(KeyCode.E))
        {
            StopAllCoroutines();  // Detenemos cualquier corrutina de escritura que esté en curso
            dialogueText.text = dialogueLines[currentLine];  // Muestra el texto completo
            DisplayNextLine();  // Continúa con la siguiente línea
        }
    }

    private void StartDialogue()
    {
        dialogueMark.SetActive(false);
        dialoguePanel.SetActive(true);
        currentLine = 0;
        isDialogueRunning = true;
        playerController.enabled = false;
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
        buttonsPanel.SetActive(true);
        isDialogueRunning = false;
        isInteracting = false;  // Reseteamos la interacción
        playerController.enabled = true;
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
            slotSelectionPanel.SetActive(false);
            nameInputPanel.SetActive(false);
            overwritePanel.SetActive(false);

            if (isDialogueRunning)
            {
                EndDialogue();
            }
        }
    }

    private void OpenSlotPanel()
    {
        isInSaveMenu = true;  // Bloqueamos la tecla E cuando se abre el panel de guardado
        buttonsPanel.SetActive(false);  // Ocultamos el panel de botones
        slotSelectionPanel.SetActive(true);

        for (int i = 0; i < slotFileNames.Length; i++)
        {
            string path = Path.Combine(Application.persistentDataPath, slotFileNames[i]);
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                SaveSlotData saved = JsonUtility.FromJson<SaveSlotData>(content);
                slotTexts[i].text = saved.slotName;
            }
            else
            {
                slotTexts[i].text = "Espacio " + (i + 1);
            }
        }
    }

    private void HandleSlotSelection(int index)
    {
        selectedSlotIndex = index;
        string path = Path.Combine(Application.persistentDataPath, slotFileNames[index]);

        if (File.Exists(path))
        {
            overwritePanel.SetActive(true);
        }
        else
        {
            nameInputPanel.SetActive(true);
            nameInputField.text = "";
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
    }

    private void SaveNameAndGame()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= slotFileNames.Length)
        {
            Debug.LogError("Índice de slot inválido.");
            return;
        }

        string input = nameInputField.text;

        if (string.IsNullOrEmpty(input))
        {
            Debug.LogWarning("Nombre de guardado vacío.");
            return;
        }

        if (FullGameController.Instance == null)
        {
            Debug.LogError("FullGameController.Instance es null.");
            return;
        }

        FullGameController.Instance.SaveGameToFile(slotFileNames[selectedSlotIndex], input);

        if (slotTexts[selectedSlotIndex] != null)
        {
            slotTexts[selectedSlotIndex].text = input;
        }

        nameInputPanel.SetActive(false);
        slotSelectionPanel.SetActive(false);
        buttonsPanel.SetActive(false);
        isInSaveMenu = false;  // Reactivamos la tecla E después de guardar

        Debug.Log($"Guardado en slot {selectedSlotIndex + 1} como '{input}'");
        selectedSlotIndex = -1;
    }

    private void DeleteSave()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= slotFileNames.Length)
        {
            Debug.LogError("Índice de slot inválido al intentar borrar.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, slotFileNames[selectedSlotIndex]);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        if (slotTexts[selectedSlotIndex] != null)
        {
            slotTexts[selectedSlotIndex].text = "Espacio " + (selectedSlotIndex + 1);
        }

        overwritePanel.SetActive(false);
        selectedSlotIndex = -1;
    }

    private void OverwriteSave()
    {
        if (selectedSlotIndex < 0 || selectedSlotIndex >= slotFileNames.Length)
        {
            Debug.LogError("Índice de slot inválido al sobrescribir.");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, slotFileNames[selectedSlotIndex]);

        if (!File.Exists(path))
        {
            Debug.LogWarning("No hay datos para sobrescribir en este slot.");
            return;
        }

        string content = File.ReadAllText(path);
        SaveSlotData saved = JsonUtility.FromJson<SaveSlotData>(content);

        if (FullGameController.Instance != null)
        {
            FullGameController.Instance.SaveGameToFile(slotFileNames[selectedSlotIndex], saved.slotName);
        }

        overwritePanel.SetActive(false);
        slotSelectionPanel.SetActive(false);
        buttonsPanel.SetActive(false);
        isInSaveMenu = false;  // Reactivamos la tecla E después de sobrescribir

        Debug.Log("Datos sobrescritos en el slot " + (selectedSlotIndex + 1));
        selectedSlotIndex = -1;
    }

    private void ExitToMainMenu()
    {
        if (FullGameController.Instance != null)
        {
            FullGameController.Instance.LoadScene("MainMenuScene");
        }
    }

    // Aquí está la clase SaveSlotData actualizada
    [System.Serializable]
    public class SaveSlotData
    {
        public string slotName;

        public int playerHealth;
        public int playerEnergy;
        public string currentItem;
        public int coins;

        public int level1;
        public int level2;
        public int level3;
        public int finalBossUnlocked;

        public int final;
        public int final2;
        public int final3;

        public int npc;
        public float gameVolume;
    }
}
