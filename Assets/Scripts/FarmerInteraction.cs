using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FarmerInteraction : NetworkBehaviour
{
    
    public GameManagement manager;
    [SerializeField] Canvas BuildUI;
    [SerializeField] GameObject playerCamera;
    public NetworkVariable<bool> IsInBuildMode = new NetworkVariable<bool>(false);
    [SerializeField] private AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
       
        if (IsOwner)

        {   // subscribe to changes of inBuildMode
            IsInBuildMode.OnValueChanged += (oldValue, newValue) => {

                BuildUI.gameObject.SetActive(newValue);
            };

            // only one camera and audio listener instance per client
            playerCamera.gameObject.SetActive(true);
            audioListener.gameObject.SetActive(true);
            playerCamera.tag = "MainCamera";
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
            audioListener.gameObject.SetActive(false);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GameManagement.Instance;
        Debug.Log("manager:", manager);

        //transform.parent.transform.position = new Vector3(0, 25, 0);
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

        //bool BuildActive = IsInBuildMode.Value && manager.gameState.Value == GameManagement.GAME_STATE.Prepare;

        
    }

    [ServerRpc]
    void ToggleBuildModeServerRpc() {
        IsInBuildMode.Value = !IsInBuildMode.Value;
    }

    

}
