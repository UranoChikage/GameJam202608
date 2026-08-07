using UnityEditor;
using UnityEngine;

public class key_ : MonoBehaviour,IItem
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
            Debug.Log("鍵がそろった");
        }
    }

    public void Use(PlayerScript playre,bool interactFailed)
    {
        if (interactFailed) return;

        if (playre)
        {
            PickUp(playre);
            gameObject .SetActive(false);
            Debug.Log("使った");
        }
    }

    public void PickUp(PlayerScript playre) 
    {
        Debug.Log("鍵を拾った");
    }
}
