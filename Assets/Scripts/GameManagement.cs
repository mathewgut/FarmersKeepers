using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class GameManagement : NetworkBehaviour
{
    public enum FARMER_STATE
    {
        Build,
        View,
    }
    public enum GAME_STATE
    {
        Waiting,
        Prepare,
        Defend,
        Final
    }

    [System.Serializable]
    public struct TowerInstance
    {
        public TowerBehaviour.TOWER_TYPE type;
        public GameObject prefab;
    }

    public TowerInstance[] towerPrefabs;


    // synced states across clients
    public NetworkVariable<int> wave = new NetworkVariable<int>(1);
    public NetworkVariable<FARMER_STATE> farmerState = new NetworkVariable<FARMER_STATE>(FARMER_STATE.View); // farmer state just means player state, too much refactoring to fix sry
    public NetworkVariable<GAME_STATE> gameState = new NetworkVariable<GAME_STATE>(GAME_STATE.Waiting);
    public NetworkVariable<bool> prepareStarted = new NetworkVariable<bool>(false);
    public NetworkVariable<int> builtItems = new NetworkVariable<int>(0);
    public NetworkVariable<int> maxBuiltItems = new NetworkVariable<int>(4);

    int allSpawnedCount = 0;
    bool waveStarted = false;

    float defaultPrepareTimer = 45;
    float prevTime = 0;
    public NetworkVariable<float> prepareTimer = new NetworkVariable<float>(-1f);

    bool clientsConnected = false;

    public static GameManagement Instance { get; private set; }

    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] List<GameObject> enemySpawnPoints = new List<GameObject>();

    List<GameObject> spawnedEnemies = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        // assert only one game management instance can exist
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // for some godforesaken reason, unity keeps defaulting this to wave 2 and 6 max, so we doing this nows
        if (IsServer)
        {
            wave.Value = 1;
            maxBuiltItems.Value = 4;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if(wave.Value > 1 && defaultPrepareTimer != 30)
        {
            defaultPrepareTimer = 30;
        }

        // this assumes only a host and client will ever be connected at the same time, bad for prod, great for this assignment
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2 && !clientsConnected)
        {
            gameState.Value = GAME_STATE.Prepare;
            clientsConnected = true;
            prepareTimer.Value = -1;
        }
        else if(!clientsConnected)
        {
            gameState.Value = GAME_STATE.Waiting;
        }

        if (gameState.Value == GAME_STATE.Prepare)
        {
            UpdateTimer();
        }
        

        if(gameState.Value == GAME_STATE.Defend && allSpawnedCount < 10 * wave.Value)
        {
            RandomEnemySpawn();
        }
        else if(allSpawnedCount >= 10 * wave.Value)
        {
            if (gameState.Value == GAME_STATE.Defend)
            {
                ManageWave();
            }
        }
        

    }
    
    // allows the server to spawn different towers depending on player input
    public GameObject GetTower(TowerBehaviour.TOWER_TYPE type)
    {
        Debug.Log($"SERVER: Requesting type {type}. Current Built: {builtItems.Value}/{maxBuiltItems.Value}");
        if (builtItems.Value >= maxBuiltItems.Value) return null;

        foreach (TowerInstance tower in towerPrefabs)
        {
            Debug.Log($"Checking: {tower.type} against requested: {type}");
            if (tower.type == type)
            { 
                return tower.prefab;
            }
            
        }
        return null;
    }
    
    // spawns enemy at random spawn point
    void RandomEnemySpawn ()
    {
        int spawnPos = Random.Range(0, enemySpawnPoints.Count);
        GameObject enemyInstance = Instantiate(EnemyPrefab, enemySpawnPoints[spawnPos].transform.position, Quaternion.identity);
        enemyInstance.GetComponent<NetworkObject>().Spawn();
        spawnedEnemies.Add(enemyInstance);
        allSpawnedCount += 1;
    }

    // manages prepare timer and switching to defend
    void UpdateTimer()
    {
        if (prepareTimer.Value == -1)
        {
            Debug.Log("== -1");
            prepareTimer.Value = defaultPrepareTimer;
            prevTime = Time.time;
        }
        else if (prepareTimer.Value != -1)
        {
            if (prepareTimer.Value >= 1 && Time.time - prevTime >= 1f)
            {
                prepareTimer.Value -= 1;
                prevTime = Time.time;
            }
            else if (prepareTimer.Value <= 0)
            {
                gameState.Value = GAME_STATE.Defend;
                prepareTimer.Value = -1;
                prevTime = 0;
            }

   
        }
    }

    void ManageWave()
    {
       // if (!waveStarted) return;

        if(spawnedEnemies.Count == 0)
        {
            gameState.Value = GAME_STATE.Prepare;
            wave.Value += 1;
            allSpawnedCount = 0;
            maxBuiltItems.Value += 2;
        }
    }

    // public method to remove enemy from list
    public void DeleteEnemy(ulong netID)
    {
        // removes from list if unique NGO ulong exists in list
        // super cool library function thats safe !!
        spawnedEnemies.RemoveAll(enemy =>
        enemy.GetComponent<NetworkObject>().NetworkObjectId == netID);
    }

}
