using Unity.Netcode;
using UnityEngine;

public class HealthSystem : NetworkBehaviour
{
    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>(100);

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;

        CurrentHealth.Value -= damage;

        if (CurrentHealth.Value <= 0)
        {
            DeleteSelf();
        }

    }

    void DeleteSelf()
    {
        // prevents attempted despawns on objects that have already been despawned
        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn();

        }
    }
}
