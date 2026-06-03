using UnityEngine;

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

    public bool IsSolved => solved;
    public MathPuzzleData PuzzleData => puzzleData;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
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

        boardController.OpenPuzzle(puzzleData, this);
    }

    public void NotifyPuzzleResolved(MathBoardController.PuzzleOutcome outcome, double result, System.Collections.Generic.List<string> expressionTerms)
    {
        if (outcome == MathBoardController.PuzzleOutcome.Fail)
            return;

        solved = true;
        playerInside = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log($"Puzzle {name} resolvido com sucesso! Resultado: {result:0.##}");
    }
}