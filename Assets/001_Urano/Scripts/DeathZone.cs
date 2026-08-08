using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<PlayerScript>(out var player))
        {
            player.Die();
            return;
        }

        // 自分自身・親方向・子方向のいずれにあってもOK
        var item = other.GetComponentInParent<ItemRespawnPoint>()
                   ?? other.GetComponentInChildren<ItemRespawnPoint>();

        if (item != null)
        {
            item.ResetToStart();
        }
    }
}