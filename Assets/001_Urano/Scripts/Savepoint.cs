using UnityEngine;

public class Savepoint : MonoBehaviour
{
    [SerializeField]
    StartPoint point;

    private void Start()
    {
        if (point != null)
        {
            point = FindObjectOfType<StartPoint>();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<PlayerScript>(out _)) 
        {
            point.transform.position = transform.position;
            MeshRenderer renderer = GetComponent<MeshRenderer>();
                if (renderer != null)
            {
                renderer.material.color = Color.green;
            }

        }
    }
}
