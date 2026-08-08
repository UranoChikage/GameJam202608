using UnityEngine;

public class GOAL : MonoBehaviour
{
    [Header("上下移動")]
    [SerializeField, Min(0f)] private float moveHeight = 0.5f;
    [SerializeField, Min(0.01f)] private float cycleDuration = 2f;

    private Vector3 startLocalPosition;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        // Sin波を使い、上下の端で自然に減速して滑らかに折り返す。
        float phase = Time.time * Mathf.PI * 2f / cycleDuration;
        float verticalOffset = Mathf.Sin(phase) * moveHeight;

        transform.localPosition =
            startLocalPosition + Vector3.up * verticalOffset;
    }

    private void OnValidate()
    {
        moveHeight = Mathf.Max(0f, moveHeight);
        cycleDuration = Mathf.Max(0.01f, cycleDuration);
    }
}
