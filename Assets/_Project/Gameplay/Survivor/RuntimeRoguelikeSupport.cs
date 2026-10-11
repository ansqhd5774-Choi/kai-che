using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    internal static class RuntimeRoguelikeSupport
    {
        public static Vector2 ApplyJoystickDeadZone(Vector2 input, float deadZone)
        {
            float magnitude = input.magnitude;
            float clampedDeadZone = Mathf.Clamp(deadZone, 0f, 0.95f);
            if (magnitude <= clampedDeadZone)
                return Vector2.zero;

            float scaled = Mathf.InverseLerp(clampedDeadZone, 1f, Mathf.Min(1f, magnitude));
            return input.normalized * scaled;
        }

        public static Vector2 CalculateSeparation(
            Vector2 self,
            IReadOnlyList<Vector2> neighbors,
            int selfIndex,
            float radius,
            float maxStrength = 1f)
        {
            if (neighbors == null || neighbors.Count == 0 || radius <= 0f)
                return Vector2.zero;

            float radiusSqr = radius * radius;
            Vector2 force = Vector2.zero;
            int count = 0;

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (i == selfIndex)
                    continue;

                Vector2 offset = self - neighbors[i];
                float sqrDistance = offset.sqrMagnitude;
                if (sqrDistance <= 0.0001f || sqrDistance >= radiusSqr)
                    continue;

                float distance = Mathf.Sqrt(sqrDistance);
                float weight = 1f - Mathf.Clamp01(distance / radius);
                force += (offset / distance) * weight;
                count++;
            }

            if (count == 0)
                return Vector2.zero;

            force /= count;
            return Vector2.ClampMagnitude(force, Mathf.Max(0f, maxStrength));
        }

        public static RuntimeAbilityStats ApplyModifier(
            in RuntimeAbilityStats source,
            in RuntimeAbilityModifier modifier)
        {
            RuntimeAbilityStats result = source;
            result.Damage = Mathf.Max(0f, source.Damage * Mathf.Max(0f, modifier.DamageMultiplier));
            result.Cooldown = Mathf.Max(0.02f, source.Cooldown * Mathf.Max(0.02f, modifier.CooldownMultiplier));
            result.ProjectileSpeed = Mathf.Max(0f, source.ProjectileSpeed * Mathf.Max(0f, modifier.ProjectileSpeedMultiplier));
            result.Area = Mathf.Max(0f, source.Area * Mathf.Max(0f, modifier.AreaMultiplier));
            result.ProjectileCount = Mathf.Max(1, source.ProjectileCount + modifier.AdditionalProjectiles);
            return result;
        }
    }

    [Serializable]
    internal struct RuntimeAbilityStats
    {
        public float Damage;
        public float Cooldown;
        public float ProjectileSpeed;
        public float Area;
        public int ProjectileCount;
    }

    [Serializable]
    internal struct RuntimeAbilityModifier
    {
        public float DamageMultiplier;
        public float CooldownMultiplier;
        public float ProjectileSpeedMultiplier;
        public float AreaMultiplier;
        public int AdditionalProjectiles;

        public static RuntimeAbilityModifier Identity => new RuntimeAbilityModifier
        {
            DamageMultiplier = 1f,
            CooldownMultiplier = 1f,
            ProjectileSpeedMultiplier = 1f,
            AreaMultiplier = 1f,
            AdditionalProjectiles = 0
        };
    }

    [Serializable]
    internal sealed class RuntimeRunSnapshot
    {
        public int Version = 1;
        public int Level = 1;
        public long Experience;
        public int Kills;
        public int Wave = 1;
        public float SurvivalTime;
        public int FireRank;
        public int IceRank;
        public int ArcaneRank;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static RuntimeRunSnapshot FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new RuntimeRunSnapshot();

            RuntimeRunSnapshot result = JsonUtility.FromJson<RuntimeRunSnapshot>(json);
            return result ?? new RuntimeRunSnapshot();
        }
    }

    internal sealed class RuntimeGridPathfinder
    {
        private readonly int _width;
        private readonly int _height;
        private readonly bool[] _blocked;
        private readonly int[] _cameFrom;
        private readonly int[] _cost;
        private readonly List<int> _open = new List<int>(128);

        public RuntimeGridPathfinder(int width, int height)
        {
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            int count = _width * _height;
            _blocked = new bool[count];
            _cameFrom = new int[count];
            _cost = new int[count];
        }

        public void SetBlocked(int x, int y, bool blocked)
        {
            if (!IsInside(x, y))
                return;

            _blocked[ToIndex(x, y)] = blocked;
        }

        public bool TryFindPath(Vector2Int start, Vector2Int goal, List<Vector2Int> result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            result.Clear();
            if (!IsInside(start.x, start.y) || !IsInside(goal.x, goal.y))
                return false;

            int startIndex = ToIndex(start.x, start.y);
            int goalIndex = ToIndex(goal.x, goal.y);
            if (_blocked[startIndex] || _blocked[goalIndex])
                return false;

            Array.Fill(_cameFrom, -1);
            Array.Fill(_cost, int.MaxValue);
            _open.Clear();
            _open.Add(startIndex);
            _cost[startIndex] = 0;

            while (_open.Count > 0)
            {
                int bestListIndex = 0;
                int bestIndex = _open[0];
                int bestScore = _cost[bestIndex] + Heuristic(bestIndex, goalIndex);

                for (int i = 1; i < _open.Count; i++)
                {
                    int candidate = _open[i];
                    int score = _cost[candidate] + Heuristic(candidate, goalIndex);
                    if (score >= bestScore)
                        continue;

                    bestScore = score;
                    bestIndex = candidate;
                    bestListIndex = i;
                }

                _open.RemoveAt(bestListIndex);
                if (bestIndex == goalIndex)
                {
                    BuildPath(startIndex, goalIndex, result);
                    return result.Count > 0;
                }

                Vector2Int cell = FromIndex(bestIndex);
                VisitNeighbor(bestIndex, cell.x + 1, cell.y);
                VisitNeighbor(bestIndex, cell.x - 1, cell.y);
                VisitNeighbor(bestIndex, cell.x, cell.y + 1);
                VisitNeighbor(bestIndex, cell.x, cell.y - 1);
            }

            return false;
        }

        private void VisitNeighbor(int currentIndex, int x, int y)
        {
            if (!IsInside(x, y))
                return;

            int nextIndex = ToIndex(x, y);
            if (_blocked[nextIndex])
                return;

            int nextCost = _cost[currentIndex] + 1;
            if (nextCost >= _cost[nextIndex])
                return;

            _cost[nextIndex] = nextCost;
            _cameFrom[nextIndex] = currentIndex;
            if (!_open.Contains(nextIndex))
                _open.Add(nextIndex);
        }

        private void BuildPath(int startIndex, int goalIndex, List<Vector2Int> result)
        {
            int current = goalIndex;
            int guard = _width * _height + 1;
            while (current >= 0 && guard-- > 0)
            {
                result.Add(FromIndex(current));
                if (current == startIndex)
                    break;
                current = _cameFrom[current];
            }

            if (result.Count == 0 || result[result.Count - 1] != FromIndex(startIndex))
            {
                result.Clear();
                return;
            }

            result.Reverse();
        }

        private int Heuristic(int a, int b)
        {
            Vector2Int left = FromIndex(a);
            Vector2Int right = FromIndex(b);
            return Mathf.Abs(left.x - right.x) + Mathf.Abs(left.y - right.y);
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _width && y < _height;
        }

        private int ToIndex(int x, int y)
        {
            return y * _width + x;
        }

        private Vector2Int FromIndex(int index)
        {
            return new Vector2Int(index % _width, index / _width);
        }
    }
}
