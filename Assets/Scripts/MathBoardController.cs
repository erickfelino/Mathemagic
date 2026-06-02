using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using Unity.Multiplayer.Center.Common;

public class MathBoardController : MonoBehaviour
{
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

    private readonly List<MathSlotView> slots = new List<MathSlotView>();
    private MathPuzzleData currentPuzzle;
    [SerializeField] private MathPuzzleData selectedPuzzle;

    public void Awake()
    {
        OpenPuzzle(selectedPuzzle);
    }

    public void OpenPuzzle(MathPuzzleData puzzle)
    {
        currentPuzzle = puzzle;

        if (boardRoot != null)
            boardRoot.SetActive(true);

        if (boardCanvasGroup != null)
        {
            boardCanvasGroup.alpha = 1f;
            boardCanvasGroup.interactable = true;
            boardCanvasGroup.blocksRaycasts = true;
        }

        BuildPuzzleUI();
    }

    public void ClosePuzzle()
    {
        ClearAll();
        if (boardRoot != null)
            boardRoot.SetActive(false);
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

        foreach (MathSlotView slot in slots)
        {
            if (slot.CurrentToken == null)
            {
                SetFeedback("Complete todos os espaços.");
                return;
            }

            expression.Add(slot.CurrentToken);
        }

        if (!MathExpressionValidator.TryEvaluate(expression, out double result, out string error))
        {
            SetFeedback(error);
            return;
        }

        if (resultText != null)
            resultText.text = $"Resultado: {result:0.##}";

        double target = currentPuzzle.targetValue;
        if (Math.Abs(result - target) < 0.0001)
        {
            SetFeedback("Perfeito! Você resolveu o puzzle.");
        }
        else if (result > target)
        {
            SetFeedback("Você passou do alvo com maestria!");
        }
        else
        {
            SetFeedback("Resultado abaixo do alvo.");
        }
    }

    public void OnClearPressed()
    {
        foreach (var slot in slots)
            slot.ClearToken(true);

        if (feedbackText != null)
            feedbackText.text = "Equação limpa.";
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }
}