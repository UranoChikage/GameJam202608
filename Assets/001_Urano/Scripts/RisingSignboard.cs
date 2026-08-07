using UnityEngine;

/// <summary>倒れている棒（親）。Playerが近づくと起き上がって看板が持ち上がる</summary>
public class RisingSignboard : MonoBehaviour
{
    [SerializeField] Vector3 risenLocalEulerAngles = Vector3.zero;
    [SerializeField] float duration = 1f;
    [SerializeField] AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] TriggerRelay triggerRelay;

    Quaternion startRotation;
    Quaternion targetRotation;
    float t;
    bool rising;
    bool active;

    void Awake()
    {
        startRotation = transform.localRotation;
        targetRotation = Quaternion.Euler(risenLocalEulerAngles);

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

        rising = true;
        active = true;
    }

    void HandleTriggerExit(Collider other)
    {
        if (!other.transform.TryGetComponent<PlayerScript>(out _)) return;

        rising = false;
        active = true;
    }

    void Update()
    {
        if (!active) return;

        float dt = Time.deltaTime / duration;
        t = rising ? Mathf.Clamp01(t + dt) : Mathf.Clamp01(t - dt);

        transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, curve.Evaluate(t));

        if (rising ? t >= 1f : t <= 0f) active = false;
    }
}
