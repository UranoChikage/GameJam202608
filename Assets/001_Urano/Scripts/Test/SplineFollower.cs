using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(Rigidbody))]
public class SplineFollower : MonoBehaviour
{
    [SerializeField]
    SplineContainer container;
    [SerializeField, Range(0, 1)]
    float t = 0;
    [SerializeField]
    float duration = 5f; // 1周にかかる秒数（再生中のみ使用）

    [SerializeField]
    Vector3 offset = Vector3.zero;

    [SerializeField]
    float rotationSmoothSpeed = 5f; // 大きいほど素早く目標回転に追従する

    Rigidbody rb;
    Vector3 targetPosition;
    Quaternion targetRotation;
    bool hasTargetRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Splineに沿って直接動かすため物理演算(重力等)の影響は受けない
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        //Splineや追従オブジェクトの失効などを検知してエラー防止
        if (container == null) return;
        if (container.CalculateLength() == 0f) return;

        if (Application.isPlaying)
        {
            // 再生中は自動でtを進めてループさせる
            t += Time.deltaTime / duration;
            t %= 1f;
        }
        else
        {
            //編集中はInspectorでの手動スクラブ用にクランプのみ
            t = math.saturate(t);
        }

        // Splineの計算をする核心部分
        container[0].Evaluate(t, out float3 pos, out float3 tangent, out float3 up);

        targetPosition = (Vector3)pos + offset;

        if (math.any(tangent))
        {
            targetRotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)up);
            hasTargetRotation = true;
        }

        if (!Application.isPlaying)
        {
            // 編集中はRigidbodyを介さず即反映してスクラブ操作を分かりやすくする
            transform.position = targetPosition;
            if (hasTargetRotation) transform.rotation = targetRotation;
        }
    }

    void FixedUpdate()
    {
        if (!Application.isPlaying) return;

        // KinematicなRigidbodyをMovePositionで動かすと、PhysXが物理的な接触・摩擦として
        // 正しく扱ってくれるため、上に乗っているPlayerも自然に一緒に運ばれる
        rb.MovePosition(targetPosition);

        // 回転の反映（急なタンジェント変化でカクつかないようSlerpで滑らかに追従）
        if (hasTargetRotation)
        {
            Quaternion rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
            rb.MoveRotation(rotation);
        }
    }
}
