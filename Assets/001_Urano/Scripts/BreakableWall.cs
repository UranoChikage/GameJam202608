using System.Collections;
using UnityEngine;

// 大砲の弾(ICannonball)が当たると壊れる壁
// 壁メッシュを隠し、破片メッシュをプールから複数個取り出して飛び散らせるヨ
public class BreakableWall : MonoBehaviour
{
    [Header("見た目")]
    [SerializeField] private GameObject wallMesh;     
    [SerializeField] private Collider wallCollider;   
    [Header("破片")]
    [SerializeField] private ObjectPool fragmentPool; 
    [SerializeField] private int fragmentCount = 6;
    [SerializeField] private float scatterForce = 5f;
    [SerializeField] private float scatterRadius = 1.5f;
    [SerializeField] private float fragmentLifetime = 3f;

    private bool isBroken = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken)
            return;
        if (collision.gameObject.GetComponentInParent<ICannonball>() == null)
            return;

        Break(collision.contacts[0].point);
    }

    private void Break(Vector3 hitPoint)
    {
        isBroken = true;

        if (wallMesh != null)
            wallMesh.SetActive(false);

        if (wallCollider != null)
            wallCollider.enabled = false;

        for (int i = 0; i < fragmentCount; i++)
        {
            Vector3 spawnPos = hitPoint + Random.insideUnitSphere * scatterRadius;
            GameObject fragment = fragmentPool.Get(spawnPos, Random.rotation);

            if (fragment.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                Vector3 dir = (fragment.transform.position - hitPoint).normalized;
                if (dir == Vector3.zero)
                    dir = Random.onUnitSphere;

                rb.AddForce(dir * scatterForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * scatterForce, ForceMode.Impulse);
            }

            StartCoroutine(ReleaseAfter(fragment, fragmentLifetime));
        }
    }

    private IEnumerator ReleaseAfter(GameObject fragment, float delay)
    {
        yield return new WaitForSeconds(delay);
        fragmentPool.Release(fragment);
    }
}
