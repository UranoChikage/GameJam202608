using UnityEngine;

public class DebugRespawn : MonoBehaviour
{
    [SerializeField] private KeyCode debugKillKey = KeyCode.Q;

    private void Update()
    {
        if (Input.GetKeyDown(debugKillKey))
        {
            StartPoint startPoint = FindFirstObjectByType<StartPoint>();
            if (startPoint != null)
            {
                startPoint.Respawn();
            }
        }
    }
}
