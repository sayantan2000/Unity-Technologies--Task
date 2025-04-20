using UnityEngine;
using TMPro;

public class DiceScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winsText;
    [SerializeField] private TextMeshProUGUI lossesText;
    [SerializeField] private TextMeshProUGUI rollsText;
    [SerializeField] private TextMeshProUGUI winRateText;
    
    private int totalWins = 0;
    private int totalLosses = 0;
    private int totalRolls = 0;
    
    private void Start()
    {
        // Load saved stats
        LoadStats();
        UpdateUI();
    }
    
    public void AddWin()
    {
        totalWins++;
        totalRolls++;
        UpdateUI();
        SaveStats();
    }
    
    public void AddLoss()
    {
        totalLosses++;
        totalRolls++;
        UpdateUI();
        SaveStats();
    }
    
    public void AddRoll()
    {
        totalRolls++;
        UpdateUI();
        SaveStats();
    }
    
    public void ResetStats()
    {
        totalWins = 0;
        totalLosses = 0;
        totalRolls = 0;
        UpdateUI();
        SaveStats();
    }
    
    private void UpdateUI()
    {
        winsText.text = $"Wins: {totalWins}";
        lossesText.text = $"Losses: {totalLosses}";
        rollsText.text = $"Total Rolls: {totalRolls}";
        
        float winRate = (totalWins + totalLosses) > 0 ? 
            (float)totalWins / (totalWins + totalLosses) * 100f : 0f;
        winRateText.text = $"Win Rate: {winRate:F1}%";
    }
    
    private void SaveStats()
    {
        PlayerPrefs.SetInt("DiceGame_Wins", totalWins);
        PlayerPrefs.SetInt("DiceGame_Losses", totalLosses);
        PlayerPrefs.SetInt("DiceGame_Rolls", totalRolls);
        PlayerPrefs.Save();
    }
    
    private void LoadStats()
    {
        totalWins = PlayerPrefs.GetInt("DiceGame_Wins", 0);
        totalLosses = PlayerPrefs.GetInt("DiceGame_Losses", 0);
        totalRolls = PlayerPrefs.GetInt("DiceGame_Rolls", 0);
    }
}