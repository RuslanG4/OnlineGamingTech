using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInformationHandler : MonoBehaviour
{
    private TextMeshProUGUI buildingName;
    private Slider healthSlider;
    private Image towerImage;
    private GameObject costContainer;
    private GameObject upgradeContainer;
    private GameObject coinImageInUpgradeContainer;
    private int rebuildCostValue;
    private int upgradeCostValue;
    private Tower currentTower;
    private bool canRepair = false;
    private bool canUpgrade = false;
    private float xTextOffsetAfterRemovingCoin;
    private Vector2 baseAnchorPositionOfUpgradeText;

    private void Awake()
    {
        buildingName = transform.Find("TowerImage").GetComponentInChildren<TextMeshProUGUI>();
        costContainer = transform.Find("CostButton/CostContainer").gameObject;
        upgradeContainer = transform.Find("CostButton (1)/CostContainer").gameObject;
        coinImageInUpgradeContainer = transform.Find("CostButton (1)/CostContainer/CoinIcon").gameObject;
        healthSlider = GetComponentInChildren<Slider>();
        towerImage = transform.Find("TowerImage/Background/TowerImage").GetComponentInChildren<Image>();

        xTextOffsetAfterRemovingCoin = coinImageInUpgradeContainer.GetComponent<RectTransform>().rect.width;
        TextMeshProUGUI textComponent = upgradeContainer.GetComponentInChildren<TextMeshProUGUI>();
        RectTransform textRectTransform = textComponent.GetComponent<RectTransform>();
        baseAnchorPositionOfUpgradeText = textRectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        TowerSelectionManager.OnNewTowerSelected += SelectNewTower; // Listen for tower selection
        Tower.onTowerSelected += UpdateUI; // Listen for health updates
    }

    private void OnDisable()
    {
        TowerSelectionManager.OnNewTowerSelected -= SelectNewTower;
        Tower.onTowerSelected -= UpdateUI;
    }

    private void SelectNewTower(Tower towerObject)
    {
        if (towerObject == null)
        {
            Debug.Log("Tower object is null");
            return;

        }

        currentTower = towerObject;
        TowerData towerData = currentTower.GetTowerData();

        if(towerData == null)
        {
            Debug.Log("Tower data is null");
            return;
        }

        // Update UI Elements (Static Information)
        buildingName.text = towerData.TowerType;
        rebuildCostValue = towerData.RebuildCost;
        upgradeCostValue = towerData.UpgradeCost;
        costContainer.GetComponentInChildren<TextMeshProUGUI>().text = rebuildCostValue.ToString();
        towerImage.sprite = towerData.Icon;

        UpdateUI(); //Refresh UI after selecting a new tower
    }

    private void UpdateTowerRepairCost()
    {
        int currentHealth = currentTower.GetCurrentHealth();
        int maxHealth = currentTower.GetTowerData().MaxHealth;

        if(currentTower.GetTowerData().TowerType == "Gold Mine" && currentTower.GetComponent<Tower>().broken.Value)
        {
            rebuildCostValue = currentTower.GetTowerData().MaxHealth;
        }
        else
        {
            rebuildCostValue = maxHealth - currentHealth;
            if (rebuildCostValue > GameManager.Instance.GetCoinCount())
            {
                rebuildCostValue = GameManager.Instance.GetCoinCount();
            }
        }

        costContainer.GetComponentInChildren<TextMeshProUGUI>().text = rebuildCostValue.ToString();

        int cointCount = GameManager.Instance.GetCoinCount();
        if (cointCount < rebuildCostValue)
        {
            canRepair = false;
            costContainer.GetComponent<Image>().color = Color.red;

        }
        else
        {
            canRepair = true;
            costContainer.GetComponent<Image>().color = Color.green;
        }
    }

    private void UpdateTowerUpgradeCost()
    {
        TowerData towerData = currentTower.GetTowerData();

        if (towerData.Level != TowerData.TowerLevel.LevelThree)
        {
            string upgradeString = towerData.UpgradeCost.ToString();
            upgradeContainer.GetComponentInChildren<TextMeshProUGUI>().text = upgradeString;
            TextMeshProUGUI textComponent = upgradeContainer.GetComponentInChildren<TextMeshProUGUI>();
            coinImageInUpgradeContainer.gameObject.SetActive(true);
            RectTransform textRectTransform = textComponent.GetComponent<RectTransform>();
            textRectTransform.anchoredPosition = baseAnchorPositionOfUpgradeText; // Reset text's x position if another tower moved it
        }
        else
        {
            TextMeshProUGUI textComponent = upgradeContainer.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = "Max";

            RectTransform textRectTransform = textComponent.GetComponent<RectTransform>();
            coinImageInUpgradeContainer.gameObject.SetActive(false); // Hide the coin image
            textRectTransform.anchoredPosition = baseAnchorPositionOfUpgradeText; // Reset text's x position if another tower moved it
            textRectTransform.anchoredPosition += new Vector2(-xTextOffsetAfterRemovingCoin / 2, 0); // Move text to the left after removing the coin
        }



        int cointCount = GameManager.Instance.GetCoinCount();
        if (cointCount < upgradeCostValue)
        {
            canUpgrade = false;
            upgradeContainer.GetComponent<Image>().color = Color.red;

        }
        else
        {
            canUpgrade = true;
            upgradeContainer.GetComponent<Image>().color = Color.green;
        }
    }

    public void UpdateUI()
    {
        if (currentTower == null) return; // Only update if this is the selected tower

        int currentHealth;
        if (currentTower.GetCurrentHealth() < 0)
        {
            currentHealth = 0;
        }
        else
        {
            currentHealth = currentTower.GetCurrentHealth();
        }
        int maxHealth = currentTower.GetTowerData().MaxHealth;
        healthSlider.value = (float)currentHealth / maxHealth;
        healthSlider.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = $"{currentHealth}/{maxHealth}";

        UpdateTowerRepairCost();
        UpdateTowerUpgradeCost();
    }

    public void RepairBuilding()
    {
        if (currentTower == null) return;
       

        if(canRepair && rebuildCostValue != 0)
        {
            currentTower.RepairTower(rebuildCostValue);
            GameManager.Instance.UpdateCoinCount(-rebuildCostValue);
            UpdateUI();
        }
    }

    public void UpgradeBuilding()
    {
        if (currentTower == null) return;

        if (canUpgrade && upgradeCostValue != 0)
        {
            TowerData towerData = currentTower.GetTowerData();
            if(towerData.Level != TowerData.TowerLevel.LevelThree && !currentTower.GetComponent<Tower>().broken.Value)
            {
                Debug.Log(currentTower.GetComponent<Tower>().GetTowerData().ID.Value);
                GameManager.Instance.RemoveTowerFromDataList(currentTower.GetComponent<Tower>().GetTowerData().ID);

                TowerData.TowerLevel newLevel = TowerData.TowerLevelUtils.IncrementTowerLevel(towerData.Level);
                TowerData upgradedTowerData = TowerFactory.GetTowerData(buildingName.text, newLevel);
                int healthBeforeUpgrade = currentTower.GetCurrentHealth();
                currentTower.Initialise(upgradedTowerData); // Set new tower data
                currentTower.SetCurrentHealth(healthBeforeUpgrade);
                TowerData newTowerData = currentTower.GetTowerData() as TowerData;
                GameManager.Instance.TowerDataListUpdate(newTowerData);
                GameManager.Instance.UpdateCoinCount(-upgradeCostValue);

                UpdateUI();
            }
            
        }
    }
}
