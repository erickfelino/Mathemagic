using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPuzzleInteractor : MonoBehaviour
{
    private PuzzleWorldInteractable currentPuzzle;

    public void OnInteract(InputValue value)
    {
        if (currentPuzzle != null)
        {
            currentPuzzle.TryOpenPuzzle();
        }
    }

    public void SetCurrentPuzzle(PuzzleWorldInteractable puzzle)
    {
        currentPuzzle = puzzle;
    }

    public void ClearCurrentPuzzle(PuzzleWorldInteractable puzzle)
    {
        if (currentPuzzle == puzzle)
            currentPuzzle = null;
    }
}