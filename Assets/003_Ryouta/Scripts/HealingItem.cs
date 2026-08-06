using UnityEngine;

public class HealingItem :
    MonoBehaviour,
    IItem
{
    [SerializeField] int healAmount = 1;

    public void PickUp(PlayerScript player)
    {
        Debug.Log("回復アイテムを拾いました");
    }

    public void Use(
        PlayerScript player,
        bool isHeld)
    {
        if (!isHeld)
            return;

        // 回復に成功した場合だけ消費
        bool healed =
            player.Heal(healAmount);

        if (healed)
        {
            player.ConsumeHeldItem();
        }
    }

}