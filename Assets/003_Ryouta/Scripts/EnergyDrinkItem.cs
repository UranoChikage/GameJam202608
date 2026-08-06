using UnityEngine;

public class EnergyDrinkItem :
    MonoBehaviour,
    IItem
{
    public void PickUp(PlayerScript player)
    {
        Debug.Log("エナジードリンクを拾いました");
    }

    public void Use(
        PlayerScript player,
        bool isHeld)
    {
        if (!isHeld)
            return;

        Debug.Log("エナジードリンクを使用しました");

        player.EnergyDrink(3f, 10f);
        Destroy(gameObject);
    }
}