using UnityEngine;

public interface ICannonball
{
    Rigidbody rb { get; set; }
    public void Fire(float power, Vector3 direction);
    public void SetPos(Vector3 vector3, PlayerScript player);

}
