using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class HealthSystem : NetworkBehaviour
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(100);

    public ProjectileBehaviour.PROJECTILE_TYPE effect = default;

    float prevTime;
    float particleTime;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] Canvas healthCanvas;
    [SerializeField] TMPro.TextMeshProUGUI healthText;
    [SerializeField] AudioSource soundEmitter;
    [SerializeField] ParticleSystem particles;
    
    float agentSpeed;

    // how many times to take fire damage
    [SerializeField] int fireEffectCount = 3;
    [SerializeField] float fireCooldown = 1.5f;
    [SerializeField] int fireDamage = 7;

    // how long ice slow effect to last
    [SerializeField] float iceEffectTime = 7;

    // when despawn is called, remove reference from game spawnedEnemies list
    public override void OnNetworkDespawn()
    {
        if (GameManagement.Instance != null)
        {
            GameManagement.Instance.DeleteEnemy(NetworkObjectId);
        }
        base.OnNetworkDespawn();
    }

    private void Start()
    {
        agentSpeed = agent.speed;
        prevTime = -1;
        particleTime = -1;
        particles.Stop();
        
    }


    private void Update()
    {
        // sync health text if camera exists
        healthText.text = CurrentHealth.Value.ToString();
        if (Camera.main) healthCanvas.transform.LookAt(Camera.main.transform.position);

        switch (effect)
        {
            // default projectile behaviour
            case ProjectileBehaviour.PROJECTILE_TYPE.Default:
                break;
            // 
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
            PlayParticlesClientRpc();
            DeleteSelf();
        }
    }

    public void DeleteSelf()
    {
        // prevents attempted despawns on objects that have already been despawned
        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned) GetComponent<NetworkObject>().Despawn();
    }

    // the forced rpc suffix is stupid and makes no sense, why do that when i also have to tag it?? 
    [ClientRpc]
    private void PlayParticlesClientRpc()
    {
        // play explosion particles everywhere
        Instantiate(particles, transform.position, Quaternion.identity);
    }
}
