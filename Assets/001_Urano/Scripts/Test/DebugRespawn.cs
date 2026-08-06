using UnityEngine;

public class DebugRespawn : MonoBehaviour
{
    [SerializeField] private KeyCode debugKillKey = KeyCode.Q;

    private void Update()
    {
        if (Input.GetKeyDown(debugKillKey))
        {
            PlayerScript player = FindFirstObjectByType<PlayerScript>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}
