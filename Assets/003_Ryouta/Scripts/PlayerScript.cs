using UnityEngine;
[RequireComponent(typeof(CharacterController))]

public class PlayerScript : MonoBehaviour
{
     //移動
     float PlayerSpeed = 5f;//移動速度
     float jumpHeight = 1.5f;//ジャンプの高さ
     float gravity = -20f;//重力

     //視点
     Transform playerCamera;//動かすカメラ
     float mouseSensitivity = 2f;//マウス感度
     float lookLimit = 90f;//下を向ける限界角度

    CharacterController controller;
    float verticalVelocity;
    float cameraPitch;
    bool cursorLocked = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        LockCursor();
        //ゲーム開始時にカーソルを画面中央へ固定
    }

    void Update()
    {
        HandleCursor();

        if (cursorLocked)
            Look();

        Move();
    }

    void Move()
    {
        // A・D
        float horizontal = Input.GetAxisRaw("Horizontal");

        // W・S
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction =
            transform.right * horizontal +
            transform.forward * vertical;

        // 斜め移動が速くなるのを防止
        direction = direction.normalized;

        // 地面にいるとき
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            // Spaceでジャンプ
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        // 重力
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * PlayerSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        float mouseX =
            Input.GetAxis("Mouse X") * mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") * mouseSensitivity;

        // プレイヤーを左右に回転
        transform.Rotate(Vector3.up * mouseX);

        // カメラを上下に回転
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -lookLimit,
            lookLimit
        );

        playerCamera.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    void HandleCursor()
    {
        // Escでカーソル解除
        if (Input.GetKeyDown(KeyCode.Escape))
            UnlockCursor();

        // 左クリックで再固定
        if (!cursorLocked && Input.GetMouseButtonDown(0))
            LockCursor();
    }

    void LockCursor()
    {
        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}