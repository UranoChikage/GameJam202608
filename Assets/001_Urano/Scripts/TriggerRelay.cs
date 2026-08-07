using System;
using UnityEngine;

/// <summary>子オブジェクトなど別GameObjectのColliderで発生したトリガーイベントを中継する</summary>
public class TriggerRelay : MonoBehaviour
{
    public event Action<Collider> TriggerEntered;
    public event Action<Collider> TriggerExited;

    void OnTriggerEnter(Collider other) => TriggerEntered?.Invoke(other);
    void OnTriggerExit(Collider other) => TriggerExited?.Invoke(other);
}
