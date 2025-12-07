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


        playerBody.Rotate(Vector3.up * mouseX);
    }
}