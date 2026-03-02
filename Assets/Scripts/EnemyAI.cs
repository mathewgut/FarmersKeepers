using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : NetworkBehaviour
{
    GameObject target;
    private NavMeshAgent agent;
    bool hasTarget = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameObject.FindWithTag("Pen");
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    { 
        if (!hasTarget)
        {
            agent.SetDestination(target.transform.position);
            hasTarget = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Tcollision");
        if (!other.gameObject.CompareTag("Pen")) return;
        GetComponent<NetworkObject>().Despawn(true);
    }

}
