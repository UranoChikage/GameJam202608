using UnityEngine;
using UnityEngine.InputSystem;
using System;
[RequireComponent(typeof(CharacterController))]
public class PlayerScript : MonoBehaviour
{
    //視点
    [SerializeField] Transform playerCamera;//動かすカメラ
   
    CharacterController controller;
    float verticalVelocity;


    [SerializeField] Transform[] holdPositions;

    // 使用する番号。0が最初
    [SerializeField] int holdPositionIndex = 0;
    [SerializeField] float pickUpDistance = 3f;

    [SerializeField] float rayDistance = 3f;
    [SerializeField] float dropDistance = 1f;
    [SerializeField] float dropHeight = 0.5f;

    [SerializeField]
    FPSCameraController movementController;

    Rigidbody heldRigidbody;
    IItem heldItem;

    [SerializeField]
    Vector3 holdOffset =
    new Vector3(0.4f, -0.3f, 0.8f);

    Collider[] heldColliders;
    public event Action<bool> OnDead;

    [SerializeField] bool showInteractDebug = true;

    IInteractable currentInteractable;
    RaycastHit currentInteractHit;
    public Vector3 Forward => playerCamera.forward;

    /// <summary>死亡判定を発火し、直近のSavePoint（StartPoint）へリスポーンする</summary>
    public void Die()
    {
        OnDead?.Invoke(true);

        StartPoint startPoint = FindFirstObjectByType<StartPoint>();
        if (startPoint != null)
        {
            startPoint.Respawn();
        }
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();

      
    }

  
    public void Update()
    {
        Move();

        CheckInteractable();

        if (Keyboard.current != null &&
    Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }

        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            DropOrPickUp();
            Debug.Log("Fキーを押しました");
        }
    }

    public void Move()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

        }
    }

    private Transform GetHoldPosition()
    {
        if (holdPositions == null ||
            holdPositions.Length == 0)
        {
            Debug.LogError(
                "HoldPositionが登録されていません",
                this
            );

            return null;
        }

        if (holdPositionIndex < 0 ||
            holdPositionIndex >= holdPositions.Length)
        {
            Debug.LogError(
                "Hold Position Indexが範囲外です",
                this
            );

            return null;
        }

        return holdPositions[holdPositionIndex];
    }

    public void Use(bool interactFailed)//使う
    {
        // 持っているアイテムを使用
        heldItem?.Use(this, interactFailed);
    }

    // 持っていれば落とす、持っていなければ拾う
    public void DropOrPickUp()
    {
        // 持っているなら落とす
        if (heldRigidbody != null)
        {
            Drop();
            return;
        }

        // 持っていなければ拾う
        PickUp();
    }


    void PickUp()
    {
        Transform holdPosition = GetHoldPosition();

        if (playerCamera == null ||
            holdPosition == null)
        {
            Debug.LogError(
                "PlayerCameraかHoldPositionが未設定です",
                this
            );

            return;
        }

        bool isHit = Physics.Raycast(
            playerCamera.position,
            Forward,
            out RaycastHit hit,
            rayDistance
        );

        if (!isHit)
        {
            Debug.Log("Rayが何にも当たっていません");
            return;
        }

        Debug.Log(
            "Rayが当たった物：" +
            hit.collider.name
        );

        Rigidbody itemRigidbody =
            hit.collider.attachedRigidbody;

        if (itemRigidbody == null)
        {
            itemRigidbody =
                hit.collider.GetComponentInParent<Rigidbody>();
        }

        if (itemRigidbody == null)
        {
            Debug.Log("Rigidbodyがありません");
            return;
        }

        // タグ判定はIItem判定に統一（アイテム固有の反応をIItem.PickUpに任せられるようにするため）
        // ColliderまたはRigidbody本体のどちらかがItemなら拾う
        // bool isItem =
        //     hit.collider.CompareTag("Item") ||
        //     itemRigidbody.CompareTag("Item");
        //
        // if (!isItem)
        // {
        //     Debug.Log("Itemタグがありません");
        //     return;
        // }

        IItem item = itemRigidbody.GetComponent<IItem>();

        if (item == null)
        {
            Debug.Log("IItemを実装していません");
            return;
        }

        heldRigidbody = itemRigidbody;
        heldItem = item;

        heldRigidbody.useGravity = false;
        heldRigidbody.isKinematic = true;

        heldColliders =
            heldRigidbody.GetComponentsInChildren<Collider>();

        foreach (Collider itemCollider in heldColliders)
        {
            itemCollider.enabled = false;
        }

        heldItem.PickUp(this);
    }

    void Drop()
    {
        if (heldRigidbody == null)
            return;

        // カメラの向きから上下方向を除く
        Vector3 flatForward = Vector3.ProjectOnPlane(
            Forward,
            Vector3.up
        ).normalized;

        // 真上・真下を向いたときの対策
        if (flatForward.sqrMagnitude < 0.01f)
        {
            flatForward = transform.forward;
        }

        // プレイヤーの前方かつ少し上へ配置
        Vector3 dropPosition =
            transform.position +
            flatForward * dropDistance +
            Vector3.up * dropHeight;

        heldRigidbody.position = dropPosition;

        // 勝手に飛ばないよう速度をリセット
        heldRigidbody.linearVelocity = Vector3.zero;
        heldRigidbody.angularVelocity = Vector3.zero;

        heldRigidbody.isKinematic = false;
        heldRigidbody.useGravity = true;

        // 高速移動による床抜け対策
        heldRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        // Colliderを最後に戻す
        if (heldColliders != null)
        {
            foreach (Collider itemCollider in heldColliders)
            {
                itemCollider.enabled = true;
            }
        }

        heldRigidbody = null;
        heldItem = null;
        heldColliders = null;
        Debug.Log("アイテムを落としました");
    }
    public void Interact()
    {
        if (playerCamera == null)
        {
            Debug.LogError(
                "Player Cameraが設定されていません"
            );

            return;
        }

        Debug.Log("インタラクト処理を開始しました");

        if (Physics.Raycast(
            playerCamera.position,
            Forward,
            out RaycastHit hit,
            rayDistance))
        {
            Debug.Log(
                "Rayが当たった物：" +
                hit.collider.name
            );

            IInteractable interactable =
                hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                Debug.Log(
                    hit.collider.name +
                    "をインタラクトします"
                );

                interactable.Interact();
                Use(false);
            }
            else
            {
                Debug.LogWarning(
                    hit.collider.name +
                    "にはIInteractableがありません"
                );

                // インタラクト失敗時、通すかどうかはItem側が判断する
                Use(true);
            }

            // 当たった場合は緑色
            Debug.DrawRay(
                playerCamera.position,
                Forward * hit.distance,
                Color.green,
                1f
            );
        }
        else
        {
            Debug.LogWarning(
                "Rayが何にも当たりませんでした"
            );

            // 当たらなかった場合は赤色
            Debug.DrawRay(
                playerCamera.position,
                Forward * rayDistance,
                Color.red,
                1f
            );

            // インタラクト失敗時、通すかどうかはItem側が判断する
            Use(true);
        }
    }


    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawRay(
                playerCamera.position,
                Forward * rayDistance
            );
        }

        if (holdPositions == null)
            return;

        for (int i = 0; i < holdPositions.Length; i++)
        {
            if (holdPositions[i] == null)
                continue;

            Gizmos.color =
                i == holdPositionIndex
                    ? Color.green
                    : Color.cyan;

            Gizmos.DrawSphere(
                holdPositions[i].position,
                0.05f
            );
        }
    }

    private void LateUpdate()
    {
        if (heldRigidbody == null)
            return;

        Transform holdPosition = GetHoldPosition();

        if (holdPosition == null)
            return;

        heldRigidbody.transform.SetPositionAndRotation(
            holdPosition.position,
            holdPosition.rotation
        );
    }
    
    private void CheckInteractable()
    {
        currentInteractable = null;

        if (playerCamera == null)
            return;

        bool isHit = Physics.Raycast(
            playerCamera.position,
            Forward,
            out currentInteractHit,
            rayDistance
        );

        if (!isHit)
        {
            // 何にも当たっていない
            Debug.DrawRay(
                playerCamera.position,
                Forward * rayDistance,
                Color.yellow
            );

            return;
        }

        currentInteractable =
            currentInteractHit.collider
                .GetComponentInParent<IInteractable>();

        // インタラクト可能なら緑、不可能なら赤
        Color rayColor =
            currentInteractable != null
                ? Color.green
                : Color.red;

        Debug.DrawRay(
            playerCamera.position,
            Forward * rayDistance,
            rayColor
        );
    }
    private void OnGUI()
    {
        if (!showInteractDebug)
            return;

        if (currentInteractable == null)
            return;

        GUI.Label(
            new Rect(
                Screen.width / 2f - 75f,
                Screen.height / 2f + 30f,
                150f,
                30f
            ),
            "[E] インタラクト"
        );
    }

    public void EnergyDrink(
    float value,
    float time)
    {
        if (movementController == null)
        {
            Debug.LogError(
                "Movement Controllerが未設定です"
            );

            return;
        }

        movementController.ApplySpeedBoost(
            value,
            time
        );
    }
}