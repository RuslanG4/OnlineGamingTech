using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

using TowerInput = TowerFactory.TowerKey;
using TowerID = TowerData.TowerID;
using static JSONEnemyInformation;
using Unity.Netcode;

public enum Difficulty { Easy = 0, Medium = 1, Hard = 2};

public class GameManager : MonoBehaviour
{
    public string GameVersion = "1.0";
    public string gameSessionHash = "";

    [System.Serializable]
    public class RunTimeUiData
    {
        public string deviceID;
        public string gameVersion;
        public string gameSessionHash;
        public float timeBetweenClicks;
    }
    public Difficulty difficulty = Difficulty.Easy; 


    // Classes to help with JSON serialization
    [System.Serializable]
    public class TowerPlacementData
    {
        public int towerId;
        public string towerType;
        public int towerLevel;
    }

    [System.Serializable]
    public class WaveDurationData
    {
        public int waveNumber;
        public float duration;
    }

    [System.Serializable]
    public class RunTimeGameData
    {
        public string deviceID;
        public string gameVersion;
        public string gameSessionHash;
        public int currentMoney;
        public int towersPlaced;
        public int enemiesSpawned;
        public int enemiesKilled;
        public int projectilesFired;
        public int wavesSurvived;
        public int totalGoldMade;
        public int goldMineCount;
        public float goldMadeFromGoldmines; 

        // Convert dictionary to a list for easier JSON serialization
        public List<TowerPlacementData> towersTypesPlaced;
        public List<WaveDurationData> wavesDurations;
    }

    [System.Serializable]
    public class StaticGameData
    {
        public string deviceID;
        public int towerCost = 700;

        public string enemySpawnRate;

        public int mediumEnemyMaxDamage;
        public float mediumEnemyMaxSpeed;
        public float mediumEnemyMaxHealth;

        public int heavyEnemyMaxDamage;
        public float heavyEnemyMaxSpeed;
        public float heavyEnemyMaxHealth;

        public int projectileDamage;
        public int hitGoldGain;
        public int killGoldGain;
    }

    private float runtimeDataSendingRate = 1; // How often we send data to the server

    private static GameManager _instance;
    public bool gameOver = false;

    private int totalGoldMade = 0;
    public int towersPlaced = 0;
    public Dictionary<TowerID, TowerInput> towersTypesPlaced = new Dictionary<TowerID, TowerInput>(); // ID -> Tower Data
    private int enemiesSpawned;
    private int enemiesKilled;
    private int projectilesFired;
    public int wavesSurvived = 1;

    public int currentGoldMineCount = 0;
    public float totalGoldMadeFromGoldMines = 0;


    public bool buildingUIOpen = false;
    private UIDataManager uiManager;

