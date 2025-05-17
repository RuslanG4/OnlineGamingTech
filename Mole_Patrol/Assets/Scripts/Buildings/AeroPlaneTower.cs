using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AeroPlaneTower : Tower
{
    public GameObject planePrefab; 
    private GameObject spawnedPlane;
    private Vector3 normal;

    public override void Initialise(TowerData data)
    {
        base.Initialise(data); // Make sure base logic runs on both server and client

        if (data.ID.NullID())
        {
            data.ID = TowerData.GetNewTowerID();
        }

        towerData = data;

        currentHealth.Value = data.MaxHealth;
        normal = transform.up;

        if (placed.Value)
        {
            SpawnPlane();
        }
    }

    void SpawnPlane()
    {
        if (!IsServer) return;
        if (spawnedPlane == null)
        {
            GameObject planet = GameObject.Find("Planet"); // Get the planet object
            spawnedPlane = Instantiate(planePrefab, transform.position + normal * 5f, Quaternion.identity);

            // Make the plane a child of the planet instead of the tower 
            // Because in Unity apparently being a child of a parent means you can still move it.
            spawnedPlane.transform.SetParent(planet.transform, true);

            NetworkObject netObj = spawnedPlane.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
            else
            {
                Debug.LogError("Plane prefab must have a NetworkObject component.");
            }

        }

        AeroPlaneUnit planeUnit = spawnedPlane.GetComponent<AeroPlaneUnit>();
        if (planeUnit != null)
        {
            planeUnit.Initialise(this, towerData, normal);
        }
    }


    public override void Update()
    {
        if (!IsServer) return;
        if (!placed.Value) return;

        if (currentHealth.Value <= 0)
        {
            GameManager.Instance.RemoveTowerFromDataList(towerData.ID);
            spawnedPlane.GetComponent<NetworkObject>().Despawn();
            GetComponent<NetworkObject>().Despawn();
            //Destroy(spawnedPlane);
            //Destroy(gameObject);
            SelectionRing.Instance.DeselectTower();
        }
    }
}
