using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = Vector3.zero;

    void Start()
    {
        AudioManager.instance.Play("BGM");
    }
    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            transform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, transform.position.z) + offset;
        }
    }
}
