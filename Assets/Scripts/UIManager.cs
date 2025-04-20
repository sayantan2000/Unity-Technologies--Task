using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI numberText;
    public Button foldButton, contestButton;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI timerText;

    private void Awake()
    {
        Instance = this;
    }
    public void ConnectPlayerButtonListeners(PlayerController playerController)
    {
        // Clear any existing listeners first
        foldButton.onClick.RemoveAllListeners();
        contestButton.onClick.RemoveAllListeners();

        // Add new listeners
        foldButton.onClick.AddListener(playerController.ChooseFold);
        contestButton.onClick.AddListener(playerController.ChooseContest);
    }


    public void DisableButtons()
    {
        foldButton.interactable = false;
        contestButton.interactable = false;
    }

    public void EnableButtons()
    {
        foldButton.interactable = true;
        contestButton.interactable = true;
        foldButton.gameObject.SetActive(true);
        contestButton.gameObject.SetActive(true);
    }

    public void ResetUI()
    {
        resultText.text = "";
        timerText.text = "Waiting...";
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
}