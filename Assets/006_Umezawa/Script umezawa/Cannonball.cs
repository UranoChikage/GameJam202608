using UnityEngine;

public class Cannonball : MonoBehaviour,IItem,ICannonball
{
    public Rigidbody rb { get; set; }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Use(PlayerScript player, bool interactFailed)
    {
    }

    public void PickUp(PlayerScript player)
    {
        Debug.Log("大砲の玉を拾った");
    }
    public void Fire(float power, Vector3 direction)
    {
        if (rb != null)
        {
            rb.AddForce(direction * power, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody is not assigned for the cannonball.");
        }
    }
    public void SetPos(Vector3 pos,PlayerScript p)
    {
        p.DropOrPickUp();
        rb.position = pos;
    }
}
