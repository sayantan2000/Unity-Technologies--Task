using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviourPun
{
    public static PlayerController Local;

    private int number;
    void Start()
    {
        if (photonView.IsMine)
        {
            // Connect this player to the UI buttons
            UIManager.Instance.ConnectPlayerButtonListeners(this);
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

    public void ChooseFold()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Player chose to Fold");
            GameManager.Instance.RegisterDecision("Fold");
            UIManager.Instance.DisableButtons();
        }
    }

    public void ChooseContest()
    {
        if (photonView.IsMine)
        {
            Debug.Log("Player chose to Contest");
            GameManager.Instance.RegisterDecision("Contest");
            UIManager.Instance.DisableButtons();
        }
    }
}