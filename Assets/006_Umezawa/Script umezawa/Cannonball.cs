using UnityEngine;

public class Cannonball : MonoBehaviour,IItem,ICannonball
{
    // Rigidbodyコンポーネントを保持するプロパティ
    public Rigidbody rb { get; set; }
    // Rigidbodyコンポーネントを取得するためのStartメソッド
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    // IItemインターフェースのUseメソッドの実装
    public void Use(PlayerScript player, bool interactFailed)
    {
    }

    // 大砲の玉を拾うメソッド
    public void PickUp(PlayerScript player)
    {
        Debug.Log("大砲の玉を拾った");
    }

    // 大砲の玉を発射するメソッド
    public void Fire(float power, Vector3 direction)
    {
        if (rb != null)
        {
            rb.AddForce(direction * power, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody is not assigned for the cannonball.");
        }
    }
    // 大砲の玉の位置を設定するメソッド
    public void SetPos(Vector3 pos,PlayerScript p)
    {
        p.DropOrPickUp();
        rb.position = pos;
    }
}
