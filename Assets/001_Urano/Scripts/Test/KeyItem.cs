using UnityEngine;

public class KeyItem : MonoBehaviour,IItem
{
    public void OnPickup(Player player) { }
    public void Use(Player player)
    {
        // 鍵を使う処理だけ書く。Dropを呼ぶかどうかは考えなくていい
    }
}