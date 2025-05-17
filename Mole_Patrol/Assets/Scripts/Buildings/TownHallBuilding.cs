using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Unity.Netcode; // Make sure to include the Netcode namespace

public class TownHallTower : Tower
{
    public override void Initialise(TowerData data)
    {
        base.Initialise(data); // Required for shared (especially client-side) initialization

        if (data.ID.NullID())
        {
            data.ID = TowerData.GetNewTowerID();
        }

        towerData = data;
        currentHealth.Value = data.MaxHealth; 
    }

    public override void Update()
    {
        if (IsServer && currentHealth.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
            //Destroy(gameObject); 
            SelectionRing.Instance.DeselectTower();
            GameManager.Instance.GameOver(); 
            GamePlayUIController.Instance.OpenGameLose();
        }
    }
}

