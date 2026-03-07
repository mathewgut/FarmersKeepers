using Unity.Netcode;
using UnityEngine;

public class SpawnBuild : NetworkBehaviour
{
    GameObject buildObject;
    GameObject buildPrefab;

    BuildableBehaviour buildable;
    [SerializeField] FarmerInteraction farmer;
    GameManagement manager;

    BuildController buildController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //buildPrefab = transform.GetComponent<BuildController>().previewObj;
        //buildObject = transform.GetComponent<BuildController>().buildObject;
        //buildable = buildObject.transform.GetComponent<BuildableBehaviour>();

        buildController = transform.GetComponent<BuildController>();
        manager = GameManagement.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        // for the server to instantiate objects
        if (buildController != null)
        {
            if(buildController.buildObject != null && buildController.previewObj != null)
            {
                buildObject = buildController.buildObject;
                buildPrefab = buildController.previewObj;
            }
           
        }

        if (!IsOwner) return;

        buildable = buildObject.transform.GetComponent<BuildableBehaviour>();

        // dont allow enemy to spawn too close to pen, or else its just a free win lol
        if (farmer.isEnemy && Vector3.Distance(buildable.transform.position, GameObject.FindWithTag("Pen").transform.position) < 22.5f) buildable.isValid = false;

        if (Input.GetMouseButtonDown(0) && farmer.IsInBuildMode.Value)
        {
            // if in versus mode and enemy trying to spawn
            if (farmer.isEnemy && GameManagement.Instance.gameState.Value != GameManagement.GAME_STATE.Defend) return;
            if (farmer.isEnemy && GameManagement.Instance.hasAllSpawned.Value == true) return;
            if (buildable.isValid && !buildController.objHasSwitched)
            {
                RequestSpawnTowerServerRpc(buildController.currentTowerType, buildable.transform.position, buildObject.transform.rotation);
                
            }
        }

    }

    // build selected item and update built items
    [ServerRpc]
    private void RequestSpawnTowerServerRpc(TowerBehaviour.TOWER_TYPE type, Vector3 pos, Quaternion rot)
    {
        GameObject toSpawn = GameManagement.Instance.GetTower(type);
        if (toSpawn)
        {
            // only increment if not in versus mode and spawning enemy
            if(type != TowerBehaviour.TOWER_TYPE.Enemy) GameManagement.Instance.builtItems.Value += 1;
            GameObject newTower = Instantiate(toSpawn, pos, rot);

            // add to spawned enemies list if spawning an enemy (allows manage wave to stay in sync)
            if(type == TowerBehaviour.TOWER_TYPE.Enemy) GameManagement.Instance.spawnedEnemies.Add(newTower);
            newTower.GetComponent<NetworkObject>().Spawn();
        }   

        
    }



}
