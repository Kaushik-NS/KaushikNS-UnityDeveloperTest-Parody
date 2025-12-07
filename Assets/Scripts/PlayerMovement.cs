using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float gravityMagnitude = 9.81f;        
    public float jumpHeight = 2f;

    private CharacterController controller;
    private Vector3 velocity = Vector3.zero;       

    public Animator animator;

    public Transform groundCheck;                   // a point under the player
    public LayerMask groundMask;
    public float groundCheckDistance = 0.2f;

    public float rotateSpeed = 10f;                 // used when hologram requests smooth rotation

    [SerializeField] Transform CameraFollowTarget;
    public Transform cameraTransform;

    // Hook to hologram rotator
    public ArrowKeyRotator hologramRotator;

    // Runtime gravity direction (normalized). Default points down.
    private Vector3 gravityDirection = Vector3.down;

    // Used to detect transition from rotating -> finished
    private bool prevHoloRotateFlag = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- Ground check along current gravity direction ---
        bool isGrounded = false;
        RaycastHit hit;
        // Raycast from slightly above the controller center to detect ground in gravityDir
        Vector3 rayOrigin = transform.position + -gravityDirection * 0.1f; // small offset so start isn't exactly on surface
        if (Physics.Raycast(rayOrigin, gravityDirection, out hit, groundCheckDistance + 0.1f, groundMask))
        {
            isGrounded = true;
        }

        // If grounded and velocity has a component along gravityDirection pointing into ground, remove it
        float velAlongG = Vector3.Dot(velocity, gravityDirection);
        if (isGrounded && velAlongG < 0f)
        {
            // remove downward component so we stay attached
            velocity -= Vector3.Project(velocity, gravityDirection);
        }

        // -------------------------
        // WASD ONLY MOVEMENT INPUT
        // -------------------------
        float z = 0f;
        float x = 0f;

        if (Input.GetKey(KeyCode.W)) z = 1;
        if (Input.GetKey(KeyCode.S)) z = -1;
        if (Input.GetKey(KeyCode.A)) x = -1;
        if (Input.GetKey(KeyCode.D)) x = 1;

        // Camera-relative movement (world forward/right projected to plane orthogonal to gravity)
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Remove vertical component relative to camera to avoid moving into gravity
        camForward = Vector3.ProjectOnPlane(camForward, gravityDirection).normalized;
        camRight = Vector3.ProjectOnPlane(camRight, gravityDirection).normalized;

        Vector3 moveDirection = (camForward * z + camRight * x);
        if (moveDirection.sqrMagnitude > 1f) moveDirection.Normalize();

        // Move (preserve any non-gravity components of velocity)
        Vector3 horizontalMotion = moveDirection * moveSpeed;

        // Jump: apply velocity opposite to gravityDirection
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // v = sqrt(2 * g * h) along -gravityDirection (i.e., away from ground)
            float jumpSpeed = Mathf.Sqrt(2f * gravityMagnitude * jumpHeight);
            velocity += -gravityDirection * jumpSpeed;
        }

        // Gravity: apply gravity vector to velocity
        velocity += gravityDirection * gravityMagnitude * Time.deltaTime;

        // Final move vector: horizontalMotion + velocity (both are world-space)
        Vector3 finalMove = horizontalMotion + velocity;

        controller.Move(finalMove * Time.deltaTime);

        // Rotate player toward movement direction (only around axis perpendicular to gravity)
        if (moveDirection != Vector3.zero)
        {
            // desired forward is moveDirection projected to plane orthogonal to gravity (already done above)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, -gravityDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        // Animation
        bool isMoving = (x != 0 || z != 0);
        if (animator != null) animator.SetBool("IsRun", isMoving);

        // ------------- handle hologram-driven rotation + gravity alignment -------------

        if (hologramRotator != null)
        {
            bool currentHoloFlag = hologramRotator.ShouldRotatePlayer();

            // If we detect transition from rotating -> finished, update gravity direction to hologram target
            if (prevHoloRotateFlag && !currentHoloFlag)
            {
                Quaternion holoTarget = hologramRotator.GetTargetRotation();
                gravityDirection = holoTarget * Vector3.down; // align gravity
                gravityDirection.Normalize();

                // also ensure player's rotation is exactly the hologram target (safety)
                transform.rotation = holoTarget;
                // ensure vertical component of velocity along new gravity is removed so player doesn't immediately snap/fly
                velocity -= Vector3.Project(velocity, gravityDirection);
            }

            prevHoloRotateFlag = currentHoloFlag;
        }

        // -------------------------
        // Gravity-correct camera follow
        // -------------------------
        LateUpdateCameraFollowPosition();
    }

    private void LateUpdateCameraFollowPosition()
    {
        // Keep camera follow target at same height (Y in world) as before — this behavior may need tweaking for rotated gravity.
        // We'll simply keep camera's follow target x/z anchored to player; leave its y unchanged.
        Vector3 camPos = CameraFollowTarget.position;
        CameraFollowTarget.position = new Vector3(
            transform.position.x,
            camPos.y,
            transform.position.z
        );
    }
}