    //caching for triangles on planet
    public List<GameObject> allTriangles = new List<GameObject>();
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject("GameManager");
                _instance = singletonObject.AddComponent<GameManager>();
                DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }
    }

    public void DecrementGoldMinesNumber()
    {
        if(currentGoldMineCount>0)
            currentGoldMineCount--;
    }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData(); //loads enemy data

        
    }

    public int GetCoinCount()
    {
        return CoinManager.Instance.getCoins();
    }

    public void UpdateCoinCount(int _coinCount)
    {


        CoinManager.Instance.addCoins(_coinCount);
        if (_coinCount > 0)
        {
            totalGoldMade += _coinCount;
        }
        GamePlayUIController.Instance.CreateMoneyEffect(_coinCount);
        GameObject.FindFirstObjectByType<BuildingInformationHandler>().UpdateUI();
    }

    public void UpdateEnemyKilled() => enemiesKilled++;
    public void UpdateEnemySpawned() => enemiesSpawned++;
    public void UpdateProjectilesFired() => projectilesFired++;

    public void RemoveTowerFromDataList(TowerID towerID)
    {
        if (towersTypesPlaced.ContainsKey(towerID))
        {
            towersTypesPlaced.Remove(towerID);
            towersPlaced--;
        }
    }
    public void TowerDataListUpdate(TowerData towerData)
    {
        TowerInput towerInput;
        towerInput.TowerType = towerData.TowerType;
        towerInput.Level = towerData.Level;
        TowerID Id = towerData.ID;
        towersTypesPlaced[Id] = towerInput;
    }
    public void UpdateTowersPlaced() => towersPlaced++;
    public RunTimeGameData GetRunTimeData()
    {
        var enemySpawner = GameObject.FindAnyObjectByType<EnemySpawner>();
        var waveDurations = enemySpawner.GetWaveDurationTimes();
        int wavesSurvived = enemySpawner.getCurrentWave();

        // Convert dictionaries to lists for json because it cant translate it
        var towersList = towersTypesPlaced.Select((kvp, index) => new TowerPlacementData
        {
            towerId = kvp.Key.AsInt(),
            towerType = kvp.Value.TowerType,
            towerLevel = (int)kvp.Value.Level + 1
        }).ToList();

        var waveDurationsList = waveDurations.Select((duration, index) => new WaveDurationData
        {
            waveNumber = index,
            duration = duration
        }).ToList();

        return new RunTimeGameData
        {
            deviceID = SystemInfo.deviceUniqueIdentifier,
            gameSessionHash = gameSessionHash,
            gameVersion = GameVersion,
            towersPlaced = towersPlaced,
            currentMoney = CoinManager.Instance.getCoins(),
            enemiesKilled = enemiesKilled,
            enemiesSpawned = enemiesSpawned,
            projectilesFired = projectilesFired,
            wavesSurvived = wavesSurvived,
            totalGoldMade = totalGoldMade,
            goldMineCount = currentGoldMineCount,
            goldMadeFromGoldmines = totalGoldMadeFromGoldMines,
            towersTypesPlaced = towersList,
            wavesDurations = waveDurationsList
        };
    }

    public void SendRunTimeData()
    {
        RunTimeGameData gameData = GetRunTimeData();

        string jsonData = JsonUtility.ToJson(gameData);


        StartCoroutine(AnalyticsManager.PostMethod(jsonData));
    }

    public void SendSetupData()
    {
        StaticGameData data = new StaticGameData();
        data.deviceID = SystemInfo.deviceUniqueIdentifier;

        EnemyStats mediumLvl1Stats = GetEnemyStats("Medium_lvl_1");

        data.mediumEnemyMaxDamage = mediumLvl1Stats.maxDamage;
        data.mediumEnemyMaxSpeed = mediumLvl1Stats.maxSpeed;
        data.mediumEnemyMaxHealth = mediumLvl1Stats.maxHealth;

        EnemyStats heavyLvl1Stats = GetEnemyStats("Heavy_lvl_1");

        data.heavyEnemyMaxDamage = heavyLvl1Stats.maxDamage;
        data.heavyEnemyMaxSpeed = heavyLvl1Stats.maxSpeed;
        data.heavyEnemyMaxHealth = heavyLvl1Stats.maxHealth;


        data.projectileDamage = (int)TowerFactory.GetTowerData("Cannon", TowerData.TowerLevel.LevelOne).Damage;
        data.towerCost = TowerFactory.GetTowerData("Cannon", TowerData.TowerLevel.LevelOne).Cost;
        data.enemySpawnRate = BasicEnemySpawner.SpawnRate.ToString("f2");
        data.hitGoldGain = Projectile.HitGoldGain;

        string jsonData = JsonUtility.ToJson(data);

        StartCoroutine(AnalyticsManager.PostMethod(jsonData));
    }

    public void StartUIDataCollection()
    {
        buildingUIOpen = true;
        StartCoroutine(CountTimeBetweenTowerPlacement());
    }
    public IEnumerator CountTimeBetweenTowerPlacement()
    {
       float time = 0;
        bool menuOpenIdle = false;
        while (buildingUIOpen)
        {
            time += Time.deltaTime;
            if(time > 10f)
            {
                menuOpenIdle = true;
                break;
            }
            yield return null;
        }
        buildingUIOpen = false;
        if (!menuOpenIdle)
        {
            uiManager.TriggerBuildingUIClosed(time);
        }
    }

    private void HandleBuildingUIClosed(float time)
    {
        RunTimeUiData data = new RunTimeUiData();
        data.deviceID = SystemInfo.deviceUniqueIdentifier;
        data.gameVersion = GameVersion;
        data.gameSessionHash = gameSessionHash;
        data.timeBetweenClicks = time;

        string jsonData = JsonUtility.ToJson(data);

#if !UNITY_EDITOR
        StartCoroutine(AnalyticsManager.PostMethod(jsonData));
#endif

    }


    // Start is called before the first frame update
    void Start()
    {
        uiManager = new UIDataManager();
        uiManager.OnBuildingUIClosed += HandleBuildingUIClosed;
    }

    void _TestGameOver()
    {

        if(enemiesKilled == 9 && enemiesSpawned == 9 && !gameOver)
        {
            GameOver();

        }
 
    }

    // Update is called once per frame
    void Update()
    {
       _TestGameOver();
    }

    void PlaceGoldMines(Vector3 townHallPosition)
    {
       
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitUntil(() => GameObject.Find("Planet") != null);

        // Wait some time for the new camera to be assigned
        yield return new WaitForSeconds(1); // Small delay so that when the camera shoots a ray its the game camera

        GameObject planet = GameObject.Find("Planet");

        GameObject townHallPrefab = Resources.Load<GameObject>("Prefabs/Towers/TownHallTower");

        Tower tower = townHallPrefab.GetComponent<Tower>();

        // The planet should be in front of the camera so shoot at the middle of the screen space.
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0); // Screen center
        Camera currentCamera = GameObject.Find("Game Camera").GetComponent<Camera>();
        Ray ray = currentCamera.ScreenPointToRay(screenCenter);  // Send ray from camera to the center of the screen

        TowerPlacer towerPlacer = FindObjectOfType<TowerPlacer>();
        // Place the town hall in front of the player
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject clickedTriangle = hit.collider.gameObject.transform.Find("centroid").gameObject;

            towerPlacer.buildingPrefab = townHallPrefab;
            towerPlacer.PlaceBuilding(clickedTriangle.transform.position, "TownHall");
        }

        GameObject[] trianlgesToPlace =
        {
            GameObject.Find("Triangle 5").transform.Find("centroid").gameObject,
            GameObject.Find("Triangle 333").transform.Find("centroid").gameObject,
            GameObject.Find("Triangle 712").transform.Find("centroid").gameObject,
        };

        if(GameVersion == "A")
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject goldDrillPrefab = Resources.Load<GameObject>("Prefabs/Towers/GoldMine");
                towerPlacer.buildingPrefab = goldDrillPrefab;
                towerPlacer.PlaceBuilding(trianlgesToPlace[i].transform.position, "Gold Mine");
            }
        }

        EnemySpawner spawner = GameObject.FindObjectOfType<EnemySpawner>();
