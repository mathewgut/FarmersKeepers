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


        if (Input.GetMouseButtonDown(0) && farmer.IsInBuildMode.Value)
        {
            if (buildable.isValid && !buildController.objHasSwitched)
            {
                Debug.Log("here!!!");
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
            Debug.Log("tospawn");
            GameManagement.Instance.builtItems.Value += 1;
            GameObject newTower = Instantiate(toSpawn, pos, rot);
            newTower.GetComponent<NetworkObject>().Spawn();
        }

        
    }



}
