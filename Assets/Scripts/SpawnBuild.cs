using Unity.Netcode;
using UnityEngine;

public class SpawnBuild : NetworkBehaviour
{
    GameObject buildObject;
    GameObject buildPrefab;

    BuildableBehaviour buildable;

    BuildController buildController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //buildPrefab = transform.GetComponent<BuildController>().previewObj;
        //buildObject = transform.GetComponent<BuildController>().buildObject;
        //buildable = buildObject.transform.GetComponent<BuildableBehaviour>();

        buildController = transform.GetComponent<BuildController>();
    }

    // Update is called once per frame
    void Update()
    {
        // for the server to instantiate objects
        if (buildController != null)
        {
            buildObject = buildController.buildObject;
            buildPrefab = buildController.previewObj;
        }

        if (!IsOwner) return;

        buildable = buildObject.transform.GetComponent<BuildableBehaviour>();
       

        if (Input.GetMouseButtonDown(0))
        {
            if (buildable.isValid && !buildController.objHasSwitched)
            {
                Debug.Log("VALID");
                RequestSpawnTowerServerRpc(buildController.currentTowerType,buildable.transform.position, buildable.transform.rotation);
            }
        }

    }

    [ServerRpc]
    private void RequestSpawnTowerServerRpc(TowerBehaviour.TOWER_TYPE type, Vector3 pos, Quaternion rot)
    {
        GameObject toSpawn = GameManagement.Instance.GetTower(type);
        if (toSpawn)
        {
            GameObject newTower = Instantiate(toSpawn, pos, rot);
            newTower.GetComponent<NetworkObject>().Spawn();
        }

        
    }


}
