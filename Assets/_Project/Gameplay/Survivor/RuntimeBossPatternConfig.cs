using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    [CreateAssetMenu(menuName = "KaiChe/Survivor/Boss Pattern Config", fileName = "BossPatternConfig")]
    internal sealed class RuntimeBossPatternConfig : ScriptableObject
    {
        [SerializeReference, SubclassSelector]
        [ReorderableList]
        private List<BossPatternStep> _steps = new List<BossPatternStep>();

        internal IReadOnlyList<BossPatternStep> Steps => _steps;
    }

    [Serializable]
    internal abstract class BossPatternStep
    {
        [SerializeField, MinValue(0.02f)]
        private float _duration = 1f;

        internal float Duration => Mathf.Max(0.02f, _duration);
        internal abstract BossPatternKind Kind { get; }
    }

    internal enum BossPatternKind
    {
        Ring,
        Spread,
        Spiral,
        Aimed
    }

    [Serializable]
    internal sealed class RingPatternStep : BossPatternStep
    {
        [SerializeField, MinValue(1)] private int _projectileCount = 12;
        [SerializeField] private float _angleOffset;

        internal override BossPatternKind Kind => BossPatternKind.Ring;
        internal int ProjectileCount => Mathf.Max(1, _projectileCount);
        internal float AngleOffset => _angleOffset;
    }

    [Serializable]
    internal sealed class SpreadPatternStep : BossPatternStep
    {
        [SerializeField, MinValue(1)] private int _projectileCount = 5;
        [SerializeField, MinValue(0f)] private float _arcDegrees = 60f;

        internal override BossPatternKind Kind => BossPatternKind.Spread;
        internal int ProjectileCount => Mathf.Max(1, _projectileCount);
        internal float ArcDegrees => Mathf.Max(0f, _arcDegrees);
    }

    [Serializable]
    internal sealed class SpiralPatternStep : BossPatternStep
    {
        [SerializeField, MinValue(1)] private int _projectileCount = 8;
        [SerializeField] private float _degreesPerShot = 17f;

        internal override BossPatternKind Kind => BossPatternKind.Spiral;
        internal int ProjectileCount => Mathf.Max(1, _projectileCount);
        internal float DegreesPerShot => _degreesPerShot;
    }

    [Serializable]
    internal sealed class AimedPatternStep : BossPatternStep
    {
        [SerializeField, MinValue(1)] private int _bursts = 3;
        [SerializeField, MinValue(0.02f)] private float _burstInterval = 0.2f;

        internal override BossPatternKind Kind => BossPatternKind.Aimed;
        internal int Bursts => Mathf.Max(1, _bursts);
        internal float BurstInterval => Mathf.Max(0.02f, _burstInterval);
    }
}
