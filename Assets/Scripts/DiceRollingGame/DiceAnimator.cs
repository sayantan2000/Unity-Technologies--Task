using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceAnimator : MonoBehaviour
{
    [SerializeField] private Image diceImage;
    [SerializeField] private Sprite[] diceSprites;
    [SerializeField] private float animationSpeed = 0.05f;
    
    public void AnimateRoll(int finalValue)
    {
        StartCoroutine(AnimateDiceRollRoutine(finalValue));
    }
    
    private IEnumerator AnimateDiceRollRoutine(int finalValue)
    {
        // Perform a few random rolls for animation
        for (int i = 0; i < 20; i++)
        {
            int randomFace = Random.Range(0, 6);
            diceImage.sprite = diceSprites[randomFace];
            yield return new WaitForSeconds(animationSpeed);
        }
        
        // Set final value (subtract 1 because array is 0-indexed)
        diceImage.sprite = diceSprites[finalValue - 1];
    }
}