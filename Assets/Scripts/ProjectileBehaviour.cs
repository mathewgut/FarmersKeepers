using Unity.Netcode;
using UnityEngine;

public class ProjectileBehaviour : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject target;
    public float damage = 34f;
    public float fireDamage = 7f;
    public float iceSpeed = 0.5f;


    public enum PROJECTILE_TYPE
    {
        Default,
        Ice,
        Fire
    }

    public PROJECTILE_TYPE projType = PROJECTILE_TYPE.Default;

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if (target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 10f * Time.deltaTime);
            transform.LookAt(target.transform.position);
        }

        if(target == null)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    // when projectile collides with enemy
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<HealthSystem>(out HealthSystem health))
            {
                health.effect = projType;
                health.TakeDamage(damage);
            }

            if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
            {
                GetComponent<NetworkObject>().Despawn();
            }
            
        }
            
           
    }
}
