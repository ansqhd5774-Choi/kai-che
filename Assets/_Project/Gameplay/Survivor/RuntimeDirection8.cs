using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    public enum RuntimeDirection8
    {
        South = 0,
        SouthEast = 1,
        East = 2,
        NorthEast = 3,
        North = 4,
        NorthWest = 5,
        West = 6,
        SouthWest = 7
    }

    public static class RuntimeDirection8Utility
    {
        private const float DeadZoneSqr = 0.0001f;

        public static RuntimeDirection8 FromVector(
            Vector2 direction,
            RuntimeDirection8 fallback = RuntimeDirection8.South)
        {
            if (direction.sqrMagnitude <= DeadZoneSqr)
                return fallback;

            float angle = Mathf.Atan2(direction.y, direction.x)
                * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;

            int octant = Mathf.RoundToInt(angle / 45f) & 7;
            switch (octant)
            {
                case 0:
                    return RuntimeDirection8.East;
                case 1:
                    return RuntimeDirection8.NorthEast;
                case 2:
                    return RuntimeDirection8.North;
                case 3:
                    return RuntimeDirection8.NorthWest;
                case 4:
                    return RuntimeDirection8.West;
                case 5:
                    return RuntimeDirection8.SouthWest;
                case 6:
                    return RuntimeDirection8.South;
                default:
                    return RuntimeDirection8.SouthEast;
            }
        }

        public static string ToResourceToken(RuntimeDirection8 direction)
        {
            switch (direction)
            {
                case RuntimeDirection8.SouthEast:
                    return "southeast";
                case RuntimeDirection8.East:
                    return "east";
                case RuntimeDirection8.NorthEast:
                    return "northeast";
                case RuntimeDirection8.North:
                    return "north";
                case RuntimeDirection8.NorthWest:
                    return "northwest";
                case RuntimeDirection8.West:
                    return "west";
                case RuntimeDirection8.SouthWest:
                    return "southwest";
                default:
                    return "south";
            }
        }
    }
}
