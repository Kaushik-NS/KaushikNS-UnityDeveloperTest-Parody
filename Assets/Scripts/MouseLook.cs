using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 150f;
    public Transform playerBody;

    float xRotation = 0f;

    public float minYaw = -90f;
    public float maxYaw = 90f;

    float currentYaw = 0f;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 0.5f * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;


        //xRotation -= mouseY;
        //xRotation = Mathf.Clamp(xRotation, -75f, 75f); // prevent flipping
        //transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        //currentYaw += mouseX;
        //currentYaw = Mathf.Clamp(currentYaw, minYaw, maxYaw);

        //playerBody.localRotation = Quaternion.Euler(0, currentYaw, 0);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}