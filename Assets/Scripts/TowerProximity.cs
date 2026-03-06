using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TowerProximity : MonoBehaviour
{

    [SerializeField] string targetTag;
    public bool hasTarget = false;
    public GameObject targetObject;
    public List<GameObject> targetsInRange = new List<GameObject>();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
       

    }

    public GameObject GetTarget()
    {
        // remove all instances where obj no longer exists
        targetsInRange.RemoveAll(obj => obj == null);

        if (targetsInRange.Count > 0)
        {
            return targetsInRange[0];
        }

        return null;
    }

    // -1 will return all items 
    public List<GameObject> GetMultipleTargets(int count)
    {
        // remove all instances where obj no longer exists
        targetsInRange.RemoveAll(obj => obj == null);

        if (count == -1 || count >= targetsInRange.Count)
        {
            return new List<GameObject>(targetsInRange);
        }
        
        // essentially acts like a splice in JS
        return targetsInRange.GetRange(0, count);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (!targetsInRange.Contains(other.gameObject))
            {
                targetsInRange.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            targetsInRange.Remove(other.gameObject);
        }
    }

}
