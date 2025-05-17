using UnityEngine;

public class TowerData
{
    public struct TowerID
    {
        public int Value;
        
        public TowerID(int value)
        {
            Value = value;
        }

        public static TowerID Default => new TowerID(-1);

        public bool NullID()
        {
            return Value == -1;
        }

        public int AsInt()
        {
            return Value;
        }
    }

    static int newTowerID = -1; // Static increment so we get a new id everytime
    public static TowerID GetNewTowerID()
    {
        newTowerID++;
        return new TowerID(newTowerID);
    }

    public enum TowerLevel
    {
        LevelOne,
        LevelTwo,
        LevelThree
    }


    public static class TowerLevelUtils
    {
        public static TowerData.TowerLevel IncrementTowerLevel(TowerData.TowerLevel currentLevel)
        {
            switch (currentLevel)
            {
                case TowerData.TowerLevel.LevelOne:
                    return TowerData.TowerLevel.LevelTwo;
                case TowerData.TowerLevel.LevelTwo:
                    return TowerData.TowerLevel.LevelThree;
                case TowerData.TowerLevel.LevelThree:
                default:
                    return TowerData.TowerLevel.LevelThree;
            }
        }
    }

    public string TowerType { get; private set; }
    public TowerID ID { get; set; }
    public int Cost { get; private set; }
    public int MaxHealth { get; private set; }
    public int RebuildCost { get; private set; } // Keep this incase we want to add a multiplier to rebuild cost and not just missing health = gold cost
    public int UpgradeCost { get; private set; }
    public float FireRate { get; private set; }
    public float Damage { get; private set; }
    public float Range { get; private set; }
    public Sprite Icon { get; private set; }
    public TowerLevel Level { get; private set; }
    public float? GoldGeneration { get; private set; }

    public float? GoldTimeInterval { get; private set; }


    public TowerData(string towerType, TowerLevel level, int buyCost, int upgradeCost,
        int rebuildCost, int maxHealth, float fireRate, float damage, float range, Sprite icon, float goldGeneration = 0f, float goldTimeInter = 0f)
    {
        TowerType = towerType;
        ID = TowerID.Default;
        Level = level;
        Cost = buyCost;
        RebuildCost = rebuildCost;
        UpgradeCost = upgradeCost;
        FireRate = fireRate;
        Damage = damage;
        Range = range;
        MaxHealth = maxHealth;

        GoldGeneration = goldGeneration;
        GoldTimeInterval = goldTimeInter;
        Icon = icon;
    }
}
