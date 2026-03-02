using UnityEngine;

public class TowerProximity : MonoBehaviour
{

    [SerializeField] string targetTag;
    public bool hasTarget = false;
    public GameObject targetObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            hasTarget = false;
            targetObject = null;
        }
    }

}
