using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class AnimalPenBehaviour : NetworkBehaviour
{
    [SerializeField] List<GameObject> animals = new List<GameObject>();
    [SerializeField] Canvas animalCanvas;
    [SerializeField] TMPro.TextMeshProUGUI countText;

    float lastPlay = -1;
    Camera main;
    public NetworkVariable<int> animalCount = new NetworkVariable<int>(5);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        main = Camera.main;
    }

    // Update is called once per frame
    void Update()

    {
        


        countText.text = animalCount.Value.ToString() + "/" + "5";

        if (Vector3.Distance(main.transform.position, transform.position) <= 8f)
        {
            if (lastPlay == -1 || Time.time - lastPlay > 5)
            {
                AudioSource.PlayClipAtPoint(GameManagement.Instance.AnimalAudios[GameManagement.Instance.GetRandomAudioAnimal()], transform.position, 1f);
                lastPlay = Time.time;
            }
        }
        if (main)
        {
            animalCanvas.transform.LookAt(main.transform.position);
            animalCanvas.transform.rotation = Quaternion.Euler(
                animalCanvas.transform.rotation.eulerAngles.x,
                animalCanvas.transform.rotation.eulerAngles.y - 180,
                animalCanvas.transform.rotation.eulerAngles.z
            );
        }

        if (!IsServer) return;

        animalCount.Value = animals.Count;

        if (animals.Count == 0)
        {
            GameManagement.Instance.gameState.Value = GameManagement.GAME_STATE.Lost;
        }
        

    }

    private void OnTriggerStay(Collider other)
    {
        if (IsServer && other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyAI>().ReachedTarget();
        }
    }

    // doesnt sync with client side :/
    [ServerRpc(RequireOwnership = false)]
    public void RemoveAnimalServerRpc()
    {
        if (animals.Count > 0)
        {
            int toRemove = Random.Range(0, animals.Count);
            animals[toRemove].GetComponent<NetworkObject>().Despawn();
            animals.RemoveAt(toRemove);
        }
        else
        {
            GameManagement.Instance.gameState.Value = GameManagement.GAME_STATE.Lost;
        }
    }

}
