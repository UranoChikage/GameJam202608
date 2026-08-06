using UnityEngine;

public class ball : MonoBehaviour, IItem
{ 
    Rigidbody rb;
    private void Start()
    {
        rb= GetComponent<Rigidbody>();
    }

    public void Use(PlayerScript player, bool interactFailed)
    {
        if (player)
        {
            player.DropOrPickUp();
            rb.AddForce(player.Forward * 5, ForceMode.Impulse);
            Debug.Log("使った");
        }

    }
    
    public void PickUp(PlayerScript player)
    {
        Debug.Log("ボールを拾った");
    }
}
