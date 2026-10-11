using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    /// <summary>
    /// Allocation-free directional pattern helpers for boss/projectile emitters.
    /// Pattern concepts are adapted from BenedictBulletHell.Patterns at
    /// f242afaa4f495d239e60c1c9a0f2dd57fdc57d90 and rewritten for Kai-Che's
    /// UnityEngine.Vector2 runtime model and caller-owned buffers.
    ///
    /// MIT License
    /// Copyright (c) 2024 JohnVonDrashek
    /// Permission is hereby granted, free of charge, to any person obtaining a copy
    /// of this software and associated documentation files (the "Software"), to deal
    /// in the Software without restriction, including without limitation the rights
    /// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    /// copies of the Software, and to permit persons to whom the Software is
    /// furnished to do so, subject to the following conditions:
    /// The above copyright notice and this permission notice shall be included in all
    /// copies or substantial portions of the Software.
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    /// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    /// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    /// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    /// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    /// SOFTWARE.
    /// </summary>
    internal static class RuntimeBulletPatternMath
    {
        private const float FullCircleDegrees = 360f;
        private const float MinimumDirectionMagnitude = 0.0001f;

        public static int FillRing(
            Vector2[] directions,
            int count,
            float startAngleDegrees = 0f)
        {
            if (directions == null || directions.Length == 0 || count <= 0)
                return 0;

            int writeCount = Mathf.Min(count, directions.Length);
            float step = FullCircleDegrees / count;

            for (int i = 0; i < writeCount; i++)
            {
                directions[i] = DirectionFromDegrees(
                    startAngleDegrees + step * i);
            }

            return writeCount;
        }

        public static int FillSpread(
            Vector2[] directions,
            int count,
            Vector2 forward,
            float totalSpreadDegrees)
        {
            if (directions == null || directions.Length == 0 || count <= 0)
                return 0;

            Vector2 normalizedForward = NormalizeOrUp(forward);
            float centerAngle = Mathf.Atan2(
                normalizedForward.y,
                normalizedForward.x) * Mathf.Rad2Deg;
            int writeCount = Mathf.Min(count, directions.Length);

            if (count == 1)
            {
                directions[0] = normalizedForward;
                return 1;
            }

            float startAngle = centerAngle - totalSpreadDegrees * 0.5f;
            float step = totalSpreadDegrees / (count - 1);

            for (int i = 0; i < writeCount; i++)
            {
                directions[i] = DirectionFromDegrees(
                    startAngle + step * i);
            }

            return writeCount;
        }

        public static int FillSpiral(
            Vector2[] directions,
            int count,
            float revolutions,
            float startAngleDegrees = 0f)
        {
            if (directions == null || directions.Length == 0 || count <= 0)
                return 0;

            int writeCount = Mathf.Min(count, directions.Length);
            if (count == 1)
            {
                directions[0] = DirectionFromDegrees(startAngleDegrees);
                return 1;
            }

            float totalDegrees = FullCircleDegrees * revolutions;
            float step = totalDegrees / (count - 1);

            for (int i = 0; i < writeCount; i++)
            {
                directions[i] = DirectionFromDegrees(
                    startAngleDegrees + step * i);
            }

            return writeCount;
        }

        public static Vector2 Aim(
            Vector2 origin,
            Vector2 target,
            Vector2 fallback)
        {
            Vector2 delta = target - origin;
            if (delta.sqrMagnitude <= MinimumDirectionMagnitude)
                return NormalizeOrUp(fallback);

            return delta.normalized;
        }

        public static Vector2 Rotate(
            Vector2 direction,
            float degrees)
        {
            Vector2 normalized = NormalizeOrUp(direction);
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);

            return new Vector2(
                normalized.x * cos - normalized.y * sin,
                normalized.x * sin + normalized.y * cos);
        }

        private static Vector2 DirectionFromDegrees(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(
                Mathf.Cos(radians),
                Mathf.Sin(radians));
        }

        private static Vector2 NormalizeOrUp(Vector2 direction)
        {
            if (direction.sqrMagnitude <= MinimumDirectionMagnitude)
                return Vector2.up;

            return direction.normalized;
        }
    }
}
