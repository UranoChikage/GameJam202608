using UnityEngine;

/// <summary>
/// 使い方：
///   - シーン内に1つだけ配置し、他のスクリプトから DirController.Instance で参照するか、
///     Inspector上でこのコンポーネントの参照をドラッグ＆ドロップして使う。
/// </summary>
public class DirController : MonoBehaviour
{
    public static DirController Instance { get; private set; }

    [Header("感度設定")]
    [SerializeField] private float mouseSensitivity = 150f;

    [Header("上下(Pitch)の可動範囲")]
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("カーソルをロックするか")]
    [SerializeField] private bool lockCursor = true;

    /// <summary>左右の向き（Y軸回転、度数法）</summary>
    public float Yaw { get; private set; }

    /// <summary>上下の向き（X軸回転、度数法）</summary>
    public float Pitch { get; private set; }

    private void Awake()
    {
        // シンプルなシングルトン。同時に複数存在させない想定。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        Yaw += mouseX;
        Pitch -= mouseY;
        Pitch = Mathf.Clamp(Pitch, minPitch, maxPitch);

        // Yawは0〜360度に正規化（任意）
        if (Yaw < 0f) Yaw += 360f;
        if (Yaw >= 360f) Yaw -= 360f;
    }

    /// <summary>左右方向のみの回転（プレイヤー本体の向き・移動方向計算などに使用）</summary>
    public Quaternion GetYawRotation() => Quaternion.Euler(0f, Yaw, 0f);

    /// <summary>上下方向のみの回転（カメラの上下振りに使用）</summary>
    public Quaternion GetPitchRotation() => Quaternion.Euler(Pitch, 0f, 0f);

    /// <summary>上下左右を合成した回転（視線方向そのもの）</summary>
    public Quaternion GetLookRotation() => Quaternion.Euler(Pitch, Yaw, 0f);

    /// <summary>水平面上の前方向ベクトル（移動処理で使いやすい）</summary>
    public Vector3 GetFlatForward() => GetYawRotation() * Vector3.forward;

    /// <summary>水平面上の右方向ベクトル（移動処理で使いやすい）</summary>
    public Vector3 GetFlatRight() => GetYawRotation() * Vector3.right;

    /// <summary>外部から向きを直接設定したい場合（リスポーン時の初期化など）</summary>
    public void SetDirection(float yaw, float pitch)
    {
        Yaw = yaw;
        Pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }
}