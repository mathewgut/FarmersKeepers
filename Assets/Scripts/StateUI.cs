using UnityEngine;
using TMPro;
using Unity.Netcode;
public class StateUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI builtText;
    [SerializeField] private TextMeshProUGUI waveText;
    GameManagement manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = GameManagement.Instance;
        gameStateText.text = "Waiting for client...";
    }


    // Update is called once per frame
    void Update()
    {

        builtText.text = manager.builtItems.Value.ToString() + "/" + manager.maxBuiltItems.Value.ToString();
        waveText.text = manager.wave.Value.ToString();

        if (manager.gameState.Value == GameManagement.GAME_STATE.Waiting)
        {
            timerText.gameObject.SetActive(false);
            gameStateText.text = "Waiting for client...";
        }

        else if (manager.gameState.Value == GameManagement.GAME_STATE.Prepare)
        {
            timerText.gameObject.SetActive(true);

            if (manager.prepareTimer.Value <= 10) timerText.color = new Color(229, 60, 60, 1);
            else timerText.color = new Color(255, 255, 255, 1);

            timerText.text = manager.prepareTimer.Value.ToString();
            gameStateText.text = "Prepare your defenses.";

        }
        else if (manager.gameState.Value == GameManagement.GAME_STATE.Defend)
        {
            timerText.gameObject.SetActive(false);
            gameStateText.text = "Protect your animals!";
        }
        else if(manager.gameState.Value == GameManagement.GAME_STATE.Lost)
        {
            if(manager.gameType == GameManagement.GAME_TYPE.Versus && IsClient && !IsHost)
            {
                gameStateText.text = "You won! :)";
            }
            else
            {
                gameStateText.text = "You lost. :(";
            }

                timerText.gameObject.SetActive(false);
        }

        else
        {
            timerText.gameObject.SetActive(false);
        }
    }
}
