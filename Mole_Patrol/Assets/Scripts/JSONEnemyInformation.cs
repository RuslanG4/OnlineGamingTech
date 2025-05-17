using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JSONEnemyInformation : MonoBehaviour
{
    [System.Serializable]
    public class EnemiesData
    {
        public List<AllEnemyData> enemies; // Matches JSON structure
    }

    [System.Serializable]
    public class AllEnemyData
    {
        public string name;
        public EnemyStats stats;
    }

    [System.Serializable]
    public class EnemyStats
    {
        public int maxDamage;
        public float maxSpeed;
        public int maxHealth;
        public int goldGainOnDeath;
        public int spawnInterval;
    }

    private static Dictionary<string, EnemyStats> enemyCache; // Cached data
    public static void LoadData()
    {
        if (enemyCache != null) return; // Already loaded

        TextAsset jsonText = Resources.Load<TextAsset>("enemyData");

        if (jsonText == null)
        {
            Debug.LogError("Failed to load enemyData.json from Resources.");
            return;
        }

        EnemiesData enemyData = JsonUtility.FromJson<EnemiesData>(jsonText.text);

        if (enemyData == null || enemyData.enemies == null)
        {
            Debug.LogError("EnemyStatData is null or improperly formatted.");
            return;
        }

        // Convert list into dictionary for faster lookup
        enemyCache = enemyData.enemies.ToDictionary(e => e.name, e => e.stats);

        Debug.Log("Enemy data successfully loaded and cached.");
    }
    public static EnemyStats GetEnemyStats(string enemyName)
    {
        if (enemyCache == null)
        {
            Debug.LogError("Enemy data not loaded! Call LoadData() first.");
            return null;
        }

        if (enemyCache.TryGetValue(enemyName, out EnemyStats stats))
        {
            return stats;
        }
        else
        {
            Debug.LogError($"Enemy '{enemyName}' not found in cached data.");
            return null;
        }
    }
}

