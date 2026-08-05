using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class Spline : MonoBehaviour
{
    [SerializeField]
    SplineContainer container;
    [SerializeField, Range(0, 1)]
    float t = 0;


    void Update()
    {
        //Splineや追従オブジェクトの失効などを検知してエラー防止
        if (container == null) return;
        if (container.CalculateLength() == 0f) return;

        //t値のクランプ
        t = math.saturate(t);

        // Splineの計算をする核心部分
        container[0].Evaluate(t, out float3 pos, out float3 tangent, out float3 up);

        // 位置の反映
        transform.transform.position = (Vector3)pos;

        // 回転の反映
        if (math.any(tangent))
        {
            transform.rotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)up);
        }
    }
}
