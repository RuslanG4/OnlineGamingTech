using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameplayMultiplayerManager : NetworkBehaviour
{
    public static GameplayMultiplayerManager Instance { get; private set; }

    public GameObject CannonPrefab;
    public GameObject SniperPrefab;
    public GameObject MortarPrefab;
    public GameObject PlanePrefab;

    //Used for spawning Knight icons in lobby
    [SerializeField] private GameObject lobbyUIRpcHandlerPrefab;
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

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlaceCannonServerRpc(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"[SERVER] Received tower placement request at {position}");

        if (!IsServer)
        {
            Debug.LogError("RequestPlaceTowerServerRpc called on a non-server instance!");
            return;
        }

        TowerPlacer buildingPlacer = FindObjectOfType<TowerPlacer>();
        buildingPlacer.buildingPrefab = CannonPrefab;
        bool success = buildingPlacer.PlaceBuilding(position, "Cannon");

        Debug.Log($"[SERVER] Tower placement success: {success}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlaceMortarServerRpc(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"[SERVER] Received tower placement request at {position}");

        if (!IsServer)
        {
            Debug.LogError("RequestPlaceTowerServerRpc called on a non-server instance!");
            return;
        }

        TowerPlacer buildingPlacer = FindObjectOfType<TowerPlacer>();
        buildingPlacer.buildingPrefab = MortarPrefab;
        bool success = buildingPlacer.PlaceBuilding(position, "Mortar");

        Debug.Log($"[SERVER] Tower placement success: {success}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlaceSniperServerRpc(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"[SERVER] Received tower placement request at {position}");

        if (!IsServer)
        {
            Debug.LogError("RequestPlaceTowerServerRpc called on a non-server instance!");
            return;
        }

        TowerPlacer buildingPlacer = FindObjectOfType<TowerPlacer>();
        buildingPlacer.buildingPrefab = SniperPrefab;
        bool success = buildingPlacer.PlaceBuilding(position, "Sniper");

        Debug.Log($"[SERVER] Tower placement success: {success}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlacePlaneServerRpc(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"[SERVER] Received tower placement request at {position}");

        if (!IsServer)
        {
            Debug.LogError("RequestPlaceTowerServerRpc called on a non-server instance!");
            return;
        }

        TowerPlacer buildingPlacer = FindObjectOfType<TowerPlacer>();
        buildingPlacer.buildingPrefab = PlanePrefab;
        bool success = buildingPlacer.PlaceBuilding(position, "Plane");

        Debug.Log($"[SERVER] Tower placement success: {success}");
    }


    //[ServerRpc(RequireOwnership = false)]
    //public void RequestEnemySpawnServerRpc(string name, Vector3 position, Quaternion rotation)
    //{
    //    if (!IsServer)
    //    {
    //        Debug.LogError("RequestEnemySpawnServerRpc called on a non-server instance!");
    //        return;
    //    }

    //    EnemySpawner buildingPlacer = FindObjectOfType<EnemySpawner>();
    //    buildingPlacer.SpawnEnemy(name, position, rotation);

    //    Debug.Log($"[SERVER] enemy spawn success");
    //}

}
