using System.Data;
using UnityEngine;

public interface  IItem 
{

    public void Use(PlayerScript player);
    public void PickUp(PlayerScript player);
}