#if UNITY_EDITOR
        CoinManager.Instance.initialiseValue(1650);
#endif
        CoinManager.Instance.initialiseValue(550);
        spawner.startSpawning();
    }



    public void StartGame()
    {


        gameSessionHash = Guid.NewGuid().ToString();

        totalGoldMade = 0;
        towersPlaced = 0;
        enemiesSpawned = 0;
        enemiesKilled = 0;
        projectilesFired = 0;
        wavesSurvived = 1;
        gameOver = false;
        currentGoldMineCount = 0;
         totalGoldMadeFromGoldMines = 0;

    GameVersion = (UnityEngine.Random.Range(0, 2) == 0) ? "A" : "B";
        if (GamePlayUIController.Instance == null)
        {
            StartCoroutine(WaitForUIController());
            return;
        }

        GamePlayUIController.Instance.PlaceTowerMenu(GameVersion);

        if(GameplayMultiplayerManager.Instance.IsServer)
             StartCoroutine(DelayedStart());
    }

    private IEnumerator WaitForUIController()
    {
        while (GamePlayUIController.Instance == null)
        {
            yield return null; // Wait one frame
        }

        GamePlayUIController.Instance.PlaceTowerMenu(GameVersion);
        StartCoroutine(DelayedStart());
    }
    public void GameOver()
    {
        gameOver = true;
    }

    public void GameWin()
    {
        gameOver = true;
        GamePlayUIController.Instance.OpenGameWin();
    }
}
