using UnityEngine;

public class Door1 : MonoBehaviour, IInteractable
{
    public void Interact(PlayerScript player)
    {
                Debug.Log("ドアを操作しました");
    }
}