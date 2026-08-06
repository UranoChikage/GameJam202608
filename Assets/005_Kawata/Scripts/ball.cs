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
        // インタラクト失敗時（何もインタラクトできる物が無かった時）だけ投げる
        if (!interactFailed) return;

        if (player)
        {
            PickUp(player);
            rb.AddForce(Vector3.forward * 5, ForceMode.Impulse);
            Debug.Log("使った");
        }

    }
    
    public void PickUp(PlayerScript player)
    {
        Debug.Log("ボールを拾った");
    }
}
