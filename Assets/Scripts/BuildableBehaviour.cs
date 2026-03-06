using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Netcode;

public class BuildableBehaviour : MonoBehaviour
{
    // this script basically dictates what makes an object placeable

    public bool isValid = true;

    private List<GameObject> obstacles = new List<GameObject>();

    void Start()
    {
       obstacles = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // if no obstacles, is valid;
        isValid = obstacles.Count == 0;

    }

    // if a to be placed object is colliding with another object, add to obstacles list
    private void OnTriggerEnter(Collider other)
    {
        if (obstacles == null) obstacles = new List<GameObject>();
        Debug.Log(other);

        if (!other.gameObject.CompareTag("BuildableArea") && !other.gameObject.CompareTag("Proximity"))
        {
            obstacles.Add(other.gameObject);
        }
    }

    // if an object leaves the bounds of the to be placed object, remove from obstacles
    private void OnTriggerExit(Collider other)
    {
        if (obstacles == null) return;
        if (!other.gameObject.CompareTag("BuildableArea"))
        {
            if(obstacles.Contains(other.gameObject)) obstacles.Remove(other.gameObject);
        }
    }

}
