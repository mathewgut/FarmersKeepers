using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildController : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] FarmerInteraction farmer;
    public GameObject previewObj; // prefab for creating buildObject

    [SerializeField] Material buildableMat;
    [SerializeField] Material notBuildableMat;

    Vector3 buildPos;
    const float CELL_SIZE = 1;
    public bool stopMouseTracking = false;
    public bool objHasSwitched = false;

    // what the user sees when they go to build/place an object
    public GameObject buildObject;

    // defines what makes an object buildable or not buildable
    BuildableBehaviour buildable;

    public TowerBehaviour.TOWER_TYPE currentTowerType;

    void Start()
    {
        buildObject = Instantiate(previewObj);
        buildObject.SetActive(false);
        buildObject.AddComponent<BuildableBehaviour>();

        buildable = buildObject.GetComponent<BuildableBehaviour>();
    }


    // Update is called once per frame
    void Update()
    {

        if (farmer == null) return;

        if (!buildObject) buildObject = Instantiate(previewObj);

        if (!IsOwner)
        {
            buildObject.SetActive(false); 
            return;
        }


        // if build mode is active on "farmer"
        if (farmer.IsInBuildMode.Value)
        {
            if (objHasSwitched)
            {
                buildable.isValid = true;
                objHasSwitched = false;
                return;
            }

            
            if (stopMouseTracking) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            buildObject.SetActive(true);

            // if hit anything (cant place on not ground)
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (!buildable || !buildObject) SpawnBuildObj();
                buildPos = new Vector3(Mathf.Round(hit.point.x / CELL_SIZE) * CELL_SIZE, 0, Mathf.Round(hit.point.z / CELL_SIZE) * CELL_SIZE); // ty caelan
                
                //buildPos = buildPos - new Vector3(buildPos.x % CELL_SIZE, buildPos.y, buildPos.z % CELL_SIZE);

                buildObject.transform.position = buildPos;

                MeshRenderer[] childrenMeshes = buildObject.GetComponentsInChildren<MeshRenderer>();

                if (farmer.isEnemy && Vector3.Distance(buildable.transform.position, GameObject.FindWithTag("Pen").transform.position) < 22.5f) buildable.isValid = false;
                Material targetMat = buildable.isValid ? buildableMat : notBuildableMat;

                // rotate object left 45deg if press q
                if (Input.GetKeyDown(KeyCode.Q) && farmer.IsInBuildMode.Value)
                {
                    buildObject.transform.rotation = Quaternion.Euler(
                        buildObject.transform.rotation.eulerAngles.x,
                        buildObject.transform.rotation.eulerAngles.y - 45,
                        buildObject.transform.rotation.eulerAngles.z
                    );
                }
                // rotate object right 45deg if press q
                else if (Input.GetKeyDown(KeyCode.E) && farmer.IsInBuildMode.Value)
                {
                    buildObject.transform.rotation = Quaternion.Euler(
                        buildObject.transform.rotation.eulerAngles.x,
                        buildObject.transform.rotation.eulerAngles.y + 45,
                        buildObject.transform.rotation.eulerAngles.z
                    );
                }

                foreach (MeshRenderer mesh in childrenMeshes)
                {
                    mesh.material = targetMat;
                }

            }
            else
            {
                buildObject.SetActive(false);
            }
        }
        else
        {
            buildObject.SetActive(false);
        }

    }

    // change ghost to new selection
    public void UpdateBuildObject (GameObject newObject)
    {
        // patch work fix bahaha i did not build this system with anything but towers in mind lmao
        if (newObject.TryGetComponent<TowerBehaviour>(out TowerBehaviour tower))
        {
            currentTowerType = tower.towerType;
        }
        else if(newObject.TryGetComponent<ObstacleInfo>(out ObstacleInfo obstacle))
        {
            currentTowerType = obstacle.type;
        }
        else if(newObject.TryGetComponent<EnemyAI>(out EnemyAI enemy))
        {
            currentTowerType = enemy.towerType;
        }

        Destroy(buildObject);
        previewObj = newObject;
        buildObject = Instantiate(previewObj);
        buildable = buildObject.AddComponent<BuildableBehaviour>();
        objHasSwitched = true;
    }

    // spawn build ghost
    void SpawnBuildObj()
    {
        if (buildObject) Destroy(buildObject);
        buildObject = Instantiate(previewObj);

        buildable = buildObject.AddComponent<BuildableBehaviour>();
    }

}
