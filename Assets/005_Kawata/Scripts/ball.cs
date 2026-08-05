using UnityEngine;

public class ball : MonoBehaviour, IItem
{ 
    Rigidbody rb;
    private void Start()
    {
        rb= GetComponent<Rigidbody>();
    }

    public void Use(PlayerScript player)
    {
        if (player)
        {
            PickUp(player);
            rb.AddForce(Vector3.forward * 5, ForceMode.Impulse);
            Debug.Log("使った");
        }

    }
    
    public void PickUp(PlayerScript player)
    {
        //後でプレイヤーがなんかする
    }
}
