using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Netcode;

public class BuildableBehaviour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool isValid = true;

    private List<GameObject> obstacles = new List<GameObject>();

    void Start()
    {
       obstacles = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!IsOwner) return;

        isValid = obstacles.Count == 0;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (obstacles == null) obstacles = new List<GameObject>();
        Debug.Log(other);

        if (!other.gameObject.CompareTag("BuildableArea") && !other.gameObject.CompareTag("Proximity"))
        {
            obstacles.Add(other.gameObject);
        }


        Debug.Log(isValid);
    }

    private void OnTriggerExit(Collider other)
    {
        if (obstacles == null) return;
        if (!other.gameObject.CompareTag("BuildableArea"))
        {
            if(obstacles.Contains(other.gameObject)) obstacles.Remove(other.gameObject);
        }
    }

}
