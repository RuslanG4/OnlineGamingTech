using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

// Calculates the placement for towers on the planet

using TowerLevel = TowerData.TowerLevel;


public class TowerPlacer : NetworkBehaviour
{
    public PlanetGeneration planetGenerator;
    
    public GameObject buildingPrefab;

    public GameObject placeBuildingEffect;

    public Sprite CannonTowerSprite;
    public Sprite FactoryTowerSprite;
    public Sprite MortarTowerSprite;
    public Sprite SniperTowerSprite;
    public Sprite AeroPlaneSprite;

    private void Awake()
    {
        // Cannon Tower - All-around balanced tower
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelOne, 800, 500, 1000, 350, 1.2f, 7f, 7f, CannonTowerSprite);
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelTwo, 1200, 750, 1500, 525, 1.0f, 15f, 8f, CannonTowerSprite);
        TowerFactory.CreateTowerData("Cannon", TowerLevel.LevelThree, 1800, 1125, 2250, 800, 0.8f, 30f, 10f, CannonTowerSprite);

        // Town Hall (Does not attack) - Economic benefit structure
        TowerFactory.CreateTowerData("TownHall", TowerLevel.LevelOne, 1500, 700, 1400, 700, 0f, 0f, 0f, CannonTowerSprite);
        TowerFactory.CreateTowerData("TownHall", TowerLevel.LevelTwo, 2500, 1050, 2100, 1050, 0f, 0f, 0f, CannonTowerSprite);
        TowerFactory.CreateTowerData("TownHall", TowerLevel.LevelThree, 4000, 1575, 3150, 1575, 0f, 0f, 0f, CannonTowerSprite);

        // Drill (Resource Generation) - Passive income
        TowerFactory.CreateTowerData("Gold Mine", TowerLevel.LevelOne, 600, 1150, 1200, 450, 0f, 0f, 0f, FactoryTowerSprite, 50f, 5f); 
        TowerFactory.CreateTowerData("Gold Mine", TowerLevel.LevelTwo, 900, 900, 1800, 675, 0f, 100f, 0f, FactoryTowerSprite, 100f, 4f);
        TowerFactory.CreateTowerData("Gold Mine", TowerLevel.LevelThree, 1350, 1350, 2700, 1000, 150f, 0f, 0f, FactoryTowerSprite, 150f, 3f);

        // Mortar Tower (Slow but powerful AoE)
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelOne, 450, 1250, 1300, 500, 3.5f, 15f, 11f, MortarTowerSprite);
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelTwo, 675, 975, 1950, 750, 3.0f, 32f, 12f, MortarTowerSprite);
        TowerFactory.CreateTowerData("Mortar", TowerLevel.LevelThree, 1000, 1450, 2900, 1125, 2.5f, 65f, 14f, MortarTowerSprite);

        // Sniper Tower (Long range, high damage)
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelOne, 700, 550, 1400, 525, 2.5f, 20f, 12f, SniperTowerSprite);
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelTwo, 1050, 650, 2100, 800, 2.0f, 30f, 14f, SniperTowerSprite);
        TowerFactory.CreateTowerData("Sniper", TowerLevel.LevelThree, 1575, 1150, 3150, 1200, 1.5f, 50f, 16f, SniperTowerSprite);

        // AeroPlane Tower (Fast attacks, mobile)
        TowerFactory.CreateTowerData("Plane", TowerLevel.LevelOne, 550, 900, 1600, 600, 0.4f, 3f, 12f, AeroPlaneSprite);
        TowerFactory.CreateTowerData("Plane", TowerLevel.LevelTwo, 825, 1200, 2400, 900, 0.3f, 6f, 13f, AeroPlaneSprite);
        TowerFactory.CreateTowerData("Plane", TowerLevel.LevelThree, 1225, 1800, 3600, 1350, 0.2f, 8f, 14f, AeroPlaneSprite);
    }


    private void Start()
    {
        planetGenerator = GameObject.Find("PlanetMaker").GetComponent<PlanetGeneration>();
        placeBuildingEffect = Resources.Load<GameObject>("Prefabs/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 2");

    }
    public bool PlaceBuilding(Vector3 position, string towerType)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("[PlaceBuilding] Attempted to place a building from a client!");
            return false;
        }

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, position.normalized);
        Tower placedTower = SpawnTower(position, rotation, TowerFactory.GetTowerData(towerType, TowerLevel.LevelOne), buildingPrefab);

        if (placedTower == null)
        {
            Debug.LogError("[SERVER] SpawnTower returned null!");
            return false;
        }

        Debug.Log("[SERVER] Tower was successfully placed and initialized");

        if (towerType != "Gold Mine")
        {
            GameManager.Instance.UpdateTowersPlaced();
            GameManager.Instance.TowerDataListUpdate(placedTower.GetTowerData());
        }

        return true;
    }

    private void ClearEnvironment(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Environment"))
            {
                Destroy(child.gameObject);
            }
        }
    }


    private Tower SpawnTower(Vector3 position, Quaternion rotation, TowerData data, GameObject buildingPrefab)
    {
        Debug.Log($"[SERVER] Spawning tower at {position}");

        GameObject building = Instantiate(buildingPrefab, position, rotation);
        building.SetActive(true);

        NetworkObject netObj = building.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[SERVER] No NetworkObject found on tower prefab!");
        }

        netObj.Spawn();
        Debug.Log($"[SERVER] Spawned NetworkObject with ID: {netObj.NetworkObjectId}");

        Tower tower = building.GetComponent<Tower>();
        if (tower == null)
        {
            Debug.LogError("[SERVER] No Tower script found on prefab!");
        }

        tower.placed.Value = true;
        tower.targetable.Value = true;
        tower.Initialise(data);
        UIRPCRelay.Instance.InitialiseTowerClientRpc(netObj.NetworkObjectId, data.TowerType); // Clients get type only

        float addedHeight = 0.55f;
        Vector3 smokeRaisedHeight = transform.forward * addedHeight;
        ParticleRPCRelay.Instance.PlayTowerPlaceParticleClientRpc(position + smokeRaisedHeight);

        return tower;
    }

}