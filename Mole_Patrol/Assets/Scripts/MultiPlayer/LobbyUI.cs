using Unity.Netcode;
using UnityEngine;
public class LobbyUIRpcHandler : NetworkBehaviour
{
    [ClientRpc]
    public void SyncAllClientIconsClientRpc(int playerCount, ulong excludedClientId, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId == excludedClientId) return;
            
        MainMenuController.Instance.AddNewPlayer(playerCount);
    }

    [ClientRpc]
    public void SyncNewClientIconClientRpc(int playerCount, ClientRpcParams rpcParams = default)
    {
        MainMenuController.Instance.AddNewPlayer(playerCount);
    }

    [ClientRpc]
    public void SyncRemovePlayerIconClientRpc(int playerIndex, ClientRpcParams rpcParams = default)
    {
        MainMenuController.Instance.RemovePlayerIcon(playerIndex);
    }

}
