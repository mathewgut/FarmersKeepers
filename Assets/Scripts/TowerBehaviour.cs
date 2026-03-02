using Unity.Netcode;
using UnityEngine;


public class TowerBehaviour : NetworkBehaviour
{
    
    [SerializeField] TowerProximity proximity;

    public GameObject toShootAt;
    public GameObject projectile;
    GameObject shot = null;
    public bool shotFired = false;

    public float yShootOffset = 0f;
    Vector3 projectilePosition;
    Vector3 projectileStartPosition;

    public float shootAtDistance = 10f; 
    
    void Start()
    {
        projectileStartPosition = new Vector3(transform.position.x, yShootOffset, transform.position.z);
        projectilePosition = projectileStartPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer) return;


        if (!proximity.hasTarget) {

            toShootAt = null;
            return;
        };


        if(proximity.hasTarget && proximity.targetObject)
        {
            toShootAt = proximity.targetObject;

            
            if (!shotFired)
            {
                shot = Instantiate(projectile, projectilePosition, Quaternion.identity);
                shot.GetComponent<NetworkObject>().Spawn();
                shot.GetComponent<ProjectileBehaviour>().target = toShootAt;
                shotFired = true;
            }

            if (!shot) {

                shotFired = false;
                return;
            }

        }
        else
        {
            toShootAt = null;
        }


       
    }
}
