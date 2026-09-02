using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class MathTokenView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text labelText;

    public MathTokenKind Kind { get; private set; }
    public int NumberValue { get; private set; }
    public MathOperatorKind OperatorValue { get; private set; }

    public MathSlotView CurrentSlot { get; private set; }
    public MathSlotView OriginSlot { get; private set; }
    public Transform HomeParent { get; private set; }

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector3 originalLocalPos;
    private Tween movementTween;

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
        {
            labelText.text = GetOperatorSymbol(op);
            labelText.color = GetOperatorColor(op);
        }
    }

    public void SetCurrentSlot(MathSlotView slot)
    {
        CurrentSlot = slot;
    }

    public void ReturnToHome()
    {
        if (HomeParent == null)
            return;

        transform.SetParent(HomeParent, true);

        int insertIndex = HomeParent.childCount;
        if (Kind == MathTokenKind.Number)
        {
            insertIndex = 0;
            for (int i = 0; i < HomeParent.childCount; i++)
            {
                var child = HomeParent.GetChild(i);
                var tv = child.GetComponent<MathTokenView>();
                if (tv == null) continue;
                if (tv.Kind == MathTokenKind.Number)
                    insertIndex = i + 1;
                else
                    break;
            }
        }
        transform.SetSiblingIndex(insertIndex);
        LayoutRebuilder.ForceRebuildLayoutImmediate(HomeParent as RectTransform);

        CurrentSlot = null;
        OriginSlot = null;
    }

    public void ReturnToHomeAnimated(float duration = 0.3f)
    {
        if (HomeParent == null)
            return;

        movementTween?.Kill();

        Transform originalParent = rectTransform.parent;
        Transform overlayParent = canvas != null ? canvas.transform : originalParent;
        Vector3 worldStart = rectTransform.position;

        int insertIndex = HomeParent.childCount;
        if (Kind == MathTokenKind.Number)
        {
            insertIndex = 0;
            for (int i = 0; i < HomeParent.childCount; i++)
            {
                var child = HomeParent.GetChild(i);
                var tv = child.GetComponent<MathTokenView>();
                if (tv == null) continue;
                if (tv.Kind == MathTokenKind.Number)
                    insertIndex = i + 1;
                else
                    break;
            }
        }

        transform.SetParent(HomeParent, false);
        transform.SetSiblingIndex(insertIndex);
        LayoutRebuilder.ForceRebuildLayoutImmediate(HomeParent as RectTransform);
        Vector3 worldEnd = rectTransform.position;

        transform.SetParent(overlayParent, true);
        rectTransform.position = worldStart;

        movementTween = rectTransform.DOMove(worldEnd, duration).SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                transform.SetParent(HomeParent, false);
                transform.SetSiblingIndex(insertIndex);
                rectTransform.localPosition = Vector3.zero;
                CurrentSlot = null;
                OriginSlot = null;
            });
    }

    public void PlaceInSlot(MathSlotView slot)
    {
        if (slot == null)
            return;

        transform.SetParent(slot.transform, false);
        transform.localPosition = Vector3.zero;
        CurrentSlot = slot;
        OriginSlot = null;
    }

    public void PlaceInSlotAnimated(MathSlotView slot, float duration = 0.3f)
    {
        if (slot == null)
            return;

        movementTween?.Kill();

        Transform originalParent = rectTransform.parent;
        Transform overlayParent = canvas != null ? canvas.transform : originalParent;
        Vector3 worldStart = rectTransform.position;

        transform.SetParent(slot.transform, false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(slot.transform as RectTransform);
        Vector3 worldEnd = rectTransform.position;

        transform.SetParent(overlayParent, true);
        rectTransform.position = worldStart;

        CurrentSlot = slot;
        movementTween = rectTransform.DOMove(worldEnd, duration).SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                transform.SetParent(slot.transform, false);
                rectTransform.localPosition = Vector3.zero;
                OriginSlot = null;
            });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalLocalPos = transform.localPosition;

        if (CurrentSlot != null)
        {
            OriginSlot = CurrentSlot;
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
            ReturnToHomeAnimated(0.3f);
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

    private Color GetOperatorColor(MathOperatorKind op)
    {
        return op switch
        {
            MathOperatorKind.Add => new Color32(0, 65, 255, 255), // blue
            MathOperatorKind.Subtract => new Color32(220, 20, 60, 255), // red (crimson-like)
            MathOperatorKind.Multiply => new Color32(255, 215, 0, 255), // yellow (gold)
            MathOperatorKind.Divide => new Color32(255, 165, 0, 255), // orange
            _ => Color.white
        };
    }
}