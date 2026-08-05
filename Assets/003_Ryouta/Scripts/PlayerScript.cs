using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
   
    [SerializeField] float moveSpeed = 5f;//移動速度
    [SerializeField] float jumpHeight = 1.5f;//ジャンプの高さ
    [SerializeField] float gravity = -20f;//重力の強さ

    //視点
     Transform playerCamera;//動かすカメラ
    [SerializeField, Range(0.01f, 1f)]
    float CameraSpeed = 0.1f;//カメラの感度
     float lookLimit = 90f;//カメラの限界角度

    CharacterController controller;
    float verticalVelocity;
    float cameraPitch;
    bool cursorLocked = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>()?.transform;

        if (playerCamera == null)
        {
            Debug.LogError(
                "Playerの子にCameraがありません。",
                this
            );

            enabled = false;
            return;
        }

        LockCursor();
    }

    public void Update()
    {
        HandleCursor();

        if (cursorLocked)
            Look();

        Move();
    }

    public void Move()
    {
        if (Keyboard.current == null)
            return;

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1f;//前移動

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1f;//後ろ移動

        if (Keyboard.current.dKey.isPressed)
            input.x += 1f;//右移動

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1f;//左移動

        // 斜め移動が速くなることを防ぐ
        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 direction =
            transform.right * input.x +
            transform.forward * input.y;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

   public void Look()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * CameraSpeed;
        float mouseY = mouseDelta.y * CameraSpeed;

        // 左右はPlayer全体を回す
        transform.Rotate(Vector3.up * mouseX);

        // 上下はカメラだけを回す
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -lookLimit,
            lookLimit
        );

        playerCamera.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

   public void HandleCursor()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
                UnlockCursor();
        }

        if (!cursorLocked &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            LockCursor();
        }
    }

   public void LockCursor()
    {
        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

   public void UnlockCursor()
    {
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Use()//使う
    {

    }

    public　void Drop()//落とす
    {

    }
    public void PickUp()//持つ
    {

    }
}