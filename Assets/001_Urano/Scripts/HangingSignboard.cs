using UnityEngine;

/// <summary>紐でぶら下がっている看板。Playerが近づくと落ちてくる</summary>
public class HangingSignboard : MonoBehaviour
{
    [SerializeField] float dropDistance = 3f;
    [SerializeField] float duration = 1f;
    [SerializeField] AnimationCurve curve;
    [SerializeField] TriggerRelay triggerRelay;

    Vector3 startPosition;
    Vector3 targetPosition;
    float t;
    bool falling;
    bool active;

    void Awake()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.down * dropDistance;

        if (triggerRelay != null)
        {
            triggerRelay.TriggerEntered += HandleTriggerEnter;
            triggerRelay.TriggerExited += HandleTriggerExit;
        }
    }

    void OnDestroy()
    {
        if (triggerRelay != null)
        {
            triggerRelay.TriggerEntered -= HandleTriggerEnter;
            triggerRelay.TriggerExited -= HandleTriggerExit;
        }
    }

    void OnTriggerEnter(Collider other) => HandleTriggerEnter(other);

    void OnTriggerExit(Collider other) => HandleTriggerExit(other);

    void HandleTriggerEnter(Collider other)
    {
        if (!other.transform.TryGetComponent<PlayerScript>(out _)) return;

        falling = true;
        active = true;
    }

    void HandleTriggerExit(Collider other)
    {
        if (!other.transform.TryGetComponent<PlayerScript>(out _)) return;

        falling = false;
        active = true;
    }

    void Update()
    {
        if (!active) return;

        float dt = Time.deltaTime / duration;
        t = falling ? Mathf.Clamp01(t + dt) : Mathf.Clamp01(t - dt);

        transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, curve.Evaluate(t));

        if (falling ? t >= 1f : t <= 0f) active = false;
    }
}
