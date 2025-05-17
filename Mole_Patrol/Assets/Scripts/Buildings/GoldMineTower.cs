using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;

public class GoldMineTower : Tower
{
    public UnityEvent onGoldCollected;

    public GameObject coinPopUp;
    public Transform spawnPoint;

    private float? spawnInterval;

    private bool brokenReset = false;

    public override void Initialise(TowerData data)
    {
        base.Initialise(data); // 🔑 Required for shared (especially client-side) initialization

        if (data.ID.NullID())
        {
            data.ID = TowerData.GetNewTowerID();
        }

        towerData = data;
        currentHealth.Value = 0;
        targetable.Value = false;
        spawnInterval = towerData.GoldTimeInterval;
    }


    public override void DestoryTower()
    {
        targetable.Value = false;
        StopCoroutine(GenerateResources());
        gameObject.GetComponent<Animator>().Play("MineBroken");
        Destroy(gameObject.GetComponentInChildren<Coin>().gameObject);
        GameManager.Instance.RemoveTowerFromDataList(towerData.ID);
        GameManager.Instance.DecrementGoldMinesNumber();
        broken.Value = true;
    }
    public override void Update()
    {
    }

    public override void RepairTower(int _amount)
    {
        currentHealth.Value += _amount;
        if (currentHealth.Value >= towerData.MaxHealth)
        {
            currentHealth.Value = towerData.MaxHealth;
        }
        GameManager.Instance.UpdateTowersPlaced();
        GameManager.Instance.TowerDataListUpdate(towerData);
        targetable.Value = true;
        if (broken.Value)
        {
            gameObject.GetComponent<Animator>().Play("Mine");
            StartCoroutine(GenerateResources());
            GameManager.Instance.currentGoldMineCount++;
            Reset();
        }
        broken.Value = false;
    }

    public void Reset()
    {
        if (!broken.Value)
        {
            StartCoroutine(GenerateResources());
            Debug.Log(GameManager.Instance.currentGoldMineCount);
        }
    }

    public IEnumerator GenerateResources()
    {
        yield return new WaitForSeconds((float)spawnInterval);
        Instantiate(coinPopUp, spawnPoint.position, Quaternion.identity, spawnPoint.parent.transform);
        GameManager.Instance.totalGoldMadeFromGoldMines += (float)GetTowerData().GoldGeneration;
    }

}

