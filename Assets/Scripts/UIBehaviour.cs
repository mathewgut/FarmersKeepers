using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBehaviour : NetworkBehaviour
{
    [SerializeField] BuildController build;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
    }


    public void ChangeObjectSelection(GameObject prefab)
    {
        build.UpdateBuildObject(prefab);
    }

}
