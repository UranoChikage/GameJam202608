using UnityEngine;

/// <summary>
/// 足場のコライダーを摩擦ゼロにするユーティリティ。
/// キネマティックRigidbodyをMovePositionで動かすとPhysXが接触摩擦で
/// 乗っているRigidbodyを押し出すが、これはIMovingPlatform.DeltaMovementの
/// 手動加算と二重に効いてしまう（プレイヤーが実際の倍近く動く原因）。
/// 摩擦を切ることで移動はDeltaMovementの加算のみに一本化される。
/// </summary>
public static class MovingPlatformFriction
{
    private static PhysicsMaterial frictionlessMaterial;

    public static void ApplyFrictionless(Collider[] colliders)
    {
        if (frictionlessMaterial == null)
        {
            frictionlessMaterial = new PhysicsMaterial("MovingPlatform_Frictionless")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0f,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
        }

        foreach (Collider platformCollider in colliders)
        {
            platformCollider.material = frictionlessMaterial;
        }
    }
}
