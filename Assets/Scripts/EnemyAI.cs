using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : NetworkBehaviour
{
    GameObject target;
    private NavMeshAgent agent;
    bool hasTarget = false;
    [SerializeField] HealthSystem health;

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

        if (!agent.pathPending && hasTarget)
        {
            // if ai has target but can't get to it (user has blocked all possible routes), off self
            if (agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                health.DeleteSelf();
            }
        }

        // destroy if within close range of goal (avoids weird ai stacking issues)
        if (Vector3.Distance(transform.position, target.transform.position) < 2f) GetComponent<NetworkObject>().Despawn(true);
    }
}
