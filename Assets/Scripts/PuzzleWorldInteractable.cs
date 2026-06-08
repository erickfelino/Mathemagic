using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PuzzleWorldInteractable : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private MathPuzzleData puzzleData;
    [SerializeField] private MathBoardController boardController;

    [Header("UI/Feedback")]
    [SerializeField] private GameObject interactionPrompt;

    [Header("State")]
    [SerializeField] private bool solved = false;

    private bool playerInside = false;
    private PlayerPuzzleInteractor currentPlayerInteractor;
    private StarterAssetsInputs playerInputs;
    private PlayerInput playerInputComponent;

    public bool IsSolved => solved;
    public MathPuzzleData PuzzleData => puzzleData;

    private void Awake()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (solved)
            return;

        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        currentPlayerInteractor = other.GetComponent<PlayerPuzzleInteractor>();
        if (currentPlayerInteractor != null)
        {
            currentPlayerInteractor.SetCurrentPuzzle(this);
        }

        // cache player input component so we can enable/disable it while puzzle is open
        playerInputs = other.GetComponentInChildren<StarterAssetsInputs>();
        playerInputComponent = other.GetComponent<PlayerInput>();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (currentPlayerInteractor != null)
        {
            currentPlayerInteractor.ClearCurrentPuzzle(this);
            currentPlayerInteractor = null;
        }

        playerInputs = null;
        playerInputComponent = null;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void TryOpenPuzzle()
    {
        if (solved || !playerInside)
            return;

        if (boardController == null || puzzleData == null)
        {
            Debug.LogWarning($"Puzzle {name} não está configurado corretamente.");
            return;
        }

        // hide prompt and disable player controls so they can't move while solving
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);


        if (playerInputs != null)
            playerInputs.SetInputEnabled(false);

        if (playerInputComponent != null)
            playerInputComponent.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        boardController.OpenPuzzle(puzzleData, this);
    }

    // Called by the board controller when the puzzle UI is closed
    public void OnPuzzleClosed()
    {
        if (playerInputs != null)
            playerInputs.SetInputEnabled(true);

        if (playerInputComponent != null)
            playerInputComponent.enabled = true;
    }

    public void NotifyPuzzleResolved(MathBoardController.PuzzleOutcome outcome, double result, System.Collections.Generic.List<string> expressionTerms)
    {
        if (outcome == MathBoardController.PuzzleOutcome.Fail)
            return;

        solved = true;
        playerInside = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
            col.enabled = false;

        Debug.Log($"Puzzle {name} resolvido com sucesso! Resultado: {result:0.##}");
    }
}