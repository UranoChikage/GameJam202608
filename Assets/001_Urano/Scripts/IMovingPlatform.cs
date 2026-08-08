using UnityEngine;

/// <summary>プレイヤーを乗せて運ぶ足場が実装するインターフェース。</summary>
public interface IMovingPlatform
{
    /// <summary>直前のFixedUpdateからの移動量。</summary>
    Vector3 DeltaMovement { get; }
}
