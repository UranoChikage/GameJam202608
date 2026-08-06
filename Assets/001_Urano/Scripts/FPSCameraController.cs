using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
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
    [SerializeField] private float crouchSpeed = 8f;
    [SerializeField] private float crouchMoveSpeed = 2.5f;
    [SerializeField] private float crouchCameraOffset = -0.5f;

    [Header("見た目")]
    [SerializeField] private Transform bodyVisual;
    [SerializeField] private float crouchBodyScaleY = 0.5f;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    private Vector3 moveInput;
    private bool isGrounded;
    private bool jumpRequested;
    private bool isCrouching;

    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 standingCameraPosition;
    private Vector3 standingBodyScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider =
            GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;

        // 立っている状態を保存
        standingHeight = capsuleCollider.height;
        standingCenter = capsuleCollider.center;

        if (cameraTransform != null)
        {
            standingCameraPosition =
                cameraTransform.localPosition;
        }

        if (bodyVisual != null)
        {
            standingBodyScale =
                bodyVisual.localScale;
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
        if (dirController == null)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed)
            horizontal -= 1f;

        if (keyboard.dKey.isPressed)
            horizontal += 1f;

        if (keyboard.sKey.isPressed)
            vertical -= 1f;

        if (keyboard.wKey.isPressed)
            vertical += 1f;

        Vector3 direction =
            dirController.GetFlatForward() * vertical +
            dirController.GetFlatRight() * horizontal;

        moveInput =
            Vector3.ClampMagnitude(direction, 1f);

        // 接地判定
        isGrounded =
            groundCheck != null &&
            Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundMask
            );

        // ジャンプ
        if (isGrounded &&
            keyboard.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        // 左Ctrlを押している間しゃがむ
        isCrouching =
            keyboard.leftCtrlKey.isPressed;
    }

    private void FixedUpdate()
    {
        ApplyCrouchCollider();
        Move();
        Rotate();
        Jump();
    }

    private void LateUpdate()
    {
        ApplyPitch();
        ApplyCrouchCamera();
        ApplyCrouchVisual();
    }

    private void Move()
    {
        float currentSpeed =
            isCrouching
                ? crouchMoveSpeed
                : moveSpeed;

        Vector3 targetVelocity =
            moveInput * currentSpeed;

        Vector3 velocity = rb.linearVelocity;

        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;

        rb.linearVelocity = velocity;
    }

    private void Rotate()
    {
        if (!rotateBodyWithYaw ||
            dirController == null)
        {
            return;
        }

        rb.MoveRotation(
            dirController.GetYawRotation()
        );
    }

    private void Jump()
    {
        if (!jumpRequested)
            return;

        jumpRequested = false;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
    }

    private void ApplyPitch()
    {
        if (cameraTransform == null ||
            dirController == null)
        {
            return;
        }

        cameraTransform.localRotation =
            dirController.GetPitchRotation();
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

        // 足元の位置を維持
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
            targetPosition.y +=
                crouchCameraOffset;
        }

        cameraTransform.localPosition =
            Vector3.Lerp(
                cameraTransform.localPosition,
                targetPosition,
                crouchSpeed * Time.deltaTime
            );
    }

    private void ApplyCrouchVisual()
    {
        if (bodyVisual == null)
            return;

        Vector3 targetScale =
            standingBodyScale;

        if (isCrouching)
        {
            targetScale.y *=
                crouchBodyScaleY;
        }

        bodyVisual.localScale =
            Vector3.Lerp(
                bodyVisual.localScale,
                targetScale,
                crouchSpeed * Time.deltaTime
            );
    }

    public void ApplySpeedBoost(
        float value,
        float time)
    {
        StartCoroutine(
            SpeedBoostCoroutine(value, time)
        );
    }

    private IEnumerator SpeedBoostCoroutine(
        float value,
        float time)
    {
        moveSpeed += value;

        Debug.Log(
            time + "秒間、速度が" +
            value + "上がりました"
        );

        yield return new WaitForSeconds(time);

        moveSpeed -= value;

        Debug.Log("速度アップが終了しました");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}