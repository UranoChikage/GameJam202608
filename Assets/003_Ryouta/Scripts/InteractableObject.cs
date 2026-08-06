using UnityEngine;

public class InteractableObject :
    MonoBehaviour,
    IInteractable
{
    public void Interact(PlayerScript player)
    {
        Debug.Log(
            gameObject.name +
            "をインタラクトしました"
        );
    }
}