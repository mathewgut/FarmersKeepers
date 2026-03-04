using UnityEngine;
using TMPro;
using Unity.Netcode;
public class StateUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI timerText;
    GameManagement manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GameManagement.Instance;
        gameStateText.text = "Waiting for client...";
    }

    // avoids creating multiple UI's

    /*
    public override void OnNetworkSpawn()
    {
       
        if (!IsOwner)
        {
            Destroy(gameObject);
        }
    }
*/

    // Update is called once per frame
    void Update()
    {

        if (manager.prepareStarted.Value)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = manager.prepareTimer.Value.ToString();
            gameStateText.text = "Prepare your defenses!";

        }
        else
        {
            timerText.gameObject.SetActive(false);
        }
    }
}
