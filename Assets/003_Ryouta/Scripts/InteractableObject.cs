using UnityEngine;

public class InteractableObject :
    MonoBehaviour,
    IInteractable
{
    public void Interact()
    {
        Debug.Log(
            gameObject.name +
            "をインタラクトしました"
        );
    }
}