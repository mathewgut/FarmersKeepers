using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class GameManagement : NetworkBehaviour
{
    public enum FARMER_STATE
    {
        Build,
        View,
    }
    public enum GAME_STATE
    {
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


    public NetworkVariable<int> wave = new NetworkVariable<int>(0);

    public NetworkVariable<FARMER_STATE> farmerState = new NetworkVariable<FARMER_STATE>(FARMER_STATE.View);
    public NetworkVariable<GAME_STATE> gameState = new NetworkVariable<GAME_STATE>(GAME_STATE.Prepare);

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
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if(spawnedEnemies.Count < 10 * wave.Value)
        {
            RandomEnemySpawn();
        }




    }

    public GameObject GetTower(TowerBehaviour.TOWER_TYPE type)
    {
        foreach (TowerInstance tower in towerPrefabs)
        {
            if (tower.type == type) return tower.prefab;
        }
        return null;
    }
    void RandomEnemySpawn ()
    {
        int spawnPos = Random.Range(0, enemySpawnPoints.Count);
        GameObject enemyInstance = Instantiate(EnemyPrefab, enemySpawnPoints[spawnPos].transform.position, Quaternion.identity);
        enemyInstance.GetComponent<NetworkObject>().Spawn();
        spawnedEnemies.Add(enemyInstance);
    }

}
