using Unity.Netcode;
using UnityEngine;

public class UIRPCRelay : NetworkBehaviour
{
    public static UIRPCRelay Instance;

    private void Awake()
    {
        Instance = this;
    }

    [ClientRpc]
    public void roundClientRpc(int round)
    {
        UpdateWaveCount.Instance.UpdateWaveText(round);
    }

    [ClientRpc]
    public void InitialiseTowerClientRpc(ulong towerNetworkObjectId, string towerType)
    {
        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] InitialiseTowerClientRpc called.");
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(towerNetworkObjectId, out NetworkObject netObj))
        {
            Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Network obejct exists");

            Tower tower = netObj.GetComponent<Tower>();
            if (tower != null)
            {
                Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Tower from object is not null");
                TowerData data = TowerFactory.GetTowerData(towerType, TowerData.TowerLevel.LevelOne); // Your local lookup
                if (data != null)
                {
                    Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] About to call Initialise with {data?.TowerType}");
                    tower.Initialise(data);
                }
            }
            else
            {
                Debug.LogError("Tower component not found on network object.");
            }
        }
        else
        {
            Debug.LogError($"Tower NetworkObject with ID {towerNetworkObjectId} not found.");
        }
    }

}
