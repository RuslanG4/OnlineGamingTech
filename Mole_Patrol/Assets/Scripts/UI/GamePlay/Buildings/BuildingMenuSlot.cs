using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenuSlot : MonoBehaviour
{
    // Start is called before the first frame update

    private TextMeshProUGUI cost;
    private Image background;
    private Image TowerSprite;

    public string buildingName = "";

    private int costForBuilding;
    void Start()
    {
        cost = GetComponentInChildren<TextMeshProUGUI>();
        TowerSprite = transform.Find("TowerBackground").transform.Find("TowerImage").GetComponent<Image>();
        background =transform.Find("TowerBackground").GetComponent<Image>();

        costForBuilding = TowerFactory.GetTowerData(buildingName, TowerData.TowerLevel.LevelOne).Cost;
        TowerSprite.sprite = TowerFactory.GetTowerData(buildingName, TowerData.TowerLevel.LevelOne).Icon;

        cost.text = costForBuilding.ToString();
    }

    public void BuyBuilding()
    {
        GameManager.Instance.UpdateCoinCount(-costForBuilding);
        GameObject.FindFirstObjectByType<BuildingInformationHandler>().UpdateUI();
    }

    public bool CanAfford()
    {
        return GameManager.Instance.GetCoinCount() - costForBuilding > 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(!CanAfford())
        {
            cost.color = Color.red;
            background.color = Color.red;
        }
        else
        {
            cost.color = Color.white;
            background.color = Color.yellow;
        }
    }
}
