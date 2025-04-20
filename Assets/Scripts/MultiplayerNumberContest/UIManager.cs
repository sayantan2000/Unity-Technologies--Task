using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Multiplayer.NumberContest
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public TextMeshProUGUI numberText;
        public Button foldButton, contestButton;
        public TextMeshProUGUI resultText;
        public TextMeshProUGUI timerText;
        
        // New UI elements for chips and betting
        public TextMeshProUGUI chipsText;
        public TextMeshProUGUI betText;
        public Button increaseBetButton;
        public Button decreaseBetButton;
        public Slider betSlider;
        
        // Flag to prevent slider value change callbacks during programmatic updates
        private bool updatingSlider = false;

        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            // Initialize betting UI
            if (betSlider != null)
            {
                betSlider.minValue = 5; // Minimum bet
                betSlider.maxValue = 100; // Initial max bet
                betSlider.value = 5; // Default bet
                betSlider.onValueChanged.AddListener(OnBetSliderChanged);
            }
            
            // Initialize chip and bet displays with default values
            UpdateChipDisplay(100);
            UpdateBetDisplay(5);
        }
        
        public void OnBetSliderChanged(float value)
        {
            // Avoid recursive calls when programmatically updating slider
            if (!updatingSlider && PlayerController.Local != null)
            {
                PlayerController.Local.SetBet(Mathf.RoundToInt(value));
            }
        }
        
        public void ConnectPlayerButtonListeners(PlayerController playerController)
        {
            // Clear any existing listeners first
            foldButton.onClick.RemoveAllListeners();
            contestButton.onClick.RemoveAllListeners();
            
            if (increaseBetButton != null)
                increaseBetButton.onClick.RemoveAllListeners();
                
            if (decreaseBetButton != null)
                decreaseBetButton.onClick.RemoveAllListeners();

            // Add new listeners
            foldButton.onClick.AddListener(playerController.ChooseFold);
            contestButton.onClick.AddListener(playerController.ChooseContest);
            
            // Add betting button listeners
            if (increaseBetButton != null)
                increaseBetButton.onClick.AddListener(() => playerController.IncreaseBet(5));
                
            if (decreaseBetButton != null)
                decreaseBetButton.onClick.AddListener(() => playerController.DecreaseBet(5));
        }

        public void DisableButtons()
        {
            foldButton.interactable = false;
            contestButton.interactable = false;
            
            if (increaseBetButton != null)
                increaseBetButton.interactable = false;
                
            if (decreaseBetButton != null)
                decreaseBetButton.interactable = false;
                
            if (betSlider != null)
                betSlider.interactable = false;
        }

        public void EnableButtons()
        {
            foldButton.interactable = true;
            contestButton.interactable = true;
            foldButton.gameObject.SetActive(true);
            contestButton.gameObject.SetActive(true);
            
            if (increaseBetButton != null)
                increaseBetButton.interactable = true;
                
            if (decreaseBetButton != null)
                decreaseBetButton.interactable = true;
                
            if (betSlider != null)
                betSlider.interactable = true;
        }

        public void ResetUI()
        {
            resultText.text = "";
            timerText.text = "Waiting...";
            UpdateBetDisplay(5);
            UpdateBetSlider(5);
            EnableButtons();
        }

        public void DisplayResult(string result)
        {
            resultText.text = result;
            DisableButtons();
        }

        public void ShowTimer(int duration)
        {
            StartCoroutine(Countdown(duration));
        }

        private IEnumerator Countdown(float duration)
        {
            float t = duration;
            while (t > 0)
            {
                // Add color based on time remaining
                string timeColor = "#FFFFFF"; // Default white
                if (t <= 3)
                    timeColor = "#FF0000"; // Red for last 3 seconds
                else if (t <= 5)
                    timeColor = "#FFFF00"; // Yellow for 4-5 seconds

                timerText.text = $"Time: <color={timeColor}>{Mathf.CeilToInt(t)}</color>";
                yield return new WaitForSeconds(1f);
                t -= 1f;
            }
            timerText.text = "<color=#FF0000>Time: 0</color>";
            DisableButtons();
        }
        
        public void SetNumber(int num, string colorHex = "#FFFFFF")
        {
            numberText.text = $"Your Number: <color={colorHex}>{num}</color>";
            EnableButtons();
            resultText.text = "";
        }
        
        public void UpdateChipDisplay(int chips)
        {
            if (chipsText != null)
            {
                chipsText.text = $"Chips: <color=#FFD700>{chips}</color>";
                
                // Update slider max value based on available chips
                if (betSlider != null)
                {
                    betSlider.maxValue = Mathf.Max(5, chips);
                }
            }
        }
        
        public void UpdateBetDisplay(int betAmount)
        {
            if (betText != null)
            {
                betText.text = $"Bet: <color=#FFD700>{betAmount}</color>";
            }
        }
        
        public void UpdateBetSlider(int value)
        {
            if (betSlider != null)
            {
                // Set flag to prevent callback execution
                updatingSlider = true;
                betSlider.value = value;
                updatingSlider = false;
            }
        }
    }
}