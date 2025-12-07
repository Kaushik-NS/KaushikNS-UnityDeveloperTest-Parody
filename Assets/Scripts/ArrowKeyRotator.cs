using UnityEngine;

public class ArrowKeyRotator : MonoBehaviour
{
    public float rotateSpeed = 10f; 
    public GameObject childObject;  
    public Transform playerTransform; 

    private Quaternion targetRotation;
    private bool rotatePlayerToTarget = false;

    void Start()
    {
        targetRotation = transform.rotation;
        if (childObject != null) childObject.SetActive(false);
    }

    void Update()
    {
        bool arrowKeyPressed = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            targetRotation = Quaternion.Euler(0f, 0f, 90f);
            arrowKeyPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            targetRotation = Quaternion.Euler(0f, 0f, -90f);
            arrowKeyPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            targetRotation = Quaternion.Euler(90f, 0f, 0f);
            arrowKeyPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            targetRotation = Quaternion.Euler(-90f, 0f, 0f);
            arrowKeyPressed = true;
        }

        if (arrowKeyPressed && childObject != null)
            childObject.SetActive(true);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (childObject != null) childObject.SetActive(false);
            rotatePlayerToTarget = true;
        }

        // rotate hologram
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        // Smoothly rotate player if flag is set (this rotates the player transform)
        if (rotatePlayerToTarget && playerTransform != null)
        {
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            if (Quaternion.Angle(playerTransform.rotation, targetRotation) < 0.1f)
            {
                playerTransform.rotation = targetRotation;
                rotatePlayerToTarget = false;
            }
        }
    }

    public bool ShouldRotatePlayer() => rotatePlayerToTarget;
    public Quaternion GetTargetRotation() => targetRotation;
}
