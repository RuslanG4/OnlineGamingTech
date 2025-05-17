using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Qos.V2.Models;
using UnityEngine;

// Base Building Class
public abstract class Tower : NetworkBehaviour
{
    public NetworkVariable<float> range = new NetworkVariable<float>(5f); 
    public Transform firePoint; 
    protected TowerData towerData; 
    public NetworkVariable<bool> placed = new NetworkVariable<bool>(false); 
    public NetworkVariable<bool> targetable = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> broken = new NetworkVariable<bool>(false); 

    protected NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public static event Action onTowerSelected;

    public bool isInitialised = false;
    public Action<Tower> OnInitialised;

    public List<Enemy> Attackers = new List<Enemy>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Tower OnNetworkSpawn called.");
    }

    public virtual void Update() { }

    public virtual void Initialise(TowerData data)
    {
        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Tower.Initialise called with data: {data?.TowerType}");

        if (data == null)
        {
            Debug.LogError("Initialise called with NULL data.");
            return;
        }

        this.towerData = data;
        isInitialised = true;
        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Tower data initialised");

        OnInitialised?.Invoke(this);
        OnInitialised = null;

        if (!IsServer) return;
        placed.Value = true;
        targetable.Value = true;
    }


    public TowerData GetTowerData() { return towerData; }

    public void TakeDamage(int _amount)
    {
        if (IsServer) // Ensure only the server handles health updates
        {
            currentHealth.Value -= _amount;
            if (currentHealth.Value < 0)
            {
                DestoryTower();
            }
            onTowerSelected?.Invoke();
        }
    }

    public virtual void DestoryTower()
    {
        if (IsServer) // Only the server should destroy the tower
        {
            broken.Value = true;
            targetable.Value = false;
        }
    }

    public virtual void RepairTower(int _amount)
    {
        if (IsServer) // Only the server should repair the tower
        {
            currentHealth.Value += _amount;
            if (currentHealth.Value >= towerData.MaxHealth)
            {
                currentHealth.Value = towerData.MaxHealth;
            }
        }
    }

    public int GetCurrentHealth() { return currentHealth.Value; }
    public void SetCurrentHealth(int newHealth) { currentHealth.Value = newHealth; }
}

