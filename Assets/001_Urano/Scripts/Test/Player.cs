using UnityEngine;

public class Player : MonoBehaviour
{
    private IItem heldItem;

    // 入力判定はPlayer側の仕事
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) heldItem?.Use(this);
        if (Input.GetKeyDown(KeyCode.Mouse1)) DropCurrentItem();
    }

    // Dropはアイテムに依存しない共通処理なのでPlayerが持つ
    public void DropCurrentItem()
    {
        if (heldItem == null) return;
        // ワールドに戻す処理（全アイテム共通）
        heldItem = null;
    }

    public void PickUp(IItem item)
    {
        heldItem = item;
        item.OnPickup(this);
    }
}