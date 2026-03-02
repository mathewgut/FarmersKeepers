using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FarmerInteraction : NetworkBehaviour
{
    
    public GameManagement manager;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GameManagement.Instance;
        Debug.Log("manager:", manager);
    }    
    
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildModeServerRpc();
        }
    }

    [ServerRpc]
    void ToggleBuildModeServerRpc() {
        IsInBuildMode.Value = !IsInBuildMode.Value;
    }

    public NetworkVariable<bool> IsInBuildMode = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);


}
