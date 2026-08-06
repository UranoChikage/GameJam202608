using UnityEngine;

public class TestInteractable :
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