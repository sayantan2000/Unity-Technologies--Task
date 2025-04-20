using UnityEngine;

[CreateAssetMenu(fileName = "DiceGameConfig", menuName = "DiceGame/Config")]
public class DiceGameConfig : ScriptableObject
{
    [Header("Game Rules")]
    public int[] winningNumbers = { 7, 11 };
    public int[] losingNumbers = { 2, 3, 12 };
    
    [Header("Visual Settings")]
    public Color winColor = new Color(0.3f, 0.8f, 0.3f);
    public Color loseColor = new Color(0.8f, 0.3f, 0.3f);
    public Color neutralColor = new Color(0.8f, 0.8f, 0.3f);
    
    [Header("Animation Settings")]
    public float rollAnimationDuration = 1.5f;
    public int rollAnimationFrames = 15;
    
    public bool IsWinningNumber(int number)
    {
        foreach (int winNum in winningNumbers)
        {
            if (number == winNum)
                return true;
        }
        return false;
    }
    
    public bool IsLosingNumber(int number)
    {
        foreach (int loseNum in losingNumbers)
        {
            if (number == loseNum)
                return true;
        }
        return false;
    }
}