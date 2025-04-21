using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Multiplayer.NumberContest
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;

        private Dictionary<int, int> playerNumbers = new();
        private Dictionary<int, string> playerDecisions = new();
        private Dictionary<int, int> playerChips = new();
        private Dictionary<int, int> playerBets = new();
        private bool roundActive = false;
        private int startingChips = 100;
        private int minimumBet = 5;
        private bool isFirstRound = true;
        private int roundCount = 0;

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
                // Initialize chips for all players
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    playerChips[p.ActorNumber] = startingChips;
                }
                
                StartCoroutine(StartRound());
            }
        }

        IEnumerator StartRound()
        {
            roundCount++;
            Debug.Log($"Starting Round {roundCount}");
            roundActive = true;
            playerNumbers.Clear();
            playerDecisions.Clear();
            playerBets.Clear();

            // Reset UI for all clients
            photonView.RPC("RPC_ResetUI", RpcTarget.All);

            // Send current chip count to all players
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (!playerChips.ContainsKey(p.ActorNumber))
                {
                    playerChips[p.ActorNumber] = startingChips;
                }
                
                photonView.RPC("RPC_UpdateChips", p, playerChips[p.ActorNumber]);
            }

            // Generate and assign random numbers
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                int number = Random.Range(1, 101);
                playerNumbers[p.ActorNumber] = number;
                Debug.Log($"Player {p.ActorNumber} assigned number: {number}");
                photonView.RPC("RPC_AssignNumber", p, number);
            }

            // Extra synchronization step for first round
            if (isFirstRound)
            {
                Debug.Log("First round - ensuring all player numbers are synchronized");
                // Force synchronization of player numbers
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    int number = playerNumbers[p.ActorNumber];
                    photonView.RPC("RPC_SyncPlayerNumber", RpcTarget.All, p.ActorNumber, number);
                }
                
                // Extra delay for first round to ensure sync
                yield return new WaitForSeconds(1.5f);
                isFirstRound = false;
            }

            photonView.RPC("RPC_StartTimer", RpcTarget.All, 10);
            yield return new WaitForSeconds(10f);

            roundActive = false;
            
            // Delay before deciding winner to ensure all decisions are received
            yield return new WaitForSeconds(0.5f);
            
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

        [PunRPC]
        void RPC_SyncPlayerNumber(int actorId, int number)
        {
            if (!playerNumbers.ContainsKey(actorId) || playerNumbers[actorId] != number)
            {
                Debug.Log($"Syncing player {actorId} number to {number}");
                playerNumbers[actorId] = number;
            }
        }

        [PunRPC]
        void RPC_UpdateChips(int chips)
        {
            PlayerController.Local.SetChips(chips);
        }

        public void RegisterDecision(string decision, int betAmount = 0)
        {
            if (!roundActive) return;
            
            // Only accept bets if the decision is "Contest"
            if (decision == "Contest")
            {
                int playerActorId = PhotonNetwork.LocalPlayer.ActorNumber;
                
                // Use minimum bet if specified bet is too low or invalid
                if (betAmount < minimumBet)
                {
                    betAmount = minimumBet;
                }
                
                // Ensure player can't bet more than they have
                int availableChips = playerChips.ContainsKey(playerActorId) ? playerChips[playerActorId] : startingChips;
                betAmount = Mathf.Min(betAmount, availableChips);
                
                photonView.RPC("RPC_RegisterDecision", RpcTarget.MasterClient, playerActorId, decision, betAmount);
            }
            else
            {
                // For fold, no bet is placed
                photonView.RPC("RPC_RegisterDecision", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, decision, 0);
            }
        }

        [PunRPC]
        void RPC_RegisterDecision(int actorId, string decision, int betAmount)
        {
            if (!roundActive) return;
            
            playerDecisions[actorId] = decision;
            
            if (decision == "Contest")
            {
                playerBets[actorId] = betAmount;
                
                // Debug check to ensure player number is set
                if (!playerNumbers.ContainsKey(actorId))
                {
                    Debug.LogError($"Player {actorId} is contesting but has no number assigned!");
                }
                else
                {
                    Debug.Log($"Player {actorId} decided to {decision} with number {playerNumbers[actorId]} and bet {betAmount}");
                }
            }
            else
            {
                Debug.Log($"Player {actorId} decided to {decision}");
            }
        }

        void DecideWinner()
        {
            Debug.Log($"Deciding winner for round {roundCount}");
            
            // Verify all player numbers before proceeding
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                int actorId = p.ActorNumber;
                if (playerDecisions.ContainsKey(actorId) && 
                    playerDecisions[actorId] == "Contest" &&
                    !playerNumbers.ContainsKey(actorId))
                {
                    Debug.LogError($"Missing number for player {actorId} who contested!");
                    // Force a number to avoid crashes
                    playerNumbers[actorId] = Random.Range(1, 101);
                }
            }
            
            List<KeyValuePair<int, int>> contenders = new();
            int totalPlayers = PhotonNetwork.PlayerList.Length;
            int contestedCount = 0;
            int foldedCount = 0;
            int totalPot = 0;

            // Collect all contesting players
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                int actorId = p.ActorNumber;

                if (playerDecisions.ContainsKey(actorId))
                {
                    if (playerDecisions[actorId] == "Contest" && playerNumbers.ContainsKey(actorId))
                    {
                        int playerNum = playerNumbers[actorId];
                        contenders.Add(new KeyValuePair<int, int>(actorId, playerNum));
                        Debug.Log($"CONTEST: Player {actorId} has number {playerNum}");
                        contestedCount++;
                        
                        // Handle bet
                        if (playerBets.ContainsKey(actorId))
                        {
                            int bet = playerBets[actorId];
                            totalPot += bet;
                            
                            if (playerChips.ContainsKey(actorId))
                            {
                                playerChips[actorId] -= bet;
                            }
                        }
                    }
                    else if (playerDecisions[actorId] == "Fold")
                    {
                        foldedCount++;
                        Debug.Log($"FOLD: Player {actorId} folded");
                    }
                }
            }

            string resultMessage;
            int winnerId = -1;
            int winningNumber = -1;

            // Determine winner based on contender count
            if (contenders.Count == 0)
            {
                resultMessage = "<color=#FF5555>No Contestants! Everyone folded!</color>";
                Debug.Log("No contestants - everyone folded");
            }
            else if (contenders.Count == 1)
            {
                winnerId = contenders[0].Key;
                winningNumber = contenders[0].Value;
                resultMessage = $"<color=#55FF55>Player {winnerId} wins by default</color> with <color=#FFFF00>{winningNumber}</color>!";
                Debug.Log($"Only one contestant - Player {winnerId} wins by default with number {winningNumber}");
            }
            else
            {
                // Find player with highest number directly
                KeyValuePair<int, int> highestPair = contenders[0];
                
                Debug.Log("Multiple contestants - comparing numbers:");
                foreach (var pair in contenders)
                {
                    Debug.Log($"Player {pair.Key} has number {pair.Value}");
                    if (pair.Value > highestPair.Value)
                    {
                        highestPair = pair;
                        Debug.Log($"New highest: Player {pair.Key} with number {pair.Value}");
                    }
                }
                
                winnerId = highestPair.Key;
                winningNumber = highestPair.Value;
                resultMessage = $"<color=#55FF55>Player {winnerId} wins</color> with <color=#FFFF00>{winningNumber}</color>!";
                Debug.Log($"Winner is Player {winnerId} with number {winningNumber}");
            }
            
            // Award pot to winner if there is one
            if (winnerId != -1 && playerChips.ContainsKey(winnerId))
            {
                playerChips[winnerId] += totalPot;
                Debug.Log($"Awarding {totalPot} chips to Player {winnerId}");
            }
            
            // Add pot info to result message
            if (totalPot > 0)
            {
                resultMessage += $"\n<color=#FFD700>Total Pot: {totalPot} chips</color>";
                
                if (winnerId != -1)
                {
                    resultMessage += $" → <color=#55FF55>Player {winnerId}</color>";
                }
            }

            resultMessage += $"\n<color=#87CEFA>Players: {totalPlayers}</color>, <color=#00FF00>Contested: {contestedCount}</color>, <color=#FF7F50>Folded: {foldedCount}</color>, <color=#AAAAAA>No Decision: {totalPlayers - contestedCount - foldedCount}</color>";

            // Send result to all players
            photonView.RPC("RPC_DisplayResult", RpcTarget.All, resultMessage);
            
            // Update chips for all players
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                int chips = playerChips.ContainsKey(p.ActorNumber) ? playerChips[p.ActorNumber] : startingChips;
                photonView.RPC("RPC_UpdateChips", p, chips);
            }

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
}