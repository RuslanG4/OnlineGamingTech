using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;

public class MultiPlayerLobby : NetworkBehaviour
{
    public static MultiPlayerLobby Instance { get; private set; }

    // number of players in game
    public NetworkVariable<int> numOfPlayers = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private HashSet<ulong> connectedClients = new HashSet<ulong>();

    //Used for spawning Knight icons in lobby
    [SerializeField] private GameObject lobbyUIRpcHandlerPrefab;
    private LobbyUIRpcHandler spawnedHandler;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            //handles client connection
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected; 
            //handles client disconnect 
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            //handles host disconnect
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnectedFromHost;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnDisconnectedFromHost;
        }
    }

    public async void HostGame()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join Code: {joinCode}");

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "udp");

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            await Task.Delay(100); // tiny delay ensures IsServer is true

            if (NetworkManager.Singleton.IsServer)
            {
                if (spawnedHandler == null)
                {
                    GameObject handlerInstance = Instantiate(lobbyUIRpcHandlerPrefab);
                    spawnedHandler = handlerInstance.GetComponent<LobbyUIRpcHandler>();
                    handlerInstance.GetComponent<NetworkObject>().Spawn(); //Spawns object across network
                }

                ulong hostClientId = NetworkManager.Singleton.LocalClientId;

                if (!connectedClients.Contains(hostClientId)) //add yourself as host to game
                {
                    connectedClients.Add(hostClientId);
                    numOfPlayers.Value++;
                }

                MainMenuController.Instance.AddNewPlayer(0); //add new player icon

            }

            MainMenuController.Instance.OpenLobby(joinCode); //UI

        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Error: {e.Message}");
        }
    }

    public async void JoinGame(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Successfully joined allocation via code: " + joinCode);

            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "udp");

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            MainMenuController.Instance.OpenLobbyJoined(joinCode); //UI
           
        }
        catch (RelayServiceException e)
        {
            MainMenuController.Instance.DisplayLobbyError();
            Debug.LogError($"Relay Join Error: {e.Message}");
        }
    }

    public void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log($"Host detected client {clientId} joining.");

        if (connectedClients.Contains(clientId))
        {
            Debug.LogWarning($"Client {clientId} has already connected. Skipping.");
            return;
        }

        connectedClients.Add(clientId);
        numOfPlayers.Value++;

        if (spawnedHandler != null)
        {
            int playerIndex = 0;
            foreach (ulong currentClientId in connectedClients)
            {
                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { clientId } }
                };
                spawnedHandler.SyncNewClientIconClientRpc(playerIndex, rpcParams);
                playerIndex++;
            }

            int newPlayerIndex = connectedClients.Count - 1;
            foreach (var existingClientId in connectedClients)
            {
                if (existingClientId == clientId)
                    continue;

                var rpcParamsForOthers = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { existingClientId } }
                };

                spawnedHandler.SyncNewClientIconClientRpc(newPlayerIndex, rpcParamsForOthers);
            }
        }
    }

    public void DisconnectClient()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void OnDisconnectedFromHost(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Disconnected from host — lobby closed.");
            MainMenuController.Instance.BackButton();
        }
    }

    public void HandleClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log($"Host detected client {clientId} leaving.");

        if (!connectedClients.Contains(clientId))
        {
            Debug.LogWarning($"Client {clientId} was not found in the connected clients list.");
            return;
        }
        connectedClients.Remove(clientId);
        numOfPlayers.Value--;

        int playerIndex = 0;
        foreach (ulong currentClientId in connectedClients)
        {
            if (currentClientId == clientId)
                break;

            playerIndex++;
        }

        if (spawnedHandler != null)
        {
            foreach (ulong currentClientId in connectedClients)
            {
                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { currentClientId } }
                };

                spawnedHandler.SyncRemovePlayerIconClientRpc(playerIndex, rpcParams);
            }
        }
    }
}
