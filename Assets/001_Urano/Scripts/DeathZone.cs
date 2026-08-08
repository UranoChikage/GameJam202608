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

        if (other.GetComponentInParent<ItemRespawnPoint>() is ItemRespawnPoint item)
        {
            item.ResetToStart();
        }
    }
}
