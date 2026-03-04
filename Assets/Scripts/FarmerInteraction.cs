using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FarmerInteraction : NetworkBehaviour
{
    
    public GameManagement manager;
    [SerializeField] Canvas BuildUI;
    public NetworkVariable<bool> IsInBuildMode = new NetworkVariable<bool>(false);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GameManagement.Instance;
        Debug.Log("manager:", manager);
    }

    public override void OnNetworkSpawn()
    {
        // subscribe to changes of inBuildMode
        if (IsOwner)
        {
            IsInBuildMode.OnValueChanged += (oldValue, newValue) => {

                BuildUI.gameObject.SetActive(newValue);
            };
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if(manager == null) manager = GameManagement.Instance;

        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildModeServerRpc();
        }

        bool BuildActive = IsInBuildMode.Value && manager.gameState.Value == GameManagement.GAME_STATE.Prepare;

    }

    [ServerRpc]
    void ToggleBuildModeServerRpc() {
        IsInBuildMode.Value = !IsInBuildMode.Value;
    }

    

}
