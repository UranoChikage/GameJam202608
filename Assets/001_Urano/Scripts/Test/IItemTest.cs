public interface IItem
{
    void OnPickup(Player player);
    void Use(Player player);   // Dropは無し
}