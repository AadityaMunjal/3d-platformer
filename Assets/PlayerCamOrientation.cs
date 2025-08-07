using UnityEngine;

public class PlayerCamOrientation : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;

    public Transform orientation;

    float xRotation;
    float yRotation;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false; 
    }

    void Update()
    {
        // legacy input
        float mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        if (orientation != null)
            orientation.localRotation = Quaternion.Euler(0, yRotation, 0);
    }
}
