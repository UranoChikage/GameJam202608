using UnityEngine;
// 大砲の玉のインターフェース
public interface ICannonball
{
    // Rigidbodyコンポーネントを保持するプロパティ
    Rigidbody rb { get; set; }
    // 大砲の玉を発射するメソッド
    public void Fire(float power, Vector3 direction);
    // 大砲の玉の位置を設定するメソッド
    public void SetPos(Vector3 vector3, PlayerScript player);

}
