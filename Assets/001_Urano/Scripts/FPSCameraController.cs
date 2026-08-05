using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private DirController dirController;
    [SerializeField] private Transform cameraTransform;

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool rotateBodyWithYaw = true;

    [Header("接地判定")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("ジャンプ")]
    [SerializeField] private float jumpForce = 5f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 物理で勝手に転がらないようにする
    }

    private void Start()
    {
        if (dirController == null)
        {
            dirController = DirController.Instance;
        }
    }

    private void Update()
    {
        if (dirController == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // DirControllerのYaw基準で移動方向を計算（水平面のみ）
        Vector3 dir = dirController.GetFlatForward() * v + dirController.GetFlatRight() * h;
        moveInput = Vector3.ClampMagnitude(dir, 1f);

        // 接地判定
        isGrounded = groundCheck != null &&
            Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
        Jump();
    }

    private void LateUpdate()
    {
        ApplyPitch();
    }

    private void ApplyPitch()
    {
        if (cameraTransform == null || dirController == null) return;
        cameraTransform.localRotation = dirController.GetPitchRotation();
    }

    private void Move()
    {
        Vector3 targetVelocity = moveInput * moveSpeed;
        // Y方向(重力・ジャンプ)の速度は既存のrb.velocityを維持してXZだけ上書きする
        Vector3 velocity = rb.linearVelocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.linearVelocity = velocity;
    }

    private void Rotate()
    {
        if (!rotateBodyWithYaw || dirController == null) return;
        rb.MoveRotation(dirController.GetYawRotation());
    }

    private void Jump()
    {
        if (!jumpRequested) return;
        jumpRequested = false;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}