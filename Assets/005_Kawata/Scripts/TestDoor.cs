using UnityEngine;
using UnityEngine.InputSystem;

public class TestDoor : MonoBehaviour, IInteractable
{
    [SerializeField]
    private int doorKeyID;

    public void Interact(PlayerScript player)
    {
        if (player.HeldItem is IKey key)
        {
            if (key.KeyID == doorKeyID)
            {
               
                OpenDoor();
            }
            else
            {
                Debug.Log("鍵が違います");
            }
        }
    }
    private void OpenDoor()
    {
        Debug.Log("ドアが開いた！");
        gameObject.SetActive(false);
    }
}