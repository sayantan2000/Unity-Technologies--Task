using Photon.Pun;
using UnityEngine;
namespace Multiplayer.NumberContest
{
    /// <summary>
    /// This class handles the player controls and decisions
    /// </summary>
    public class PlayerController : MonoBehaviourPun
    {
        public static PlayerController Local;

        private int number;
        private int chips = 100;
        private int currentBet = 5; // Default minimum bet

        void Start()
        {
            if (photonView.IsMine)
            {
                // Connect this player to the UI buttons
                UIManager.Instance.ConnectPlayerButtonListeners(this);
                
                // Initialize UI with starting values
                UIManager.Instance.UpdateChipDisplay(chips);
                UIManager.Instance.UpdateBetDisplay(currentBet);
            }
        }

        void Awake()
        {
            if (photonView.IsMine)
            {
                Local = this;
                Debug.Log("Local player controller initialized");
            }
        }

        public void SetNumber(int num)
        {
            number = num;
            if (photonView.IsMine)
            {
                // Colorize number based on value (higher is greener)
                string colorHex;
                if (num >= 75)
                    colorHex = "#00FF00"; // Green for high numbers
                else if (num >= 50)
                    colorHex = "#FFFF00"; // Yellow for medium numbers
                else if (num >= 25)
                    colorHex = "#FFA500"; // Orange for lower numbers
                else
                    colorHex = "#FF0000"; // Red for very low numbers

                UIManager.Instance.SetNumber(num, colorHex);
                Debug.Log($"My number is set to: {num}");
            }
        }

        public void SetChips(int amount)
        {
            chips = amount;
            if (photonView.IsMine)
            {
                // Also update the bet slider max value
                UIManager.Instance.UpdateChipDisplay(chips);
                
                // Make sure current bet doesn't exceed chip count
                if (currentBet > chips)
                {
                    SetBet(chips);
                }
            }
        }

        public void IncreaseBet(int amount)
        {
            if (currentBet + amount <= chips)
            {
                currentBet += amount;
                UIManager.Instance.UpdateBetDisplay(currentBet);
                UIManager.Instance.UpdateBetSlider(currentBet);
            }
        }

        public void DecreaseBet(int amount)
        {
            // Ensure minimum bet of 5
            if (currentBet - amount >= 5)
            {
                currentBet -= amount;
                UIManager.Instance.UpdateBetDisplay(currentBet);
                UIManager.Instance.UpdateBetSlider(currentBet);
            }
        }

        public void SetBet(int amount)
        {
            // Clamp bet between 5 and available chips
            currentBet = Mathf.Clamp(amount, 5, chips);
            UIManager.Instance.UpdateBetDisplay(currentBet);
            UIManager.Instance.UpdateBetSlider(currentBet);
        }

        public void ChooseFold()
        {
            if (photonView.IsMine)
            {
                Debug.Log("Player chose to Fold");
                GameManager.Instance.RegisterDecision("Fold", 0);
                UIManager.Instance.DisableButtons();
            }
        }

        public void ChooseContest()
        {
            if (photonView.IsMine)
            {
                Debug.Log($"Player chose to Contest with bet: {currentBet}");
                GameManager.Instance.RegisterDecision("Contest", currentBet);
                UIManager.Instance.DisableButtons();
            }
        }
    }
}