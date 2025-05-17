using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static JSONEnemyInformation;
using static UnityEngine.EventSystems.EventTrigger;
using static WaveGenerator;

public class WaveAnyliticData 
{
    public List<float> timeLastedPerWave = new List<float>();
    public List<List<EnemyData>> enemyWaveData = new List<List<EnemyData>>();

}

public class EnemySpawner : MonoBehaviour
{

    public WaveAnyliticData waveAnyliticData = new WaveAnyliticData();   
    public Queue<int > levelLock = new Queue<int>();

    public List<GameObject> enemies;
    public List<GameObject> enemiesSpawned;

    public int m_timeBetweenSpawns = 1;

    int prevWave = 0;
    int currentWave = 0;
    public GameObject planetTransform;
    

    private List<GameObject> currentEnemyPool;

    private bool beganSpawning = false;

    public WaveData waveData = null;

    GameObject planet;

    public GameObject moleHolePrefab;
    public static int BaseWaveGoldGain = 300;
    public static int WaveEndGoldGain = 75;

    List<GameObject> spawnLocations = new List<GameObject>();


    float waveDuration = 0;

    [Header("Enemies")]
    public GameObject mediunOne;
    public GameObject mediumTwo;
    public GameObject mediumThree;

    public GameObject heavyOne;

    private int maxWave = 0;

    void Start()
    {
        waveData = WaveGenerator.LoadWavesFromJson();

        planet = GameObject.Find("Planet");

        currentWave = 1;
    }

    public List<float> GetWaveDurationTimes()
    {
        return waveAnyliticData.timeLastedPerWave;
    }

    public int getCurrentWave()
    {
        return currentWave;
    }   

    public void selectDifficulty()
    {
        switch(GameManager.Instance.difficulty){
            case Difficulty.Easy:
                maxWave = 20;
                break;
            case Difficulty.Medium:
                maxWave = 40;
                break;
            case Difficulty.Hard:
                maxWave = 60;
                break;
        }
    }
    
    public void startSpawning()
    {
        if( !beganSpawning )
        {
            beganSpawning =true;
            selectDifficulty();
            StartCoroutine(StartWave(currentWave));
        }
    }

    public IEnumerator LogWaveDuration()
    {
        while (true)
        {
            waveDuration += Time.deltaTime;
            yield return null;
        }
    }


    public IEnumerator StartWave(int waveNumber)
    {
        EnemyWave wave = waveData.waves.Find(w => w.waveNumber == waveNumber);

        spawnLocations.Clear();

        //Generate mole holes in random locations
        for (int i = 0; i < wave.spawnPoint; i++)
        {
            GameObject centroid = getRandomCentroid2();
            GameObject moleHole = Instantiate(moleHolePrefab, centroid.transform.position, centroid.transform.rotation, centroid.transform);
            moleHole.GetComponent<NetworkObject>().Spawn();
            spawnLocations.Add(moleHole);
        }

        StartCoroutine(SpawnEnemiesWithDelay(wave));
        StartCoroutine(LogWaveDuration());

        yield return new WaitUntil(() => roundOver());

        foreach (var enemy in wave.enemies)
        {
            // add new list to data
            waveAnyliticData.enemyWaveData.Add(new List<EnemyData>());

            for (int i = 0; i < enemy.numOfEnemies; i++)
            {
                //for current list, add enemy type and number of them
                EnemyData enemyData = new EnemyData();
                enemyData.enemyType = enemy.enemyType;
                enemyData.numOfEnemies = enemy.numOfEnemies;
                waveAnyliticData.enemyWaveData[currentWave - 1].Add(enemyData);
            }
        }

        GameManager.Instance.UpdateCoinCount(BaseWaveGoldGain + (currentWave * WaveEndGoldGain));
        GameObject.FindFirstObjectByType<BuildingInformationHandler>().UpdateUI();
        UpdateWaveCount.Instance.UpdateWaveText(currentWave + 1);
        UIRPCRelay.Instance.roundClientRpc(currentWave + 1);
        waveAnyliticData.timeLastedPerWave.Add(waveDuration); // adds wave duration

        if (waveNumber < waveData.waves.Count)
        {
            GameManager.Instance.SendRunTimeData();
            currentWave++;

            StopCoroutine(LogWaveDuration()); //stop counting time for wave
            waveDuration = 0;

            if (waveNumber >= maxWave)
            {
                GameManager.Instance.GameWin();
                yield break;
            }

            yield return new WaitForSeconds(3f); //3 seconds wait between waves
            StartCoroutine(StartWave(currentWave));

        }
    }

