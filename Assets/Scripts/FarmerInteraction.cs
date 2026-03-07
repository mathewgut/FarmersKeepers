using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FarmerInteraction : NetworkBehaviour
{
    
    public GameManagement manager;
    public bool isEnemy = false;
    [SerializeField] Canvas BuildUI;
    [SerializeField] Canvas VersusUI; // for the versus mode, client is in charge of spawning enemies
    [SerializeField] GameObject playerCamera;
    public NetworkVariable<bool> IsInBuildMode = new NetworkVariable<bool>(false);
    [SerializeField] private AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
       
        if (IsOwner)
        {
            // if in versus mode and user is client (i got one hour to finish, sorry brodie you perma enemy)
            if (!IsHost && IsClient && GameManagement.Instance.gameType == GameManagement.GAME_TYPE.Versus)
            {
                BuildUI = VersusUI;
                isEnemy = true;
            }

            // subscribe to changes of inBuildMode
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
        
    }

    [ServerRpc]
    void ToggleBuildModeServerRpc() {
        IsInBuildMode.Value = !IsInBuildMode.Value;
    }

    

}
