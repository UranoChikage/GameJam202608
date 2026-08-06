using UnityEngine;

/// <summary>倒れている棒（親）。Playerが近づくと起き上がって看板が持ち上がる</summary>
public class RisingSignboard : MonoBehaviour
{
    [SerializeField] Vector3 risenLocalEulerAngles = Vector3.zero;
    [SerializeField] float duration = 1f;
    [SerializeField] AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Quaternion startRotation;
    Quaternion targetRotation;
    bool triggered;
    float elapsed;

    void Awake()
    {
        startRotation = transform.localRotation;
        targetRotation = Quaternion.Euler(risenLocalEulerAngles);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.transform.TryGetComponent<PlayerScript>(out _)) return;

        triggered = true;
    }

    void Update()
    {
        if (!triggered) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, curve.Evaluate(t));
    }
}
