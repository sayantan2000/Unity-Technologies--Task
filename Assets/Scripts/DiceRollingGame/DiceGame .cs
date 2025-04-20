using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceGame : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private DiceGameConfig gameConfig;

    [Header("Dice Images")]
    [SerializeField] private Image firstDiceImage;
    [SerializeField] private Image secondDiceImage;
    [SerializeField] private Sprite[] diceSprites; // Array of sprites for dice faces 1-6

    [Header("UI Elements")]
    [SerializeField] private Button rollButton;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI totalText;
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button playAgainButton;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    // References
    [SerializeField] private DiceScoreManager scoreManager;

    // Game state
    private bool isRolling = false;
    private int firstDiceValue = 1;
    private int secondDiceValue = 1;
    private int rollCount = 0;

    private void Start()
    {
        // Make sure we have a config
        if (gameConfig == null)
        {
            Debug.LogError("DiceGame: No game config assigned!");
            gameConfig = ScriptableObject.CreateInstance<DiceGameConfig>();
        }

        // Set up instructions text based on config
        string winningNumbersText = string.Join(", ", gameConfig.winningNumbers);
        string losingNumbersText = string.Join(", ", gameConfig.losingNumbers);
        instructionsText.text = $"{winningNumbersText}: Win\n{losingNumbersText}: Lose\nOther numbers: Roll again";

        // Initialize UI
        resultText.text = "";
        totalText.text = "";
        gameStateText.text = "Roll the dice!";
        
        // Hide game over panel
        gameOverPanel.SetActive(false);
        
        // Set up button listeners
        rollButton.onClick.AddListener(RollDice);
        playAgainButton.onClick.AddListener(ResetGame);
        
        // Initialize dice images
        UpdateDiceImages(1, 1);
    }

    public void RollDice()
    {
        if (isRolling) return;
        
        isRolling = true;
        rollButton.interactable = false;
        rollCount++;
        
        // Play roll sound
        if (audioSource != null && rollSound != null)
        {
            audioSource.PlayOneShot(rollSound);
        }
        
        StartCoroutine(AnimateDiceRoll());
    }

    private IEnumerator AnimateDiceRoll()
    {
        float timePerFrame = gameConfig.rollAnimationDuration / gameConfig.rollAnimationFrames;
        
        // Animate dice rolling effect
        for (int i = 0; i < gameConfig.rollAnimationFrames; i++)
        {
            int randomDice1 = Random.Range(0, 6) + 1;
            int randomDice2 = Random.Range(0, 6) + 1;
            UpdateDiceImages(randomDice1, randomDice2);
            
            yield return new WaitForSeconds(timePerFrame);
        }
        
        // Final dice values
        firstDiceValue = Random.Range(1, 7);  // 1-6
        secondDiceValue = Random.Range(1, 7); // 1-6
        int total = firstDiceValue + secondDiceValue;
        
        // Update UI with final values
        UpdateDiceImages(firstDiceValue, secondDiceValue);
        totalText.text = $"Total: {total}";
        
        // Track roll in score manager
        if (scoreManager != null)
        {
            scoreManager.AddRoll();
        }
        
        // Check game rules using config
        if (gameConfig.IsWinningNumber(total))
        {
            // Win
            resultText.text = "You Win!";
            resultText.color = gameConfig.winColor;
            gameStateText.text = $"You rolled {total} and won!";
            
            // Add win to score manager
            if (scoreManager != null)
            {
                scoreManager.AddWin();
            }
            
            GameOver(true);
        }
        else if (gameConfig.IsLosingNumber(total))
        {
            // Lose
            resultText.text = "You Lose!";
            resultText.color = gameConfig.loseColor;
            gameStateText.text = $"You rolled {total} and lost!";
            
            // Add loss to score manager
            if (scoreManager != null)
            {
                scoreManager.AddLoss();
            }
            
            GameOver(false);
        }
        else
        {
            // Roll again
            resultText.text = "Roll Again";
            resultText.color = gameConfig.neutralColor;
            gameStateText.text = $"You rolled {total}. Roll again!";
            
            // Enable roll button after a short delay
            yield return new WaitForSeconds(0.5f);
            rollButton.interactable = true;
            isRolling = false;
        }
    }

    private void UpdateDiceImages(int dice1Value, int dice2Value)
    {
        // Update the dice images based on the values
        if (diceSprites != null && diceSprites.Length >= 6)
        {
            firstDiceImage.sprite = diceSprites[dice1Value - 1];
            secondDiceImage.sprite = diceSprites[dice2Value - 1];
        }
    }

    private void GameOver(bool isWin)
    {
        // Play sound effect
        if (audioSource != null)
        {
            if (isWin && winSound != null)
            {
                audioSource.PlayOneShot(winSound);
            }
            else if (!isWin && loseSound != null)
            {
                audioSource.PlayOneShot(loseSound);
            }
        }
        
        // Show game over panel after a delay
        StartCoroutine(ShowGameOverPanel());
    }

    private IEnumerator ShowGameOverPanel()
    {
        yield return new WaitForSeconds(1.5f);
        gameOverPanel.SetActive(true);
        isRolling = false;
    }

    public void ResetGame()
    {
        // Reset game state
        rollCount = 0;
        resultText.text = "";
        totalText.text = "";
        gameStateText.text = "Roll the dice!";
        
        // Hide game over panel
        gameOverPanel.SetActive(false);
        
        // Reset dice images
        UpdateDiceImages(1, 1);
        
        // Enable roll button
        rollButton.interactable = true;
        isRolling = false;
    }
}