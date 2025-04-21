using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;
namespace Multiplayer.NumberContest
{
    /// <summary>
    /// This class handles the connection and room management
    /// </summary>
    public class Launcher : MonoBehaviourPunCallbacks

    {
        public static Launcher Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.buildIndex == 0) // Assuming the launcher scene is at index 0
            {
                Init();
            }
        }

        // Start is called before the first frame update
        void Init()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            Debug.developerConsoleVisible = true;
        }

        public override void OnConnectedToMaster()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room");
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2 && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("GameScene");
                PhotonNetwork.CurrentRoom.IsVisible = false; // Hide the room from the lobby
                PhotonNetwork.CurrentRoom.IsOpen = false; // Prevent new players from joining the room
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log("Player entered room: " + newPlayer.NickName);
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2 && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("GameScene");
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log("Player left room: " + otherPlayer.NickName);

            // Return to launcher scene immediately regardless of being master client
            StartCoroutine(DisconnectAndReturnToLauncher());
        }

        System.Collections.IEnumerator DisconnectAndReturnToLauncher()
        {
            // Safely disconnect from Photon
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LeaveLobby();

                // Small delay to ensure leaving room completes
                yield return new WaitForSeconds(0.2f);

                PhotonNetwork.Disconnect();
            }

            // Load launcher scene
            SceneManager.LoadScene("launcher");
        }

        // Handle disconnection callback to ensure clean disconnection
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Disconnected from Photon: " + cause.ToString());

            // If we're not already at the launcher scene, load it
            if (SceneManager.GetActiveScene().name != "launcher")
            {
                SceneManager.LoadScene("launcher");
            }
        }


        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            PhotonNetwork.Disconnect(); // Ensure we disconnect when the object is destroyed
        }
    }
}