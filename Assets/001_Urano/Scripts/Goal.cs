using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<PlayerScript>(out _))
        {
            GameManager.Instance.LoadScene(nextSceneName);
        }
    }

}
