using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mathemagic/Puzzle")]
public class MathPuzzleData : ScriptableObject
{
    public string puzzleName;
    public int targetValue;
    public float timeLimitSeconds = 0f;

    public List<int> numbers = new List<int>();
    public List<MathOperatorKind> operators = new List<MathOperatorKind>();

    [TextArea]
    public string hint;
}