    private IEnumerator SpawnEnemiesWithDelay(EnemyWave wave)
    {
        foreach (var enemy in wave.enemies)
        {
            for (int i = 0; i < enemy.numOfEnemies; i++)
            {
                GameObject randomMoleHole = spawnLocations[UnityEngine.Random.Range(0, spawnLocations.Count)];
                //float waitTime = spawnEnemy(enemy.enemyType, randomMoleHole.transform.position, randomMoleHole.transform.rotation);

                SpawnEnemy(enemy.enemyType, randomMoleHole.transform.position, randomMoleHole.transform.rotation);

                yield return new WaitForSeconds(2f); //wait between enemies
            }
        }

        foreach (GameObject molehole in spawnLocations) {
            molehole.GetComponent<Animator>().Play("Remove");
        }

        spawnLocations.Clear();
    }

    bool roundOver()
    {
        enemiesSpawned.RemoveAll(enemy => enemy == null);
        
        return enemiesSpawned.Count == 0;
    }

    public void SpawnEnemy(string enemyName, UnityEngine.Vector3 t_centroid, UnityEngine.Quaternion rotation)
    {
        GameObject spawnedEnemy = null;
        switch (enemyName)
        {
            case "medium":
                spawnedEnemy = Instantiate(mediunOne, planet.transform.position, rotation, planet.transform);

                EnemyStats mediumLvl1Stats = GetEnemyStats("Medium_lvl_1");
                spawnedEnemy.GetComponent<Enemy>().Initialise(mediumLvl1Stats);
                break;
            case "medium2":
                spawnedEnemy = Instantiate(mediumTwo, planet.transform.position, rotation, planet.transform);

                EnemyStats mediumLvl2Stats = GetEnemyStats("Medium_lvl_2");
                spawnedEnemy.GetComponent<Enemy>().Initialise(mediumLvl2Stats);
                break;
            case "medium3":
                spawnedEnemy = Instantiate(mediumThree, planet.transform.position, rotation, planet.transform);

                EnemyStats mediumLvl3Stats = GetEnemyStats("Medium_lvl_3");
                spawnedEnemy.GetComponent<Enemy>().Initialise(mediumLvl3Stats);
                break;
            case "heavy":
                spawnedEnemy = Instantiate(heavyOne, planet.transform.position, rotation, planet.transform);

                EnemyStats heavyLvl1Stats = GetEnemyStats("Heavy_lvl_1");
                spawnedEnemy.GetComponent<Enemy>().Initialise(heavyLvl1Stats);
                break;
            default:
                Debug.LogWarning($"Unknown enemy type: {enemyName}");
                break;
        }

        spawnedEnemy.GetComponent<Enemy>().planet = planetTransform;
        spawnedEnemy.transform.position = t_centroid;

        spawnedEnemy.GetComponent<NetworkObject>().Spawn();


        planetTransform.GetComponent<Atmosphere>().gatherObjectsInAtmosphere();
        GameManager.Instance.UpdateEnemySpawned();
        enemiesSpawned.Add(spawnedEnemy);

        //return spawnedEnemy.GetComponent<Enemy>().spawnTimeWait;
    }

    private GameObject getRandomCentroid2()
    {

        GameObject planet = GameObject.Find("Planet");
        PlanetGeneration planetGenerator = planetTransform.GetComponent<PlanetGeneration>();

        Transform[] turrets = GameObject.FindGameObjectsWithTag("Tower")
                                    .Select(t => t.transform)
                                    .ToArray();

        GameObject selectedCentroid = new GameObject();
        bool validSpawn = false;
        float minDistance = 8f;

        while (!validSpawn)
        {
            GameObject[] triangles = GameObject.FindGameObjectsWithTag("Triangle");

            int randomTriangle = UnityEngine.Random.Range(0, triangles.Length - 1);

            selectedCentroid = triangles[randomTriangle].transform.Find("centroid").gameObject;

            // Check distance from all turrets
            validSpawn = turrets.All(turret => UnityEngine.Vector3.Distance(selectedCentroid.transform.position, turret.position) > minDistance);
        }


        return selectedCentroid;
    }
}