using System.Collections.Generic;
using UnityEngine;

public class TowerFactory
{
    // Key for storing tower data by type and level.
    public struct TowerKey
    {
        public string TowerType;
        public TowerData.TowerLevel Level;

        public TowerKey(string towerType, TowerData.TowerLevel level)
        {
            TowerType = towerType;
            Level = level;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TowerKey))
                return false;

            TowerKey other = (TowerKey)obj;
            return TowerType.Equals(other.TowerType) && Level.Equals(other.Level);
        }

        public override int GetHashCode()
        {
            return TowerType.GetHashCode() ^ Level.GetHashCode();
        }
    }

    private static Dictionary<TowerKey, TowerData> towerDataCache = new Dictionary<TowerKey, TowerData>();

    // Method to create and store new TowerData if it doesn’t exist
    public static void CreateTowerData(string towerType, TowerData.TowerLevel level, int maxHealth,
        int buyCost, int upgradeCost, int rebuildCost, float fireRate, float damage, float range, Sprite icon, float goldGeneration = 0f, float goldTimeInter = 0f)
    {
        TowerKey key = new TowerKey(towerType, level);
        if (!towerDataCache.ContainsKey(key))
        {
            towerDataCache[key] = new TowerData(towerType, level, buyCost, upgradeCost, rebuildCost, maxHealth, fireRate, damage, range, icon, goldGeneration, goldTimeInter);
        }
    }

    // Method to retrieve an existing TowerData object by tower type and level
    public static TowerData GetTowerData(string towerType, TowerData.TowerLevel level)
    {
        TowerKey key = new TowerKey(towerType, level);
        return towerDataCache.TryGetValue(key, out TowerData data) ? data : null;
    }
}
