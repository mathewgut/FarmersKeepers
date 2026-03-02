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

    // what the user sees when they go to build/place an object
    public GameObject buildObject;

    // defines what makes an object buildable or not buildable
    BuildableBehaviour buildable;

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

        if (!IsOwner)
        {
            buildObject.SetActive(false); 
            return;
        }


        // if build mode is active on "farmer"
        if (farmer.IsInBuildMode.Value)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            buildObject.SetActive(true);

            // if hit anything (cant place on not ground)
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                buildPos = new Vector3(hit.point.x, 1f, hit.point.z);
                buildPos = buildPos - new Vector3(buildPos.x % CELL_SIZE, buildPos.y, buildPos.z % CELL_SIZE);

                buildObject.transform.position = buildPos;

                MeshRenderer[] childrenMeshes = buildObject.GetComponentsInChildren<MeshRenderer>();

                Material targetMat = buildable.isValid ? buildableMat : notBuildableMat;

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

}
