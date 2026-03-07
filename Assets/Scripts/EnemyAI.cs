using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

public class EnemyAI : NetworkBehaviour
{
    GameObject target;
    NavMeshAgent agent;
    bool hasTarget = false;
    float deathTimer = -1;
    float currentDeathTime = 10f;
    public bool hasStartedExplosion = false;
    public TowerBehaviour.TOWER_TYPE towerType = TowerBehaviour.TOWER_TYPE.Enemy;

    float movementTime = -1;
    Vector3 lastPosition;

    [SerializeField] HealthSystem health;
    [SerializeField] AudioSource emitter;

    // by using a list for speeds, i can change the probability of getting a higher speed vs a lower speed
    // using random then accessing a direct index is actually O(1) which is insane
    // also the performance for this game is already horrible, idk why i care now lol
    [SerializeField] List<float> speeds = new List<float>();

    void Start()
    {
        target = GameObject.FindWithTag("Pen");
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speeds[Random.Range(0, speeds.Count)];
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // ensures only host updates position, otherwise client ai has weird jittery motion
        if (!IsServer) return;

        // explode when gets stuck for too long
        if (Vector3.Distance(lastPosition, transform.position) > 2f)
        {
            lastPosition = transform.position;
            movementTime = Time.time;

        }
        else if(Time.time - movementTime >= 10f && movementTime != -1) // if hasn't moved more than 2m in 6 seconds, explode lol
        {
            GameManagement.Instance.PlayParticlesClientRpc(new Vector3(
               agent.transform.position.x,
               agent.transform.position.y + 3, // offset because pivot is on the floor for enemy
               agent.transform.position.z
               ));

            health.DeleteSelf();
        }

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
                GetComponent<NetworkObject>().Despawn(true);
            }
        }

        if (hasStartedExplosion)
        {
            if (Time.time - deathTimer >= currentDeathTime)
            {
                if (NetworkObject.IsSpawned)
                {
                    // Execute the explosion
                    GameManagement.Instance.PlayParticlesClientRpc(new Vector3(
                        agent.transform.position.x,
                        agent.transform.position.y + 3, // offset because pivot is on the floor for enemy
                        agent.transform.position.z
                    ));
                    target.GetComponent<AnimalPenBehaviour>().RemoveAnimalServerRpc();
                    health.DeleteSelf();
                    
                }
            }
        }

    }


    // method that invokes final voice line when reached target and destroys itself after time
    public void ReachedTarget()
    {
        // stops agent form pushing around other agents
        if (agent.enabled) agent.isStopped = true;

        // tracks whether agent has previously reached the goal and is waiting to explode
        if (!hasStartedExplosion)
        {
            hasStartedExplosion = true;
            deathTimer = Time.time;

            int clipIndex = GameManagement.Instance.GetRandomAudioExplode();
            GameManagement.Instance.PlayExplosionSoundClientRpc(NetworkObjectId, clipIndex);

           currentDeathTime = GameManagement.Instance.ExplodeAudios[clipIndex].length / 1.25f; // reduce by a quarter of time bc funnier
        }

    }
}
