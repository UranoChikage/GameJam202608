using UnityEngine;

/// <summary>紐でぶら下がっている看板。Playerが近づくと落ちてくる</summary>
public class HangingSignboard : MonoBehaviour
{
    [SerializeField] float dropDistance = 3f;
    [SerializeField] float duration = 1f;
    [SerializeField] AnimationCurve curve;

    Vector3 startPosition;
    Vector3 targetPosition;
    bool triggered;
    float elapsed;

    void Awake()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.down * dropDistance;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.transform.TryGetComponent<PlayerScript>(out _)) return;

        triggered = true;
    }

    void Update()
    {
        if (!triggered || elapsed >= duration) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, curve.Evaluate(t));
    }
}
