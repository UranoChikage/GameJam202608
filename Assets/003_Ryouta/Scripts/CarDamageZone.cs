using UnityEngine;

public class CarDamageZone : MonoBehaviour
{
    [SerializeField] int damage = 1;

    [Header("走行中だけダメージ")]
    [SerializeField] bool onlyWhileMoving = true;
    [SerializeField] float minimumSpeed = 1f;

    Rigidbody carRigidbody;

    private void Awake()
    {
        carRigidbody =
            GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerScript player =
            other.GetComponentInParent<PlayerScript>();

        // Player以外には何もしない
        if (player == null)
            return;

        // 車が一定速度以下ならダメージなし
        if (onlyWhileMoving &&
            carRigidbody != null &&
            carRigidbody.linearVelocity.magnitude <
            minimumSpeed)
        {
            return;
        }

        player.TakeDamage(damage);

        Debug.Log(
            gameObject.name +
            "がPlayerに当たり、" +
            damage + "ダメージ与えました"
        );
    }
}