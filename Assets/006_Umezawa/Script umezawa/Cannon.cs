using UnityEngine;

public class Cannon : MonoBehaviour, IInteractable
{
    public float pow;
    public Transform firePoint; // 発射位置を指定するTransform
    // Rigidbodyコンポーネントを保持するプロパティ
    public void Interact(PlayerScript player)
    // IInteractableインターフェースの実装
    {
        if (player.HeldItem != null)
        {
            if(player.HeldItem is ICannonball cannonball)
            {
                if (firePoint == null)
                {
                    // firePointがインスペクターで設定されていない場合の警告
                    Debug.LogWarning("firePointが設定されていません。インスペクターで設定してください。");
                    return;
                }

                // 大砲の玉を使う処理
                cannonball.SetPos(firePoint.position, player); // firePointの位置に玉を配置
                cannonball.Fire(pow, firePoint.forward);       // firePointの向きに発射
            }
            else
            {
                Debug.Log("大砲の玉ではないアイテムを持っています。");
            }
        }
        Debug.Log("大砲発射");
        
        //そのRigidBodyに👇のように力を与え 
        
    }
}
