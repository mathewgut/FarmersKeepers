using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static GameManagement;


public class TowerBehaviour : NetworkBehaviour
{

    public enum TOWER_TYPE
    {
        Project,
        Area,
        Obstacle
    }

    public TOWER_TYPE towerType = TOWER_TYPE.Project;

    [SerializeField] TowerProximity proximity;
    MeshRenderer proximityMesh;
    [SerializeField] Material invisibleProxMat;
    Material originalProxMat;

    [SerializeField] float cooldown = 1.5f;
    [SerializeField] int targetCount = 1; // if -1, will shoot all targets within proximity range

    float prevTime = -1;
    public GameObject projectile;
    GameObject shot = null;
    public bool shotFired = false;

    public float yShootOffset = 0f;
    Vector3 projectilePosition;
    Vector3 projectileStartPosition;
    bool isBuildMode;

    public float shootAtDistance = 10f;
    
    void Start()
    {
        projectileStartPosition = new Vector3(transform.position.x, yShootOffset, transform.position.z);
        projectilePosition = projectileStartPosition;
        proximityMesh = proximity.GetComponent<MeshRenderer>();
        originalProxMat = proximityMesh.material;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        if(IsClient)
        {
            isBuildMode = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<FarmerInteraction>().IsInBuildMode.Value;

            // changes material of range overlay depending on build mode
            if (!isBuildMode)
            {
                proximityMesh.material = invisibleProxMat;
            }
            else
            {
                proximityMesh.material = originalProxMat;
            }
        }
        


        if (!IsServer) return;

        // if tower has no targets, do nothing
        if (proximity.targetsInRange.Count == 0) {
            return;
        }

        // if at least one target
        else
        {
            
            // if cooldown not passed and timer has been started
            if (Time.time - prevTime < cooldown && prevTime != -1) return;

            if (targetCount == 1)
            {
                Shoot(proximity.GetTarget());
            }
            else
            {
                List<GameObject> selectedTargets = proximity.GetMultipleTargets(targetCount);

                foreach (GameObject target in selectedTargets)
                {
                    Shoot(target);
                }
            }
            
            prevTime = Time.time;
        }
    }

    void Shoot(GameObject target)
    {
        shot = Instantiate(projectile, projectilePosition, Quaternion.identity);
        shot.GetComponent<NetworkObject>().Spawn();
        shot.GetComponent<ProjectileBehaviour>().target = target;
    }

}
