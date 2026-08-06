using UnityEngine;

public class Taihou : MonoBehaviour, IInteractable
{
    public float pow;
    public GameObject prefab;
    Vector3 Scale = new Vector3(0.1f, 0.1f, 0.1f);
    public void Interact(PlayerScript player)
    {
        Debug.Log("大砲発射");
        //Gameobjectを生成する
        GameObject instantiatedPrefab = Instantiate(prefab, transform.position, transform.rotation);
        //そのゲームオブジェクトのスケールを変える
        instantiatedPrefab.transform.localScale = Scale;
        //RigidBodyを持たす
        Rigidbody rb = instantiatedPrefab.AddComponent<Rigidbody>();
        //そのRigidBodyに👇のように力を与え 
        rb.AddForce(transform.forward * pow, ForceMode.Impulse);
    }
}
