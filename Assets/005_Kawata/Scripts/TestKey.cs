using UnityEngine;

public class TestKey : MonoBehaviour,IItem,IKey 
{
    private int doorKeyID;

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
            
        }
    }
    public void Interact(PlayerScript player)
    {
        if (player.HeldItem is IKey key)
        {
            if (key.KeyID == doorKeyID)
            {

                keydrop();
            }
            else
            {
                Debug.Log("鍵が違います");
            }
        }
    }

    private void keydrop()
    {
        Debug.Log("鍵を使った！");
        gameObject.SetActive(false);
    }
}


