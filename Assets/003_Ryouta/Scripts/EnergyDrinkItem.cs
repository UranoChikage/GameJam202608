using UnityEngine;

public class EnergyDrinkItem :
    MonoBehaviour,
    IItem
{
    [SerializeField, Min(0.01f)] private float speedMultiplier = 3f;
    [SerializeField, Min(0.01f)] private float effectDuration = 10f;

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

        player.EnergyDrink(speedMultiplier, effectDuration);

        DamageVignetteEffect.TryPlayBoostEffect(effectDuration);

        Destroy(gameObject);
    }
}
