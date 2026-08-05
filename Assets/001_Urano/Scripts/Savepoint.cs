using UnityEngine;

public class Savepoint : MonoBehaviour
{
    [SerializeField]
    StartPoint point;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<DirController>(out _)) 
        {
            point.transform.position = transform.position;
        }
    }
}
