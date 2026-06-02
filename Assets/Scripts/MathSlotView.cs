using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MathSlotView : MonoBehaviour, IDropHandler
{
    [SerializeField] private MathTokenKind acceptedKind;
    [SerializeField] private Image backgroundImage;

    public int SlotIndex { get; private set; }
    public MathTokenView CurrentToken { get; private set; }

    public void Setup(int index, MathTokenKind kind)
    {
        SlotIndex = index;
        acceptedKind = kind;
    }

    public bool CanAccept(MathTokenView token)
    {
        return token != null && token.Kind == acceptedKind;
    }

    public void OnDrop(PointerEventData eventData)
    {
        MathTokenView token = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<MathTokenView>()
            : null;

        if (token == null || !CanAccept(token))
            return;

        PlaceToken(token);
    }

    public void PlaceToken(MathTokenView token)
    {
        if (token == null || !CanAccept(token))
            return;

        if (CurrentToken != null && CurrentToken != token)
        {
            CurrentToken.ReturnToHome();
        }

        CurrentToken = token;

        if (token.CurrentSlot != null && token.CurrentSlot != this)
        {
            token.CurrentSlot.ClearToken(false);
        }

        token.PlaceInSlot(this);
    }

    public void ClearToken(bool returnToHome = true)
    {
        if (CurrentToken == null)
            return;

        MathTokenView token = CurrentToken;
        CurrentToken = null;

        if (returnToHome)
            token.ReturnToHome();
    }

    public void SetHighlight(bool active)
    {
        if (backgroundImage != null)
            backgroundImage.color = active ? Color.yellow : Color.white;
    }
}