using UnityEngine;

public class PlayerCameraMovement : MonoBehaviour
{
    public Transform cameraPos;

    void Update()
    {
        transform.position = cameraPos.position;
    }

}
