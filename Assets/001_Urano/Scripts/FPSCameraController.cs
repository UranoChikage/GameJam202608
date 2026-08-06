using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("しゃがみ")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchMoveSpeed = 2.5f;
    [SerializeField] private float crouchCameraOffset = -0.5f;
    [SerializeField] private float crouchSpeed = 10f;

    private CapsuleCollider capsuleCollider;
    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 standingCameraPosition;
    private bool isCrouching;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;

        // 立っている状態を保存
        standingHeight = capsuleCollider.height;
        standingCenter = capsuleCollider.center;

        if (cameraTransform != null)
        {
            standingCameraPosition =
                cameraTransform.localPosition;
        }
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
        //Debug.Log(isGrounded);
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
            Debug.Log("aaaaaaa");
        }
        if (Keyboard.current != null)
        {
            isCrouching =
                Keyboard.current.leftCtrlKey.isPressed;
        }
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
        Jump();
        ApplyCrouchCollider();
    }

    private void LateUpdate()
    {
        ApplyPitch();
        ApplyCrouchCamera();
    }

    private void ApplyPitch()
    {
        if (cameraTransform == null || dirController == null) return;
        cameraTransform.localRotation = dirController.GetPitchRotation();
    }

    
        private void Move()
    {
        float currentSpeed =
            isCrouching
                ? crouchMoveSpeed
                : moveSpeed;

        Vector3 targetVelocity =
            moveInput * currentSpeed;
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
    private void ApplyCrouchCollider()
    {
        float targetHeight =
            isCrouching
                ? crouchHeight
                : standingHeight;

        capsuleCollider.height = Mathf.Lerp(
            capsuleCollider.height,
            targetHeight,
            crouchSpeed * Time.fixedDeltaTime
        );

        // Colliderの下端が動かないようにする
        float standingBottom =
            standingCenter.y -
            standingHeight / 2f;

        Vector3 center = standingCenter;

        center.y =
            standingBottom +
            capsuleCollider.height / 2f;

        capsuleCollider.center = center;
    }

    private void ApplyCrouchCamera()
    {
        if (cameraTransform == null)
            return;

        Vector3 targetPosition =
            standingCameraPosition;

        if (isCrouching)
        {
            targetPosition.y += crouchCameraOffset;
        }

        cameraTransform.localPosition =
            Vector3.Lerp(
                cameraTransform.localPosition,
                targetPosition,
                crouchSpeed * Time.deltaTime
            );
    }
}