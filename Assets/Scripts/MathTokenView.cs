using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class MathTokenView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text labelText;

    public MathTokenKind Kind { get; private set; }
    public int NumberValue { get; private set; }
    public MathOperatorKind OperatorValue { get; private set; }

    public MathSlotView CurrentSlot { get; private set; }
    public Transform HomeParent { get; private set; }

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector3 originalLocalPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetupNumber(int value, Transform homeParent)
    {
        Kind = MathTokenKind.Number;
        NumberValue = value;
        HomeParent = homeParent;

        if (labelText != null)
            labelText.text = value.ToString();
    }

    public void SetupOperator(MathOperatorKind op, Transform homeParent)
    {
        Kind = MathTokenKind.Operator;
        OperatorValue = op;
        HomeParent = homeParent;

        if (labelText != null)
            labelText.text = GetOperatorSymbol(op);
    }

    public void SetCurrentSlot(MathSlotView slot)
    {
        CurrentSlot = slot;
    }

    public void ReturnToHome()
    {
        if (HomeParent == null)
            return;

        transform.SetParent(HomeParent, false);
        transform.localPosition = originalLocalPos;
        CurrentSlot = null;
    }

    public void PlaceInSlot(MathSlotView slot)
    {
        CurrentSlot = slot;
        transform.SetParent(slot.transform, false);
        transform.localPosition = Vector3.zero;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalLocalPos = transform.localPosition;

        if (CurrentSlot != null)
        {
            CurrentSlot.ClearToken(false);
            CurrentSlot = null;
        }

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (CurrentSlot == null)
        {
            ReturnToHome();
        }
    }

    private string GetOperatorSymbol(MathOperatorKind op)
    {
        return op switch
        {
            MathOperatorKind.Add => "+",
            MathOperatorKind.Subtract => "-",
            MathOperatorKind.Multiply => "×",
            MathOperatorKind.Divide => "÷",
            _ => "?"
        };
    }
}