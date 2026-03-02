using UnityEngine;
using UnityEngine.InputSystem;

public class CameraEx : MonoBehaviour
{
    [SerializeField]
    float SENSITIVITY = 0.2f;
    [SerializeField]
    float MOVE_SENSITIVITY = 0.05f;

    [SerializeField] 
    float zoomMult;

    public float minZoom;
    public float maxZoom;

    float minPitch = 5;
    float maxPitch = 85;

    public float maxDistance = 10;

    Vector3 targetPos = Vector3.zero;
    Vector3 pos = Vector3.zero;
    
    // goto
    float targetDist = 3;
    
    // current
    float dist = 8;
    float yaw;
    float pitch = 0.52f;

    Vector2 mouseDelta;

    Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = transform.GetChild(0).GetComponent<Camera>();
    }

    void RotateAroundTarget()
    {
        yaw += mouseDelta.x;
        pitch -= mouseDelta.y;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, transform.rotation.z);
    }

    void ZoomCam ()
    {
        targetDist += (Input.GetAxis("Mouse ScrollWheel") * -1) * targetDist;
        targetDist = Mathf.Clamp(targetDist, minZoom, maxZoom);
    }

    void Move()
    {
        float inputZ = Input.GetKey(KeyCode.W) ? 1 : 0;
        inputZ += Input.GetKey(KeyCode.S) ? -1 : 0;
        float inputX = Input.GetKey(KeyCode.A) ? 1 : 0;
        inputX += Input.GetKey(KeyCode.D) ? -1 : 0;

        inputZ *= MOVE_SENSITIVITY;
        inputX *= MOVE_SENSITIVITY;

        Vector3 unitCamTransform = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;

        targetPos += unitCamTransform * inputZ;
        targetPos += -cam.transform.right * inputX;

        // keep cam in specified range
        targetPos = new Vector3(
            Mathf.Clamp(targetPos.x, -maxDistance, maxDistance),
            targetPos.y,
            Mathf.Clamp(targetPos.z, -maxDistance, maxDistance)
        );

        pos = Vector3.Lerp(pos, targetPos, Time.deltaTime * 6);
    }

    Vector3 GetPositionRelativeToTarget()
    {
        return pos + (-cam.transform.forward * dist);
    }

    // Update is called once per frame
    void Update()
    {
        mouseDelta = Mouse.current.delta.ReadValue() * SENSITIVITY;
        dist = Mathf.Lerp(dist, targetDist, 6 * Time.deltaTime);
        
        if(Input.GetMouseButton(1)) RotateAroundTarget();

        transform.position = GetPositionRelativeToTarget();

        Move();
        ZoomCam();
    }
}