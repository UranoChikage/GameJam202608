using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
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

        // 位置の反映
        transform.transform.position = (Vector3)pos + offset;

        // 回転の反映（急なタンジェント変化でカクつかないようSlerpで滑らかに追従）
        if (math.any(tangent))
        {
            Quaternion targetRotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)up);

            transform.rotation = Application.isPlaying
                ? Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed)
                : targetRotation; // 編集中は即反映してスクラブ操作を分かりやすくする
        }
    }
}
