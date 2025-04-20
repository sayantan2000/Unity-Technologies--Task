using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private Dictionary<int, int> playerNumbers = new();
    private Dictionary<int, string> playerDecisions = new();
    private bool roundActive = false;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private string playerPrefabName = "Player"; // must match prefab name in Resources

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Instantiate(playerPrefabName, Vector3.zero, Quaternion.identity);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(StartRound());
        }
    }

    IEnumerator StartRound()
    {
        roundActive = true;
        playerNumbers.Clear();
        playerDecisions.Clear();

        // Reset UI for all clients
        photonView.RPC("RPC_ResetUI", RpcTarget.All);

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            int number = Random.Range(1, 101);
            playerNumbers[p.ActorNumber] = number;
            Debug.Log("Player " + p.ActorNumber + " assigned number: " + number);
            photonView.RPC("RPC_AssignNumber", p, number);
        }

        photonView.RPC("RPC_StartTimer", RpcTarget.All, 10);
        yield return new WaitForSeconds(10f);

        roundActive = false;
        DecideWinner();
    }

    [PunRPC]
    void RPC_ResetUI()
    {
        UIManager.Instance.ResetUI();
    }

    [PunRPC]
    void RPC_StartTimer(int duration)
    {
        UIManager.Instance.ShowTimer(duration);
    }

    [PunRPC]
    void RPC_AssignNumber(int num)
    {
        PlayerController.Local.SetNumber(num);
    }

    public void RegisterDecision(string decision)
    {
        if (!roundActive) return;
        photonView.RPC("RPC_RegisterDecision", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, decision);
    }

    [PunRPC]
    void RPC_RegisterDecision(int actorId, string decision)
    {
        if (!roundActive) return;
        playerDecisions[actorId] = decision;
        Debug.Log($"Player {actorId} decided to {decision}");
    }

    void DecideWinner()
    {
        List<KeyValuePair<int, int>> contenders = new();
        int totalPlayers = PhotonNetwork.PlayerList.Length;
        int contestedCount = 0;
        int foldedCount = 0;

        // Check all players who made a decision
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            int actorId = p.ActorNumber;

            if (playerDecisions.ContainsKey(actorId))
            {
                if (playerDecisions[actorId] == "Contest" && playerNumbers.ContainsKey(actorId))
                {
                    contenders.Add(new(actorId, playerNumbers[actorId]));
                    contestedCount++;
                }
                else if (playerDecisions[actorId] == "Fold")
                {
                    foldedCount++;
                }
            }
        }

        string resultMessage;

        if (contenders.Count == 0)
        {
            resultMessage = "<color=#FF5555>No Contestants! Everyone folded!</color>";
        }
        else if (contenders.Count == 1)
        {
            resultMessage = $"<color=#55FF55>Player {contenders[0].Key} wins by default</color> with <color=#FFFF00>{contenders[0].Value}</color>!";
        }
        else
        {
            contenders.Sort((a, b) => b.Value.CompareTo(a.Value));
            resultMessage = $"<color=#55FF55>Player {contenders[0].Key} wins</color> with <color=#FFFF00>{contenders[0].Value}</color>!";
        }

        resultMessage += $"\n<color=#87CEFA>Players: {totalPlayers}</color>, <color=#00FF00>Contested: {contestedCount}</color>, <color=#FF7F50>Folded: {foldedCount}</color>, <color=#AAAAAA>No Decision: {totalPlayers - contestedCount - foldedCount}</color>";

        // Send result to all players
        photonView.RPC("RPC_DisplayResult", RpcTarget.All, resultMessage);

        StartCoroutine(NextRoundDelay());
    }

    [PunRPC]
    void RPC_DisplayResult(string resultMessage)
    {
        UIManager.Instance.DisplayResult(resultMessage);
    }

    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(5);
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(StartRound());
    }
}