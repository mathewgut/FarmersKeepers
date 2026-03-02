using Unity.Netcode;
using UnityEngine;

public class ProjectileBehaviour : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject target;
    public float damage = 34f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if (target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 10f * Time.deltaTime);
            transform.LookAt(target.transform.position);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("hit");

            if (other.TryGetComponent<HealthSystem>(out HealthSystem health))
            {
                Debug.Log("subtracting health");

                health.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
            
           
    }
}
