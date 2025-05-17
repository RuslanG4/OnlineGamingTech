using System;
using UnityEngine;

public class TowerSelectionManager : MonoBehaviour
{
    public static event Action<Tower> OnNewTowerSelected; 
    private static Tower selectedTower;

    public static void SelectTower(Tower tower)
    {
        if (tower == null || tower == selectedTower) return;

        Debug.Log("SELECTED");
        selectedTower = tower;
        OnNewTowerSelected?.Invoke(tower); 
    }
}
