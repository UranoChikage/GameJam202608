using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour
{
    [SerializeField]
    UnityEvent onHit;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<ICannonball>(out ICannonball cannonball))
        {
            onHit.Invoke();
            cannonball.rb.linearVelocity = Vector3.zero;
            Destroy(gameObject);
        }
    }
}
