using Unity.Netcode;
using UnityEngine;

public class SpawnBuild : NetworkBehaviour
{
    GameObject buildObject;
    GameObject buildPrefab;

    BuildableBehaviour buildable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildPrefab = transform.GetComponent<BuildController>().previewObj;
        buildObject = transform.GetComponent<BuildController>().buildObject;
        buildable = buildObject.transform.GetComponent<BuildableBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            
            if (buildable.isValid)
            {
                Debug.Log("VALID");
                RequestSpawnTowerServerRpc(buildable.transform.position, buildable.transform.rotation);
            }
        }

    }

    [ServerRpc]
    private void RequestSpawnTowerServerRpc(Vector3 pos, Quaternion rot)
    {

        GameObject newTower = Instantiate(buildPrefab, pos, rot);
        newTower.GetComponent<NetworkObject>().Spawn();
    }


}
