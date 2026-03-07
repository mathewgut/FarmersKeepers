using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class HealthSystem : NetworkBehaviour
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(100);

    public ProjectileBehaviour.PROJECTILE_TYPE effect = default;

  
    float prevTime;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] Canvas healthCanvas;
    [SerializeField] TMPro.TextMeshProUGUI healthText;
    
    float agentSpeed;

    // how many times to take fire damage
    [SerializeField] int fireEffectCount = 3;
    [SerializeField] float fireCooldown = 1.5f;
    [SerializeField] int fireDamage = 7;

    // how long ice slow effect to last
    [SerializeField] float iceEffectTime = 7;

    // when despawn is called by server, remove reference from game spawnedEnemies list
    public override void OnNetworkDespawn()
    { 
        GameManagement.Instance.DeleteEnemy(NetworkObjectId);
        
        // call rest of base functionality
        base.OnNetworkDespawn(); 
    }

    private void Start()
    {
        agentSpeed = agent.speed;
        prevTime = -1;

        if (GameManagement.Instance.wave.Value > 1 && IsServer) CurrentHealth.Value += 5;
    }


    private void Update()
    {
        // sync health text if camera exists
        healthText.text = CurrentHealth.Value.ToString();
        if (Camera.main) {
            healthCanvas.transform.LookAt(Camera.main.transform.position);
            healthCanvas.transform.rotation =  Quaternion.Euler(
                healthCanvas.transform.rotation.eulerAngles.x,
                healthCanvas.transform.rotation.eulerAngles.y -180,
                healthCanvas.transform.rotation.eulerAngles.z
            );
        };

        switch (effect)
        {
            // default projectile behaviour
            case ProjectileBehaviour.PROJECTILE_TYPE.Default:
                break;

            case ProjectileBehaviour.PROJECTILE_TYPE.Fire:
                if(fireEffectCount > 0)
                {
                    if(prevTime == -1)
                    {
                        prevTime = Time.time;
                        break;
                    }
                    // if cooldown done, and fire effect still has uses, do fire damage
                    if(Time.time - prevTime >= fireCooldown && fireEffectCount > 0)
                    {
                        TakeDamage(fireDamage);
                        fireEffectCount -= 1;
                    }   
                }
                break;
            case ProjectileBehaviour.PROJECTILE_TYPE.Ice:
                if(prevTime == -1)
                {
                    prevTime = Time.time;
                    break;
                }
                else if (Time.time - prevTime <= iceEffectTime)
                {
                    agent.speed = agentSpeed / 2;
                    agent.acceleration = 4;
                }
                else
                {
                    agent.speed = agentSpeed;
                    agent.acceleration = 8;
                }
                break;

        }
    }


    public void TakeDamage(float damage)
    {
        if (!IsServer) return;

        CurrentHealth.Value -= damage;

        if (CurrentHealth.Value <= 0)
        {
            GameManagement.Instance.PlayParticlesClientRpc(new Vector3(
                agent.transform.position.x,
                agent.transform.position.y + 3, // offset because pivot is on the floor for enemy
                agent.transform.position.z
                ));
            DeleteSelf();
        }
    }

    public void DeleteSelf()
    {
        // when called that means any entity is attempting to delete, bc dead, so, nuke timer
        prevTime = -1;
        
 
        // prevents attempted despawns on objects that have already been despawned
        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned) {

            // kept getting memory allocation errors, this frees up all threads so no weird crashes
            agent.isStopped = true;
            agent.enabled = false;

            GetComponent<NetworkObject>().Despawn();
        }
    }

    
}
