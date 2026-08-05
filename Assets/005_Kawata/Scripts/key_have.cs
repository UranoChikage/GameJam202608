using UnityEngine;

public class key_ : MonoBehaviour
{
    [SerializeField]
    bool key1 = false;
    [SerializeField]
    bool key2 = false;
    [SerializeField]
    bool key3 = false;

    private void Start()
    {
        if (key1 && key2 && key3)
        {
            Debug.Log("扉があいた");
        }
    }

}
