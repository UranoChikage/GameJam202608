using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public void Interact(PlayerScript player)
    {
        Debug.Log("ドアを操作しました");
    }
}