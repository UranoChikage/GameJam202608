using System.Data;
using UnityEngine;

public interface  IItem 
{

    /// <summary>interactFailed: インタラクト失敗時にtrue。実際に使うかどうかはItem側で判断する</summary>
    public void Use(PlayerScript player, bool interactFailed);
    public void PickUp(PlayerScript player);
    public interface IItem
    {
        void Use(PlayerScript player);
    }

}
