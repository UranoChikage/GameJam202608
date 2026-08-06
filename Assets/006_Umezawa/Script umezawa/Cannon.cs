using UnityEngine;

public class Cannon : MonoBehaviour, IInteractable
{
    public float pow;
    public void Interact(PlayerScript player)
    {
        if (player.HeldItem != null)
        {
            if(player.HeldItem is ICannonball cannonball)
            {
                // 大砲の玉を使う処理
                cannonball.SetPos(transform.position + transform.forward * 2f, player); // 大砲の前方に玉を配置
                cannonball.Fire(pow, transform.forward);
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
