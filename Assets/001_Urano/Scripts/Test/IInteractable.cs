using UnityEngine;

//インタラクト可能か判別することができる
public interface IInteractable
{
    void Interact(PlayerScript player);// 自分のふるまいを書くだけで、呼び出し側は何も考えなくていい
}
