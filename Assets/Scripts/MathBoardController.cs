using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MathBoardController : MonoBehaviour
{
    public enum PuzzleOutcome
    {
        Fail,
        Success,
        Mastery
    }

    public event Action<MathPuzzleData, PuzzleOutcome, double, List<string>> OnPuzzleResolved;

    [Header("UI Root")]
    [SerializeField] private GameObject boardRoot;
    [SerializeField] private CanvasGroup boardCanvasGroup;

    [Header("Layout")]
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private Transform poolContainer;

    [Header("Prefabs")]
    [SerializeField] private MathSlotView numberSlotPrefab;
    [SerializeField] private MathSlotView operatorSlotPrefab;
    [SerializeField] private MathTokenView numberTokenPrefab;
    [SerializeField] private MathTokenView operatorTokenPrefab;

    [Header("UI")]
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text timerText;

    private readonly List<MathSlotView> slots = new List<MathSlotView>();
    private MathPuzzleData currentPuzzle;
    private PuzzleWorldInteractable currentSource;
    private float remainingTime;
    private bool timerActive;

    public void OpenPuzzle(MathPuzzleData puzzle, PuzzleWorldInteractable source)
    {
        currentPuzzle = puzzle;
        currentSource = source;

        if (boardRoot != null)
            boardRoot.SetActive(true);

        if (boardCanvasGroup != null)
        {
            boardCanvasGroup.alpha = 1f;
            boardCanvasGroup.interactable = true;
            boardCanvasGroup.blocksRaycasts = true;
        }

        BuildPuzzleUI();
        StartTimer();
    }

    public void ClosePuzzle()
    {
        StopTimer();
        ClearAll();

        if (boardRoot != null)
            boardRoot.SetActive(false);

        // notify source that the puzzle UI was closed so it can restore player control
        if (currentSource != null)
        {
            currentSource.OnPuzzleClosed();
        }

        currentPuzzle = null;
        currentSource = null;
    }

    public void ClearAll()
    {
        foreach (Transform child in slotsContainer)
            Destroy(child.gameObject);

        foreach (Transform child in poolContainer)
            Destroy(child.gameObject);

        slots.Clear();
    }

    private void BuildPuzzleUI()
    {
        ClearAll();

        if (currentPuzzle == null)
            return;

        if (targetText != null)
            targetText.text = $"Objetivo: {currentPuzzle.targetValue}";

        if (resultText != null)
            resultText.text = "";

        if (feedbackText != null)
            feedbackText.text = currentPuzzle.hint;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(currentPuzzle.hasTimeLimit && currentPuzzle.timeLimitSeconds > 0f);
            UpdateTimerDisplay();
        }

        int totalSlots = currentPuzzle.numbers.Count + currentPuzzle.operators.Count;

        for (int i = 0; i < totalSlots; i++)
        {
            bool isNumberSlot = i % 2 == 0;

            MathSlotView prefab = isNumberSlot ? numberSlotPrefab : operatorSlotPrefab;
            MathSlotView slot = Instantiate(prefab, slotsContainer);
            slot.Setup(i, isNumberSlot ? MathTokenKind.Number : MathTokenKind.Operator);
            slots.Add(slot);
        }

        foreach (int number in currentPuzzle.numbers)
        {
            MathTokenView token = Instantiate(numberTokenPrefab, poolContainer);
            token.SetupNumber(number, poolContainer);
        }

        foreach (MathOperatorKind op in currentPuzzle.operators)
        {
            MathTokenView token = Instantiate(operatorTokenPrefab, poolContainer);
            token.SetupOperator(op, poolContainer);
        }
    }

        public void OnDonePressed()
    {
        List<MathTokenView> expression = new List<MathTokenView>();
        List<string> expressionTerms = new List<string>();

        foreach (MathSlotView slot in slots)
        {
            if (slot.CurrentToken == null)
            {
                SetFeedback("Complete todos os espaços.");
                return;
            }

            expression.Add(slot.CurrentToken);
            expressionTerms.Add(slot.CurrentToken.name);
        }

        if (!MathExpressionValidator.TryEvaluate(expression, out double result, out string error))
        {
            SetFeedback(error);
            return;
        }

        if (resultText != null)
            resultText.text = $"Resultado: {result:0.##}";

        double target = currentPuzzle.targetValue;

        if (result < target)
        {
            SetFeedback("Resultado abaixo do alvo.");
            return;
        }

        PuzzleOutcome outcome = Math.Abs(result - target) < 0.0001
            ? PuzzleOutcome.Success
            : PuzzleOutcome.Mastery;

        if (outcome == PuzzleOutcome.Success)
            SetFeedback("Perfeito! Você resolveu o puzzle.");
        else
            SetFeedback("Você passou do alvo com maestria!");

        OnPuzzleResolved?.Invoke(currentPuzzle, outcome, result, expressionTerms);

        if (currentSource != null)
            currentSource.NotifyPuzzleResolved(outcome, result, expressionTerms);

        ClosePuzzle();
    }

    public void OnClearPressed()
    {
        foreach (var slot in slots)
            slot.ClearToken(true);

        if (feedbackText != null)
            feedbackText.text = "Equação limpa.";
    }

    private void StartTimer()
    {
        if (currentPuzzle == null || !currentPuzzle.hasTimeLimit || currentPuzzle.timeLimitSeconds <= 0f)
        {
            timerActive = false;
            return;
        }

        remainingTime = currentPuzzle.timeLimitSeconds;
        timerActive = true;
        UpdateTimerDisplay();
    }

    private void StopTimer()
    {
        timerActive = false;
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        if (currentPuzzle == null || !currentPuzzle.hasTimeLimit || currentPuzzle.timeLimitSeconds <= 0f)
        {
            timerText.text = string.Empty;
            return;
        }

        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string formattedTime = minutes > 0 ? $"{minutes:00}:{seconds:00}" : $"{seconds:00}s";

        timerText.text = $"Tempo restante: {formattedTime}";
    }

    private void Update()
    {
        if (!timerActive || currentPuzzle == null)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerDisplay();
            timerActive = false;
            if (feedbackText != null)
                feedbackText.text = "Tempo esgotado.";
            ClosePuzzle();
            return;
        }

        UpdateTimerDisplay();
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }
}