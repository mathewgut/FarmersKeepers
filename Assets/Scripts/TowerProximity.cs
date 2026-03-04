using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TowerProximity : MonoBehaviour
{

    [SerializeField] string targetTag;
    public bool hasTarget = false;
    public GameObject targetObject;
    private MeshRenderer rangeMesh;
    bool isBuildMode;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rangeMesh = transform.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("working!");
        isBuildMode = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<FarmerInteraction>().IsInBuildMode.Value;

        // show range while in build mode
        /*
        if (!isBuildMode)
        {
            rangeMesh.gameObject.SetActive(false);
        }
        else
        {
            rangeMesh.gameObject.SetActive(true);
        }
    */

        // if object explodes while area
        if (!targetObject && hasTarget)
        {
            targetObject = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(targetTag) && !targetObject)
        {
            hasTarget = true;
            targetObject = other.gameObject;
        }
    }

    // if enemy is killed and another is still in range
    private void OnTriggerStay(Collider other)
    {
        if(!targetObject && !hasTarget)
        {
            if (other.CompareTag(targetTag))
            {
                hasTarget = true;
                targetObject = other.gameObject;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            hasTarget = false;
            targetObject = null;
        }
    }

}
