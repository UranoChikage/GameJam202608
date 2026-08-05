using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{

    //[SerializeField] float moveSpeed = 5f;//移動速度
    //[SerializeField] float jumpHeight = 1.5f;//ジャンプの高さ
    //[SerializeField] float gravity = -20f;//重力の強さ

    //視点
    [SerializeField] Transform playerCamera;//動かすカメラ
    //[SerializeField, Range(0.01f, 1f)]
    //float CameraSpeed = 0.1f;//カメラの感度
     //float lookLimit = 90f;//カメラの限界角度

    CharacterController controller;
    float verticalVelocity;
    //float cameraPitch;
   // bool cursorLocked = true;

    [SerializeField] Transform holdPosition;
    [SerializeField] float pickUpDistance = 3f;

    [SerializeField] float rayDistance = 3f;

   
    IItem heldItem;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

       // if (playerCamera == null)
         //   playerCamera = GetComponentInChildren<Camera>()?.transform;

        //if (playerCamera == null)
        //{
            //Debug.LogError(
            //    "PlayerにCameraがありません。",
            //    this
           // );

           // enabled = false;
           // return;
       // }

        //LockCursor();
    }

    public void Update()
    {
       // HandleCursor();

       // if (cursorLocked)
         //   Look();

        Move();
        if (Keyboard.current.eKey.isPressed) { Use(); }

        if (Keyboard.current != null &&
    Keyboard.current.fKey.wasPressedThisFrame)//押した瞬間だけ
        {
            DropOrPickUp();
            
            Debug.Log("Fキーを押しました");
            
        }

    }

    public void Move()
    {
       // if (Keyboard.current == null)
        //    return;

      //  Vector2 input = Vector2.zero;

        //if (Keyboard.current.wKey.isPressed)
         //   input.y += 1f;//前移動

       // if (Keyboard.current.sKey.isPressed)
         //   input.y -= 1f;//後ろ移動
        //
        //if (Keyboard.current.dKey.isPressed)
         //   input.x += 1f;//右移動

       // if (Keyboard.current.aKey.isPressed)
          //  input.x -= 1f;//左移動

        // 斜め移動が速くなることを防ぐ
        //input = Vector2.ClampMagnitude(input, 1f);

           //Vector3 direction =
         //   transform.right * input.x +
          //  transform.forward * input.y;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            //if (Keyboard.current.spaceKey.wasPressedThisFrame)
           // {
            //    verticalVelocity =
            //        Mathf.Sqrt(jumpHeight * -2f * gravity);
            //}
        }

        //verticalVelocity += gravity * Time.deltaTime;

        //Vector3 velocity = direction * moveSpeed;
        //velocity.y = verticalVelocity;

       // controller.Move(velocity * Time.deltaTime);
    }

   //public void Look()
    //{
       // if (Mouse.current == null)
       //     return;

       // Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        //float mouseX = mouseDelta.x * CameraSpeed;
       // float mouseY = mouseDelta.y * CameraSpeed;

        // 左右はPlayer全体を回す
       // transform.Rotate(Vector3.up * mouseX);

        // 上下はカメラだけを回す
       // cameraPitch -= mouseY;
       // cameraPitch = Mathf.Clamp(
        //    cameraPitch,
        //    -lookLimit,
          //  lookLimit
       // );

       // playerCamera.localRotation =
        //    Quaternion.Euler(cameraPitch, 0f, 0f);
   // }

  // public void HandleCursor()
    //{
      //  if (Keyboard.current != null &&
      //      Keyboard.current.escapeKey.wasPressedThisFrame)
      //  {
      //          UnlockCursor();
      //  }

      //  if (!cursorLocked &&
       //     Mouse.current != null &&
       //     Mouse.current.leftButton.wasPressedThisFrame)
       // {
       //     LockCursor();
       // }
   // }

 //  public void LockCursor()
 //   {
   //     cursorLocked = true;
   //     Cursor.lockState = CursorLockMode.Locked;
    //    Cursor.visible = false;
  //  }

 // public void UnlockCursor()
  //  {
   //     cursorLocked = false;
   //     Cursor.lockState = CursorLockMode.None;
    //    Cursor.visible = true;
   // }

    public void Use()//使う
    {
        heldItem?.Use(this);
    }

    // 持っていれば落とす、持っていなければ拾う
    public void DropOrPickUp()
    {
        // HoldPositionに物があるなら落とす
        if (holdPosition.childCount > 0)
        {
            Drop();
            Debug.Log("アイテムを落とします");
            return;
        }

        // 持っていなければ、Rayに当たった物を拾う
        PickUp();
        Debug.Log("アイテムを拾おうとしています");
    }


    void PickUp()
        {
            if (playerCamera == null)
            {
                Debug.LogError(
                    "Player Cameraが設定されていません。",
                    this
                );
                return;
            }

            if (holdPosition == null)
            {
                Debug.LogError(
                    "Hold Positionが設定されていません。",
                    this
                );
                return;
            }

            if (Physics.Raycast(
                playerCamera.position,
                playerCamera.forward,
                out RaycastHit hit,
                rayDistance))
            {
                Rigidbody itemRigidbody =
                    hit.collider.GetComponentInParent<Rigidbody>();

                if (itemRigidbody == null)
                {
                    Debug.Log(
                        hit.collider.name +
                        "にRigidbodyがありません"
                    );
                    return;
                }

                // Rigidbodyが付いている本体のタグを確認
                if (!itemRigidbody.CompareTag("Item"))
                {
                    Debug.Log(
                        itemRigidbody.name +
                        "にItemタグがありません"
                    );
                    return;
                }

                itemRigidbody.useGravity = false;
                itemRigidbody.isKinematic = true;
            itemRigidbody.transform.SetParent(holdPosition);
            itemRigidbody.transform.localPosition = Vector3.zero;
            itemRigidbody.transform.localRotation = Quaternion.identity;


        }
        
    }

    void Drop()
    {
        if (holdPosition.childCount == 0)
            return;

        Transform item = holdPosition.GetChild(0);
        Rigidbody itemRigidbody =
            item.GetComponent<Rigidbody>();

        // HoldPositionから外す
        item.SetParent(null);

        // カメラの少し前に置く
        item.position =
            playerCamera.position +
            playerCamera.forward * 1.2f;

        // 重力と物理移動を戻す
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
            itemRigidbody.useGravity = true;
        }
    }
    public void Interact()
    {
        Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, rayDistance);
        if (GetComponent<IInteractable>() != null)
        {

        }
    }
    void OnDrawGizmosSelected()
    {
        Transform cameraTransform = playerCamera;

        // playerCameraが未設定なら子から探す
        if (cameraTransform == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();

            if (childCamera == null)
                return;

            cameraTransform = childCamera.transform;
        }

        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            cameraTransform.position,
            cameraTransform.forward * rayDistance
        );
    }
}