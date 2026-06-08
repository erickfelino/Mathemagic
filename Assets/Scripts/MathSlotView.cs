using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

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

        MathSlotView previousSlot = token.CurrentSlot ?? token.OriginSlot;
        MathTokenView occupying = CurrentToken;

        // If token already occupies this slot, nothing to do
        if (occupying == token)
            return;

        // If there's an occupying token, handle swap or return-to-home
        if (occupying != null && occupying != token)
        {
            if (previousSlot != null)
            {
                // Swap: occupying token moves to previous slot with animation
                CurrentToken = token;
                previousSlot.CurrentToken = occupying;
                
                // Token being placed goes without animation (user dragged it)
                token.PlaceInSlot(this);
                // Occupying token animates to the now-free slot
                occupying.PlaceInSlotAnimated(previousSlot, 0.3f);
                return;
            }
            else
            {
                // Token came from pool/home; send occupying back to pool
                occupying.ReturnToHomeAnimated(0.3f);
            }
        }

        // Normal placement: clear token's previous slot reference and place here
        CurrentToken = token;

        if (previousSlot != null && previousSlot != this)
            previousSlot.CurrentToken = null;

        token.PlaceInSlot(this);
    }

    public void ClearToken(bool returnToHome = true)
    {
        if (CurrentToken == null)
            return;

        MathTokenView token = CurrentToken;
        CurrentToken = null;

        if (returnToHome)
            token.ReturnToHomeAnimated(0.3f);
    }

    public void SetHighlight(bool active)
    {
        if (backgroundImage != null)
            backgroundImage.color = active ? Color.yellow : Color.white;
    }
}