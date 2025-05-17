using System.Collections.Generic;
using System;
using UnityEngine;


[Serializable]
public class WaveData
{
    public List<EnemyWave> waves;
}

[Serializable]
public class EnemyWave
{
    public int waveNumber;
    public int spawnPoint;
    public List<EnemyData> enemies;
}

[Serializable]
public class EnemyData
{
    public string enemyType;
    public int numOfEnemies;
}
