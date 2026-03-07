using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using static UnityEngine.ParticleSystem;
using UnityEngine.UIElements;

public class GameManagement : NetworkBehaviour
{
    public static GameManagement Instance { get; private set; }
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
        Lost
    }

    public enum GAME_TYPE { 
        Coop,
        Versus
    }

    [System.Serializable]
    public struct TowerInstance
    {
        public TowerBehaviour.TOWER_TYPE type;
        public GameObject prefab;
    }

    public GAME_TYPE gameType = GAME_TYPE.Coop;
    public TowerInstance[] towerPrefabs;
    public List<AudioClip> ExplodeAudios = new List<AudioClip>();
    public List<AudioClip> AnticipateAudios = new List<AudioClip>();
    public List<AudioClip> AnimalAudios = new List<AudioClip>();
    [SerializeField] AudioClip explosionSound;


    // synced states across clients
    public NetworkVariable<int> wave = new NetworkVariable<int>(1);
    public NetworkVariable<FARMER_STATE> farmerState = new NetworkVariable<FARMER_STATE>(FARMER_STATE.View); // farmer state just means player state, too much refactoring to fix sry
    public NetworkVariable<GAME_STATE> gameState = new NetworkVariable<GAME_STATE>(GAME_STATE.Waiting);
    public NetworkVariable<bool> prepareStarted = new NetworkVariable<bool>(false);
    public NetworkVariable<int> builtItems = new NetworkVariable<int>(0);
    public NetworkVariable<int> maxBuiltItems = new NetworkVariable<int>(4);
    public NetworkVariable<int> toSpawnCount = new NetworkVariable<int>(0);
    public NetworkVariable<bool> hasAllSpawned = new NetworkVariable<bool>(false);
    int allSpawnedCount = 0;
   

    float defaultPrepareTimer = 30;
    float prevTime = 0;
    public NetworkVariable<float> prepareTimer = new NetworkVariable<float>(-1f);

    bool clientsConnected = false;

    [SerializeField] ParticleSystem particles;
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] List<GameObject> enemySpawnPoints = new List<GameObject>();
    
    public List<GameObject> spawnedEnemies = new List<GameObject>();

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

        if (gameType == GAME_TYPE.Versus) toSpawnCount.Value = 10 * wave.Value;

        if(gameState.Value == GAME_STATE.Lost)
        {
            Time.timeScale = 0; // stop gametime if lost
            return;
        }

        // after initial setup, every round should be quicker (less items to spawn, less time needed)
        if(wave.Value > 1 && defaultPrepareTimer != 15)
        {
            defaultPrepareTimer = 15;
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


        if (gameType == GAME_TYPE.Coop)
        {
            if (gameState.Value == GAME_STATE.Defend && allSpawnedCount < 10 * wave.Value)
            {
                RandomEnemySpawn();
            }
            else if (allSpawnedCount >= 10 * wave.Value)
            {
                if (gameState.Value == GAME_STATE.Defend)
                {
                    ManageWave();
                }
            }
        }
        // could this be refactored into one neat conditional? yes. am I going to do that? no. no i am not.
        else
        {
            if (gameState.Value == GAME_STATE.Defend && allSpawnedCount < toSpawnCount.Value)
            {
                hasAllSpawned.Value = false;
            }
            else if (allSpawnedCount >= 10 * wave.Value)
            {
                hasAllSpawned.Value = true;
                if (gameState.Value == GAME_STATE.Defend)
                {
                    ManageWave();
                }
            }

            
        }

    }
    
    // allows the server to spawn different towers depending on player input
    public GameObject GetTower(TowerBehaviour.TOWER_TYPE type)
    {

        // so enemy player can still spawn enemies when build limit is reached
        if (builtItems.Value >= maxBuiltItems.Value && type != TowerBehaviour.TOWER_TYPE.Enemy) return null;
        if(type == TowerBehaviour.TOWER_TYPE.Enemy)
        {
            allSpawnedCount += 1;
        }

        foreach (TowerInstance tower in towerPrefabs)
        {
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

    // play explosion particles everywhere
    [ClientRpc]
    public void PlayParticlesClientRpc(Vector3 position)
    {
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        Instantiate(particles, position, Quaternion.identity);
    }

    [ClientRpc]
    public void PlayExplosionSoundClientRpc(ulong netId, int explodeVoice)
    {
        // find enemy 
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out NetworkObject enemy))
        {
            AudioSource emitter = enemy.GetComponent<AudioSource>();
            emitter.clip = ExplodeAudios[explodeVoice];
            emitter.volume = 0.5f; // voice lines are stupid loud
            emitter.Play();
        }
    }


    // methods for getting voice lines
    public int GetRandomAudioAnimal()
    {
        return Random.Range(0, AnimalAudios.Count);
    }

    public int GetRandomAudioExplode()
    {
        return Random.Range(0, ExplodeAudios.Count);
    }

    public int GetRandomAudioAnticipate()
    {
        return Random.Range(0, AnticipateAudios.Count);
    }

}
