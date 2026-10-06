using UnityEngine;

namespace KaiChe.Gameplay
{
    internal static class RuntimeWorldSpace
    {
        public const float GroundY = 0f;
        public const float PlayerRootHeight = 0.43f;
        public const float EnemyRootHeight = 0.56f;
        public const float ProjectileHeight = 0.86f;
        public const float EffectHeight = 0.72f;
        public const float LabelHeight = 2.15f;
        public const float CameraTargetHeight = 0.72f;

        public static Vector3 ToWorld(Vector2 logical, float height = GroundY)
        {
            return new Vector3(logical.x, height, logical.y);
        }

        public static Vector2 ToLogical(Vector3 world)
        {
            return new Vector2(world.x, world.z);
        }
    }
}
