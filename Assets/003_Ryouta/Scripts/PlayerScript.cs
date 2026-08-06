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


    [SerializeField] Transform holdPosition;
    [SerializeField] float pickUpDistance = 3f;

    [SerializeField] float rayDistance = 3f;

    Rigidbody heldRigidbody;
    IItem heldItem;

    [SerializeField]
    Vector3 holdOffset =
    new Vector3(0.4f, -0.3f, 0.8f);

    Collider[] heldColliders;
    public event Action<bool> OnDead;

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
    

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

        }

    }

 

    public void Use()//使う
    {
        heldItem?.Use(this);
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
        if (playerCamera == null || holdPosition == null)
        {
            Debug.LogError(
                "PlayerCameraかHoldPositionが未設定です"
            );
            return;
        }

        bool isHit = Physics.Raycast(
            playerCamera.position,
            playerCamera.forward,
            out RaycastHit hit,
            rayDistance
        );

        if (!isHit)
        {
            Debug.Log("Rayが何にも当たっていません");
            return;
        }

        Debug.Log("Rayが当たった：" + hit.collider.name);

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

        // 持っている間はColliderを無効にする
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

        // プレイヤーの少し前へ移動
        heldRigidbody.transform.position =
            playerCamera.position +
            playerCamera.forward * 2f;

        heldRigidbody.isKinematic = false;
        heldRigidbody.useGravity = true;

        // Colliderを戻す
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
        Physics.Raycast(playerCamera.position, playerCamera.forward, out RaycastHit hit, rayDistance);
        if (GetComponent<IInteractable>() != null)
        {

        }
    }
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(
                playerCamera.position,
                playerCamera.forward * rayDistance
            );
        }

        if (holdPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(
                holdPosition.position,
                0.05f
            );
        }
    }

    void LateUpdate()
    {
        if (heldRigidbody == null ||
            playerCamera == null)
        {
            return;
        }

        // カメラから見て右・下・前の位置
        Vector3 targetPosition =
            playerCamera.TransformPoint(holdOffset);

        heldRigidbody.transform.position =
            targetPosition;

        heldRigidbody.transform.rotation =
            playerCamera.rotation;
    }
}