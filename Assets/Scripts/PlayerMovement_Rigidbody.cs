using UnityEngine;
using UnityEngine.Rendering;

//[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement_Rigidbody : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float gravityMagnitude = 9.81f;

    public Animator animator;

    public Transform CameraFollowTarget;
    public Transform cameraTransform;

    public ArrowKeyRotator hologramRotator;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector3 gravityDirection = Vector3.down;
    private bool prevHoloRotateFlag = false;

    public float rotationSmooth = 10f;

    public GameManager GM;

    // Free fall check paramters
    public float fallSpeedThreshold = 10f;    
    public float fallDuration = 3f;
    private float fallTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true; 
    }

    void Update()
    {
        if (!GM.GetComponent<GameManager>().IsGameOver)
        {
            HandleMovement();
            HandleGravityRotation();
            LateUpdateCameraFollowPosition();
            HandleAnimation();
        }
    }

    void FixedUpdate()
    {
        //Fall detection
       
        float speedAlongGravity = Vector3.Dot(rb.velocity, gravityDirection.normalized);
        bool fallingFast = speedAlongGravity > fallSpeedThreshold;

        if (fallingFast)
        {
            fallTimer += Time.fixedDeltaTime;
            if (fallTimer >= fallDuration && !GM.IsGameOver)
            {
                GM.ShowGameOverPanel();
                GM.IsGameOver = true;
            }
        }
        else
        {
            fallTimer = 0f;
        }

        if (!GM.IsGameOver)
        {
            ApplyCustomGravity();
        }

    }

    void HandleMovement()
    {
        float x = 0f;
        float z = 0f;

     

        if (Input.GetKey(KeyCode.W)) z = 1;
        if (Input.GetKey(KeyCode.S)) z = -1;
        if (Input.GetKey(KeyCode.A)) x = -1;
        if (Input.GetKey(KeyCode.D)) x = 1;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, gravityDirection).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, gravityDirection).normalized;

        Vector3 moveDir = forward * z + right * x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Vector3 vel = Vector3.Project(rb.velocity, gravityDirection);
        Vector3 horizVel = moveDir * moveSpeed;

        rb.velocity = horizVel + vel;

        // rotate
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, -gravityDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmooth * Time.deltaTime);
        }

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.velocity += -gravityDirection * jumpForce;
        }
    }

    bool IsGrounded()
    {
        float extra = 0.1f;
        return Physics.Raycast(transform.position, gravityDirection, (capsule.height / 2f) + extra);
    }

    void ApplyCustomGravity()
    {
        rb.AddForce(gravityDirection * gravityMagnitude, ForceMode.Acceleration);
    }

    void HandleGravityRotation()
    {
        if (hologramRotator == null) return;

        bool active = hologramRotator.ShouldRotatePlayer();

        if (prevHoloRotateFlag && !active)
        {
            Quaternion target = hologramRotator.GetTargetRotation();
            gravityDirection = (target * Vector3.down).normalized;

            transform.rotation = target;
            rb.velocity -= Vector3.Project(rb.velocity, gravityDirection);
        }

        prevHoloRotateFlag = active;
    }



    private void LateUpdateCameraFollowPosition()
    {
        if (CameraFollowTarget == null) return;

        Vector3 playerPos = transform.position;

        Vector3 forwardTangent = Vector3.ProjectOnPlane(transform.forward, gravityDirection).normalized;
        if (forwardTangent.sqrMagnitude < 0.001f)
            forwardTangent = transform.forward;

        float distance = 4f;
        float height = 1.6f;

        Vector3 desiredPos =
            playerPos
            - forwardTangent * distance
            + (-gravityDirection) * height;

        CameraFollowTarget.position = desiredPos;
        Quaternion desiredRot = Quaternion.LookRotation(playerPos - desiredPos, -gravityDirection);

        CameraFollowTarget.rotation = desiredRot;
    }



    void HandleAnimation()
    {
        if (animator == null) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        bool isMoving = (x != 0 || z != 0);
        animator.SetBool("IsRun", isMoving);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            GameManager.Instance.AddScore(1);
            Destroy(other.gameObject);
        }
    }
}
