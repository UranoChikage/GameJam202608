using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void Start()
    {
        PlacePlayerHere();
    }

    /// <summary>外部（ダメージ処理や落下判定など）から呼び出してプレイヤーをこの地点に戻す</summary>
    public void Respawn()
    {
        PlacePlayerHere();
    }

    private void PlacePlayerHere()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        if (player.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = transform.position;
            rb.rotation = transform.rotation;
        }
        else
        {
            player.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        // FPSCameraControllerが毎FixedUpdateでDirControllerのYawに合わせて回転させ直すため、
        // ここでリセットしないと次のFixedUpdateで向きが元に戻ってしまう
        if (DirController.Instance != null)
        {
            DirController.Instance.SetDirection(transform.eulerAngles.y, 0f);
        }
    }
}
