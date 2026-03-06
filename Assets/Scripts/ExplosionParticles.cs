using UnityEngine;

public class ExplosionParticles : MonoBehaviour
{
    void Start() {
        // i didnt know destroy had a time variable, oh boy thats cool
        Destroy(gameObject, 3f);
    }  
}