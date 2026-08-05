using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<DirController>(out _))
        {
            Debug.Log("Goal reached!");
        }
    }

}
