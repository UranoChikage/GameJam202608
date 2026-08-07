using UnityEngine;

public class TestKey : MonoBehaviour,IItem,IKey 
{
    [SerializeField]
    private int keyID;

    public int KeyID => keyID;

    public void PickUp(PlayerScript player)
    {
        Debug.Log("鍵を拾った");
    }

    
    
        public void Use(PlayerScript playre, bool interactFailed)
    {
        if (interactFailed) return;

        if (playre)
        {
            PickUp(playre);
            gameObject.SetActive(false);
            Debug.Log("使った");
        }
    }
}


