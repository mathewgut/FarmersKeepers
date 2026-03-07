using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using static TowerBehaviour;

public class MenuUI : MonoBehaviour
{
    public enum MENU_TYPE
    {
        Coop,
        Versus
    }

    public MENU_TYPE menuType = MENU_TYPE.Coop;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(menuType == MENU_TYPE.Coop)
        {
            hostBtn.onClick.AddListener(() => {
                NetworkManager.Singleton.StartHost();
                SwitchScene();
            });

            clientBtn.onClick.AddListener(() => {
                NetworkManager.Singleton.StartClient();
            });
        }
        else
        {
            hostBtn.onClick.AddListener(() => {
                NetworkManager.Singleton.StartHost();
                SwitchScene("VersusMainScene");
            });

            clientBtn.onClick.AddListener(() => {
                NetworkManager.Singleton.StartClient();
            });
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SwitchScene(string scene = "MainScene")
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                scene,
                LoadSceneMode.Single
            );
        }
    }
}
