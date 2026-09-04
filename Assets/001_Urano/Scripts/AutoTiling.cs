using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class AutoTiling : MonoBehaviour
{
    [Header("1ユニットあたりの繰り返し回数")]
    [SerializeField] private float tilingPerUnit = 0.2f;

    [Header("どの軸をUVのX/Yに使うか")]
    [SerializeField] private Axis uAxis = Axis.X;
    [SerializeField] private Axis vAxis = Axis.Y;

    [Header("対象テクスチャプロパティ名")]
    [SerializeField] private string texturePropertyName = "_BaseMap"; // URP Lit: _BaseMap, Built-in Standard: _MainTex

    private enum Axis { X, Y, Z }

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private Vector3 _lastScale;

    private void OnEnable()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        Apply();
    }

    private void Update()
    {
        // スケールが変わった時だけ更新(エディタでの調整にも追従させたい場合)
        if (transform.lossyScale != _lastScale)
        {
            Apply();
        }
    }

    private void Apply()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();

        Vector3 scale = transform.lossyScale;
        float uScale = GetAxisValue(scale, uAxis) * tilingPerUnit;
        float vScale = GetAxisValue(scale, vAxis) * tilingPerUnit;

        _renderer.GetPropertyBlock(_mpb);
        string stPropertyName = texturePropertyName + "_ST";
        _mpb.SetVector(stPropertyName, new Vector4(uScale, vScale, 0f, 0f));
        _renderer.SetPropertyBlock(_mpb);

        _lastScale = scale;
    }

    private float GetAxisValue(Vector3 v, Axis axis)
    {
        switch (axis)
        {
            case Axis.X: return v.x;
            case Axis.Y: return v.y;
            case Axis.Z: return v.z;
            default: return 1f;
        }
    }
}