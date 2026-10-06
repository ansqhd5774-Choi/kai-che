using System.Collections.Generic;
using KaiChe.Core.Content;
using KaiChe.Core.Progression;
using KaiChe.Gameplay;
using KaiChe.Gameplay.Adventure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Gameplay.Survivor
{
    public sealed class SurvivorArenaController : MonoBehaviour
    {
        private sealed class EnemyState
        {
            public GameObject View;
            public MonsterDefinition Definition;
            public float Health;
            public float MaxHealth;
            public float AnimationPhase;
            public float BaseScale;
            public bool IsAggro;
            public float SlowTimer;
            public float AttackTimer;
            public float SpecialTimer;
            public float BurstTimer;
            public bool SpecialArmed;
            public bool PhaseTwoTriggered;
            public bool BossUsesLineSpecial;
            public Vector2 SpecialDirection;
            public int SpecialSequence;
            public bool IsElite;
            public int EliteKind;
            public float MoveMultiplier = 1f;
            public float DamageMultiplier = 1f;
            public int ExperienceMultiplier = 1;
            public GameObject SlowAura;
        }

        private sealed class ProjectileState
        {
            public GameObject View;
            public EnemyState Target;
            public Vector2 Direction;
            public float Life;
            public SkillDefinition Skill;
            public bool IsFireball;
            public bool IsIceBall;
        }

        private sealed class EffectState
        {
            public GameObject View;
            public float Life;
            public float MaxLife;
            public Vector3 StartScale;
            public float SpinSpeed;
            public Vector3 Velocity;
        }

        private readonly List<EnemyState> _enemies = new List<EnemyState>();
        private readonly List<ProjectileState> _projectiles = new List<ProjectileState>();
        private readonly List<EffectState> _effects = new List<EffectState>();
        private Material _materialTemplate;
        private GameObject _player;
        private Transform _weaponRoot;
        private RuntimeCombatVisualAnimator _playerVisualAnimator;
        private Vector2 _moveInput;
        private Vector2 _playerWorldPosition;
        private float _spawnTimer;
        private int _spawnSequence;
        private readonly Dictionary<string, float> _skillShotTimers =
            new Dictionary<string, float>();
        private readonly Dictionary<int, Material> _runtimeMaterialCache =
            new Dictionary<int, Material>();
        private readonly HashSet<string> _loggedBaseSkillShots =
            new HashSet<string>();
        private readonly HashSet<string> _activeBaseSkillIds =
            new HashSet<string>();
        private readonly Dictionary<string, int> _baseSkillUpgradeRanks =
            new Dictionary<string, int>();
        private readonly string[] _skillChoiceCandidateIds =
            new string[3];
        private int _skillChoiceGeneration;
        private float _moveAnimationTime;
        private float _attackAnimationTime;
        private float _playerHitFlashTimer;
        private float _cameraShakeTimer;
        private float _cameraShakeMagnitude;
        private float _slowMotionTimer;
        private float _survivalTime;
        private int _lastWaveLog = 1;
        private int _lastBossWaveSpawned;
        private int _lastActiveLoadoutWave;
        private int _comboCount;
        private float _comboTimer;
        private float _frenzyTimer;
        private bool _frenzyFirstLogged;
        private int _pendingSkillChoices;
        private int _fireUpgradeRank;
        private int _iceUpgradeRank;
        private int _arcaneUpgradeRank;
        private int _projectileHitSequence;
        private int _pendingBossRewards;
        private int _bossAttackRewardRank;
        private int _bossVitalityRewardRank;
        private int _bossTempoRewardRank;
        private bool _bossRewardStateNeedsMigration;
        private bool _skillMasteryStateNeedsMigration;
        private bool _skillMasterySaveDirty;
        private float _skillMasterySaveTimer;
        private bool _adventureHealthSaveDirty;
        private float _adventureHealthSaveTimer;
        private bool _skillChoiceFirstLogged;
        private bool _initialized;
        private bool _fireballFirstShotLogged;
        private bool _iceballFirstShotLogged;
        private bool _playerHitFirstLogged;
        private bool _runnerSpawnLogged;
        private bool _bruteSpawnLogged;
        private bool _runnerDashLogged;
        private bool _bruteSlamLogged;
        private bool _bossSpawnLogged;
        private bool _bossPulseLogged;
        private bool _thermalShockFirstLogged;
        private bool _arcaneChainFirstLogged;
        private bool _bossDefeatLogged;
        private bool _eliteSpawnLogged;
        private bool _bossLineLogged;
        private const float PlayerBaseScale = 0.58f;
        private const float IceSlowDuration = 2.0f;
        private const float IceSlowMultiplier = 0.60f;
        private const float AttackAnimationDuration = 0.18f;
        private const float EnemyContactRadius = 0.34f;
        private const float EnemyAttackInterval = 0.55f;
        private const float EnemyContactKnockback = 0.22f;
        private const float PlayerHitFlashDuration = 0.18f;
        private const float RunnerDashInterval = 2.6f;
        private const float RunnerDashDuration = 0.42f;
        private const float RunnerDashSpeedMultiplier = 2.15f;
        private const float BruteSlamRadius = 0.82f;
        private const float BruteSlamInterval = 1.65f;
        private const float BruteSlamKnockback = 0.38f;
        private const float WaveDuration = 30f;
        private const float PressureWaveDuration = 8f;
        private const float FireDamagePerRank = 0.28f;
        private const float FireCooldownPerRank = 0.06f;
        private const float IceDamagePerRank = 0.18f;
        private const float IceSlowDurationPerRank = 0.35f;
        private const float IndependentBaseSkillDamagePerRank = 0.14f;
        private const float IndependentBaseSkillCooldownPerRank = 0.04f;
        private const float IndependentBaseSkillProjectileSpeedPerRank = 0.05f;
        private const int FireEvolutionRank = 3;
        private const int IceEvolutionRank = 3;
        private const float FireSplashRadius = 1.35f;
        private const float FireSplashDamageMultiplier = 0.45f;
        private const float IcePulseRadius = 1.60f;
        private const int BossWaveInterval = 3;
        private const float BossPulseRadius = 2.75f;
        private const float BossPulseInterval = 3.4f;
        private const float BossTelegraphDuration = 0.65f;
        private const float BossPulseDamageWindow = 0.90f;
        private const float BossHealthScalePerWave = 0.16f;
        private const float BossPhaseTwoHealthThreshold = 0.50f;
        private const float BossPhaseTwoPulseRadius = 3.35f;
        private const float BossPhaseTwoPulseInterval = 2.45f;
        private const float BossPhaseTwoTelegraphDuration = 0.50f;
        private const float BossLineLength = 7.0f;
        private const float BossLineHalfWidth = 0.72f;
        private const float BossLineDamageMultiplier = 1.15f;
        private const int ThermalShockRequiredFireRank = 2;
        private const int ThermalShockRequiredIceRank = 2;
        private const float ThermalShockBonusMultiplier = 0.90f;
        private const float ThermalShockRadius = 1.75f;
        private const float ThermalShockSplashMultiplier = 0.32f;
        private const int ArcaneChainRequiredRank = 2;
        private const int ArcaneChainRequiredElementRank = 2;
        private const int ArcaneChainHitInterval = 5;
        private const float ArcaneChainDamageMultiplier = 0.55f;
        private const float ArcaneChainRadius = 3.4f;
        private const float BossFinishSlowMotionScale = 0.22f;
        private const float BossFinishSlowMotionDuration = 0.42f;
        private const float LevelDamagePerLevel = 0.06f;
        private const float LevelHealthPerLevel = 5f;
        private const float LevelHealFraction = 0.30f;
        private const float BodyHealthPerPoint = 20f;
        private const float AgilityMoveSpeedPerPoint = 0.05f;
        private const float PerceptionRangePerPoint = 0.06f;
        private const float PerceptionProjectileSpeedPerPoint = 0.06f;
        private const float IntelligenceDamagePerPoint = 0.08f;
        private const float WisdomCooldownPerPoint = 0.05f;
        private const float WillDamageReductionPerPoint = 0.06f;
        private const float GoodIceDamagePerPoint = 0.10f;
        private const float EvilFireDamagePerPoint = 0.10f;
        private const int EliteSpawnInterval = 7;
        private const float EliteSwiftHealthMultiplier = 1.40f;
        private const float EliteSwiftMoveMultiplier = 1.35f;
        private const float EliteSwiftDamageMultiplier = 1.18f;
        private const float EliteBulwarkHealthMultiplier = 2.20f;
        private const float EliteBulwarkMoveMultiplier = 0.82f;
        private const float EliteBulwarkDamageMultiplier = 1.34f;
        private const float EliteFrenzyHealthMultiplier = 1.70f;
        private const float EliteFrenzyMoveMultiplier = 1.12f;
        private const float EliteFrenzyDamageMultiplier = 1.55f;
        private const float BossRewardDamagePerRank = 0.18f;
        private const float BossRewardHealthPerRank = 28f;
        private const float BossRewardCooldownPerRank = 0.08f;
        private const int MaxVisibleEnemies = 100;
        private const int MaxActiveProjectiles = 56;
        private const float ComboWindow = 3.0f;
        private const int FrenzyComboThreshold = 5;
        private const float FrenzyDuration = 5.5f;
        private const float FrenzyDamageMultiplier = 1.18f;
        private const float FrenzyCooldownMultiplier = 0.82f;
        private const float FrenzyMoveSpeedMultiplier = 1.12f;
        private const float FrenzyHealFraction = 0.08f;
        private const float PotionHealFraction = 0.35f;
        private const int MaxActiveBaseSkills = 4;
        private const float PerspectiveCameraHeight = 12.5f;
        private const float PerspectiveCameraBackDistance = 13.5f;
        private const float PerspectiveCameraSideOffset = -3.2f;
        private const float PerspectiveCameraFieldOfView = 60f;
        private static readonly Vector2 TownEntrancePosition =
            new Vector2(0f, -8.5f);
        private const float TownEntranceRadius = 0.72f;
        private const string ProgressionSavePrefix =
            "KaiChe.PrototypeProgression.";
        private const int SkillProgressionSaveVersion = 1;
        private const int BossRewardSaveVersion = 1;
        private const int SkillMasterySaveVersion = 1;
        private const float SkillMasterySaveInterval = 1f;
        private const float AdventureHealthSaveInterval = 1f;

        private readonly SkillMasteryState _skillMastery = new SkillMasteryState();
        private readonly CharacterProgressionState _progression = new CharacterProgressionState();

        public float PlayerHealth { get; private set; } = 100f;
        public float MaxPlayerHealth =>
            100f
            + Mathf.Min(Mathf.Max(0, Level - 1), 50) * LevelHealthPerLevel
            + GetAttributeValue(CharacterAttribute.Body) * BodyHealthPerPoint
            + _bossVitalityRewardRank * BossRewardHealthPerRank;
        public float LevelDamageBonusPercent =>
            Mathf.Min(Mathf.Max(0, Level - 1), 25)
            * LevelDamagePerLevel
            * 100f;
        public int Kills { get; private set; }
        public int Experience { get; private set; }
        public int PotionCount => PrototypeAdventureState.PotionCount;
        public bool IsDead => PlayerHealth <= 0f;
        public bool CanUsePotion =>
            !IsDead
            && !IsAttributeAllocationPending
            && !IsSkillChoicePending
            && !IsBossRewardPending
            && PotionCount > 0
            && PlayerHealth < MaxPlayerHealth - 0.01f;
        public int EnemyCount => _enemies.Count;
        public Vector2 PlayerWorldPosition => _playerWorldPosition;
        public string ActiveSkillId => RuntimeContentCatalog.StarterSkill.Id;
        public string ActiveSkillName => RuntimeContentCatalog.StarterSkill.DisplayName;
        public string ActiveMonsterId => RuntimeContentCatalog.StarterMonster.Id;
        public int ActiveSkillMastery => _skillMastery.GetMastery(ActiveSkillId);
        public string IceSkillName => RuntimeContentCatalog.IceBall.DisplayName;
        public int IceSkillMastery =>
            _skillMastery.GetMastery(RuntimeContentCatalog.IceBall.Id);
        public int ActiveBaseSkillCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
                {
                    if (IsBaseSkillActive(i))
                        count += 1;
                }
                return count;
            }
        }
        public string ActiveBaseSkillSummary => BuildActiveBaseSkillSummary();
        public string ActiveBaseSkillProgressSummary =>
            BuildActiveBaseSkillProgressSummary();
        public int MaxActiveBaseSkillCount => MaxActiveBaseSkills;
        public int ComboCount => _comboCount;
        public bool IsFrenzyActive => _frenzyTimer > 0f;
        public float FrenzyTimeRemaining => _frenzyTimer;
        public bool IsPlayerHitFlashActive => _playerHitFlashTimer > 0f;
        public float SurvivalTime => _survivalTime;
        public int Wave => 1 + Mathf.FloorToInt(_survivalTime / WaveDuration);
        public float WaveProgress =>
            (_survivalTime % WaveDuration) / WaveDuration;
        public bool IsPressureWave =>
            (_survivalTime % WaveDuration)
            >= WaveDuration - PressureWaveDuration;
        public bool IsBossWave =>
            Wave >= BossWaveInterval && Wave % BossWaveInterval == 0;
        public string BossName => RuntimeContentCatalog.BossMonster.DisplayName;
        public bool HasBossAlive
        {
            get
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    EnemyState enemy = _enemies[i];
                    if (enemy != null
                        && enemy.View != null
                        && enemy.Definition != null
                        && enemy.Definition.Id == ContentIds.BossMonster)
                        return true;
                }

                return false;
            }
        }
        public float BossHealthNormalized
        {
            get
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    EnemyState enemy = _enemies[i];
                    if (enemy != null
                        && enemy.View != null
                        && enemy.Definition != null
                        && enemy.Definition.Id == ContentIds.BossMonster)
                    {
                        return Mathf.Clamp01(
                            enemy.Health / Mathf.Max(1f, enemy.MaxHealth));
                    }
                }

                return 0f;
            }
        }
        public int UnlockedSkillCount => _skillMastery.UnlockedSkillCount;
        public string LastUnlockedSkillId => _skillMastery.LastUnlockedSkillId;
        public int Level => _progression.Level;
        public long LevelExperience => _progression.LevelExperience;
        public long ExperienceRequiredForNextLevel =>
            _progression.ExperienceRequiredForNextLevel;
        public int UnspentAttributePoints => _progression.UnspentAttributePoints;
        public bool IsAttributeAllocationPending =>
            _progression.UnspentAttributePoints > 0;
        public bool IsSkillChoicePending => _pendingSkillChoices > 0;
        public int PendingSkillChoices => _pendingSkillChoices;
        public int FireUpgradeRank => _fireUpgradeRank;
        public int IceUpgradeRank => _iceUpgradeRank;
        public int ArcaneUpgradeRank => _arcaneUpgradeRank;
        public int EliteCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _enemies.Count; i++)
                {
                    EnemyState enemy = _enemies[i];
                    if (enemy != null && enemy.View != null && enemy.IsElite)
                        count += 1;
                }
                return count;
            }
        }
        public bool IsBossRewardPending => _pendingBossRewards > 0;
        public int PendingBossRewards => _pendingBossRewards;
        public int BossAttackRewardRank => _bossAttackRewardRank;
        public int BossVitalityRewardRank => _bossVitalityRewardRank;
        public int BossTempoRewardRank => _bossTempoRewardRank;
        public bool IsThermalShockUnlocked =>
            _fireUpgradeRank >= ThermalShockRequiredFireRank
            && _iceUpgradeRank >= ThermalShockRequiredIceRank;
        public bool IsArcaneChainUnlocked =>
            _arcaneUpgradeRank >= ArcaneChainRequiredRank
            && (_fireUpgradeRank >= ArcaneChainRequiredElementRank
                || _iceUpgradeRank >= ArcaneChainRequiredElementRank);
        public bool IsBossPhaseTwo
        {
            get
            {
                for (int i = 0; i < _enemies.Count; i++)
                {
                    EnemyState enemy = _enemies[i];
                    if (enemy != null
                        && enemy.View != null
                        && enemy.Definition != null
                        && enemy.Definition.Id == ContentIds.BossMonster
                        && enemy.PhaseTwoTriggered)
                        return true;
                }

                return false;
            }
        }

        public string GetBossRewardTitle(int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0: return "전리품 · 파괴의 인장";
                case 1: return "전리품 · 생명의 핵";
                case 2: return "전리품 · 시간의 파편";
                default: return "알 수 없는 전리품";
            }
        }

        public string GetBossRewardDescription(int choiceIndex)
        {
            switch (choiceIndex)
            {
                case 0:
                    return $"+{Mathf.RoundToInt(BossRewardDamagePerRank * 100f)}% 전체 스킬 피해";
                case 1:
                    return $"+{Mathf.RoundToInt(BossRewardHealthPerRank)} 최대 체력 · 즉시 회복";
                case 2:
                    return $"재사용 대기시간 -{Mathf.RoundToInt(BossRewardCooldownPerRank * 100f)}%";
                default:
                    return string.Empty;
            }
        }

        public bool TryChooseBossReward(int choiceIndex)
        {
            if (_pendingBossRewards <= 0)
                return false;

            switch (choiceIndex)
            {
                case 0:
                    _bossAttackRewardRank += 1;
                    Debug.Log("KAI_CHE_BOSS_REWARD_ATTACK_" + _bossAttackRewardRank);
                    break;
                case 1:
                    _bossVitalityRewardRank += 1;
                    PlayerHealth = Mathf.Min(
                        MaxPlayerHealth,
                        PlayerHealth + BossRewardHealthPerRank);
                    MarkAdventureHealthDirty();
                    Debug.Log("KAI_CHE_BOSS_REWARD_VITALITY_" + _bossVitalityRewardRank);
                    break;
                case 2:
                    _bossTempoRewardRank += 1;
                    Debug.Log("KAI_CHE_BOSS_REWARD_TEMPO_" + _bossTempoRewardRank);
                    break;
                default:
                    return false;
            }

            _pendingBossRewards -= 1;
            SavePrototypeProgression();
            Debug.Log(
                "KAI_CHE_R25_BOSS_REWARD_STATE_SAVED_"
                + BuildBossRewardStateSummary());
            CreateLevelUpPowerEffect(PlayerPosition);
            TriggerImpactFeedback(0.11f, false);
            return true;
        }

        public string GetSkillUpgradeTitle(int choiceIndex)
        {
            SkillDefinition skill = GetSkillChoiceDefinition(choiceIndex);
            if (skill == null)
                return "스킬 선택 대기";

            if (!IsBaseSkillOwned(skill.Id))
                return "신규 습득 · " + skill.DisplayName;

            if (skill.Id == ContentIds.StarterSkill)
            {
                return _fireUpgradeRank + 1 >= FireEvolutionRank
                    ? "화염 진화 · 태양 폭발"
                    : "파이어볼 강화 · Lv." + (_fireUpgradeRank + 2);
            }

            if (skill.Id == ContentIds.IceBall)
            {
                return _iceUpgradeRank + 1 >= IceEvolutionRank
                    ? "냉기 진화 · 절대영도 파동"
                    : "아이스볼 강화 · Lv." + (_iceUpgradeRank + 2);
            }

            return skill.DisplayName
                + " 강화 · Lv."
                + (GetBaseSkillUpgradeRank(skill.Id) + 2);
        }

        public string GetSkillUpgradeDescription(int choiceIndex)
        {
            SkillDefinition skill = GetSkillChoiceDefinition(choiceIndex);
            if (skill == null)
                return string.Empty;

            if (!IsBaseSkillOwned(skill.Id))
            {
                return "기본기 습득 · 피해 "
                    + Mathf.RoundToInt(skill.Damage)
                    + " · 재사용 "
                    + skill.Cooldown.ToString("0.00")
                    + "초 · 사거리 "
                    + skill.Range.ToString("0.0");
            }

            if (skill.Id == ContentIds.StarterSkill)
            {
                return $"+{Mathf.RoundToInt(FireDamagePerRank * 100f)}% 피해"
                    + $" · 재사용 -{Mathf.RoundToInt(FireCooldownPerRank * 100f)}%"
                    + (_fireUpgradeRank + 1 == FireEvolutionRank
                        ? " · 이번 강화부터 명중 시 광역 폭발"
                        : string.Empty);
            }

            if (skill.Id == ContentIds.IceBall)
            {
                return $"+{Mathf.RoundToInt(IceDamagePerRank * 100f)}% 피해"
                    + $" · 둔화 +{IceSlowDurationPerRank:0.00}초"
                    + (_iceUpgradeRank + 1 == IceEvolutionRank
                        ? " · 이번 강화부터 주변 적 냉기 파동"
                        : string.Empty);
            }

            return $"+{Mathf.RoundToInt(IndependentBaseSkillDamagePerRank * 100f)}% 피해"
                + $" · 재사용 -{Mathf.RoundToInt(IndependentBaseSkillCooldownPerRank * 100f)}%"
                + $" · 탄속 +{Mathf.RoundToInt(IndependentBaseSkillProjectileSpeedPerRank * 100f)}%";
        }

        public bool TryChooseSkillUpgrade(int choiceIndex)
        {
            if (_pendingSkillChoices <= 0)
                return false;

            SkillDefinition skill = GetSkillChoiceDefinition(choiceIndex);
            if (skill == null)
                return false;

            if (!IsBaseSkillOwned(skill.Id))
            {
                if (_activeBaseSkillIds.Count >= MaxActiveBaseSkills)
                    return false;

                _activeBaseSkillIds.Add(skill.Id);
                EnsureBaseSkillRankEntry(skill.Id);
                Debug.Log("KAI_CHE_R23_SKILL_ACQUIRE_" + skill.Id);
            }
            else
            {
                int nextRank = GetBaseSkillUpgradeRank(skill.Id) + 1;
                _baseSkillUpgradeRanks[skill.Id] = nextRank;

                if (skill.Id == ContentIds.StarterSkill)
                {
                    _fireUpgradeRank = nextRank;
                    Debug.Log("KAI_CHE_SKILL_PICK_FIRE_" + _fireUpgradeRank);
                }
                else if (skill.Id == ContentIds.IceBall)
                {
                    _iceUpgradeRank = nextRank;
                    Debug.Log("KAI_CHE_SKILL_PICK_ICE_" + _iceUpgradeRank);
                }
                else
                {
                    _arcaneUpgradeRank += 1;
                }

                Debug.Log(
                    "KAI_CHE_R23_SKILL_UPGRADE_"
                    + skill.Id
                    + "_"
                    + nextRank);
            }

            _pendingSkillChoices -= 1;
            _lastActiveLoadoutWave = 0;
            LogActiveBaseSkillLoadout();

            if (_pendingSkillChoices > 0)
                GenerateSkillChoiceCandidates();
            else
                ClearSkillChoiceCandidates();

            SavePrototypeProgression();
            Debug.Log(
                "KAI_CHE_R24_SKILL_STATE_SAVED_COUNT_"
                + ActiveBaseSkillCount
                + "_"
                + BuildActiveBaseSkillProgressSummary());

            return true;
        }

        public void Initialize(Material materialTemplate)
        {
            _materialTemplate = materialTemplate;
            LoadPrototypeProgression();
            PrototypeAdventureState.EnsureLoaded();

            if (PrototypeAdventureState.Health <= 0.01f)
            {
                PrototypeAdventureState.PrepareCombatRetry(MaxPlayerHealth);
                Debug.Log(
                    "KAI_CHE_R28_DEFEAT_RECOVERY_BOOTSTRAP_MAX_"
                    + Mathf.RoundToInt(MaxPlayerHealth));
            }

            Debug.Log(
                "KAI_CHE_R29_POTION_INVENTORY_ACTIVE_COUNT_"
                + PrototypeAdventureState.PotionCount);

            PlayerHealth = Mathf.Min(
                MaxPlayerHealth,
                PrototypeAdventureState.Health);
            MarkAdventureHealthDirty();
            FlushAdventureHealthPersistence();
            Debug.Log("KAI_CHE_R27_HEALTH_PERSISTENCE_ACTIVE");
            if (DoesPersistedAdventureHealthMatchRuntime())
            {
                Debug.Log(
                    "KAI_CHE_R27_HEALTH_RESTORE_PASS_CURRENT_"
                    + Mathf.RoundToInt(PlayerHealth)
                    + "_MAX_"
                    + Mathf.RoundToInt(MaxPlayerHealth));
            }
            else
            {
                Debug.LogError(
                    "KAI_CHE_R27_HEALTH_RESTORE_MISMATCH_CURRENT_"
                    + Mathf.RoundToInt(PlayerHealth)
                    + "_MAX_"
                    + Mathf.RoundToInt(MaxPlayerHealth));
            }
            if (MaxPlayerHealth > PrototypeAdventureState.BaseMaxHealth + 0.01f
                && PrototypeAdventureState.MaxHealth
                    > PrototypeAdventureState.BaseMaxHealth + 0.01f)
            {
                Debug.Log(
                    "KAI_CHE_R27_DYNAMIC_MAX_HEALTH_RESTORED_"
                    + Mathf.RoundToInt(PrototypeAdventureState.MaxHealth));
            }
            ConfigureCamera();

            RuntimeBattlefieldEnvironment.Build(
                transform,
                _materialTemplate,
                TownEntrancePosition);
            _player = CreatePlayerView();
            InitializeBaseSkillLoadout();

            _spawnTimer = 0.2f;
            _skillShotTimers.Clear();
            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
            {
                SkillDefinition skill = RuntimeContentCatalog.BaseSkills[i];
                _skillShotTimers[skill.Id] = 0.08f + i * 0.09f;
            }

            _lastActiveLoadoutWave = 0;
            _pendingSkillChoices = Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    ProgressionSavePrefix + "PendingSkillChoices",
                    0));
            int savedSkillChoiceGeneration = Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    ProgressionSavePrefix + "SkillChoiceGeneration",
                    0));
            _skillChoiceGeneration = _pendingSkillChoices > 0
                ? Mathf.Max(0, savedSkillChoiceGeneration - 1)
                : savedSkillChoiceGeneration;
            ClearSkillChoiceCandidates();
            if (_pendingSkillChoices > 0)
            {
                GenerateSkillChoiceCandidates();
                Debug.Log(
                    "KAI_CHE_R24_PENDING_SKILL_CHOICES_RESTORED_"
                    + _pendingSkillChoices);
            }
            Debug.Log("KAI_CHE_R24_PENDING_SKILL_CHOICE_PERSISTENCE_ACTIVE");

            bool migrateSkillMastery = _skillMasteryStateNeedsMigration;
            bool migrateBossRewards = _bossRewardStateNeedsMigration;
            if (migrateSkillMastery || migrateBossRewards)
            {
                SavePrototypeProgression();
                _skillMasteryStateNeedsMigration = false;
                _bossRewardStateNeedsMigration = false;

                if (migrateSkillMastery)
                    Debug.Log("KAI_CHE_R26_SKILL_MASTERY_STATE_MIGRATED");
                if (migrateBossRewards)
                    Debug.Log("KAI_CHE_R25_BOSS_REWARD_STATE_MIGRATED");
            }

            Debug.Log("KAI_CHE_R26_SKILL_MASTERY_PERSISTENCE_ACTIVE");
            if (DoesPersistedSkillMasteryStateMatchRuntime())
            {
                Debug.Log(
                    "KAI_CHE_R26_SKILL_MASTERY_RESTORE_PASS_"
                    + BuildSkillMasteryStateSummary());
            }
            else
            {
                Debug.LogError(
                    "KAI_CHE_R26_SKILL_MASTERY_RESTORE_MISMATCH_"
                    + BuildSkillMasteryStateSummary());
            }

            Debug.Log("KAI_CHE_R25_BOSS_REWARD_PERSISTENCE_ACTIVE");
            if (DoesPersistedBossRewardStateMatchRuntime())
            {
                Debug.Log(
                    "KAI_CHE_R25_BOSS_REWARD_STATE_RESTORE_PASS_"
                    + BuildBossRewardStateSummary());
            }
            else
            {
                Debug.LogError(
                    "KAI_CHE_R25_BOSS_REWARD_STATE_RESTORE_MISMATCH_"
                    + BuildBossRewardStateSummary());
            }

            _comboCount = 0;
            _comboTimer = 0f;
            _frenzyTimer = 0f;
            Debug.Log(
                "KAI_CHE_R21_BASE_SKILLS_READY_"
                + RuntimeContentCatalog.BaseSkills.Length);
            Debug.Log("KAI_CHE_R23_PLAYER_SELECTED_LOADOUT_ACTIVE");
            Debug.Log("KAI_CHE_R21_COMBAT_MOMENTUM_ACTIVE");
            Debug.Log("KAI_CHE_R21_FRENZY_REWARD_ACTIVE");
            Debug.Log("KAI_CHE_R22_FRENZY_TEMPO_ACTIVE_5_3000_5500");
            Debug.Log("KAI_CHE_R22_BASE_SKILL_GROWTH_ACTIVE");
            Debug.Log("KAI_CHE_R23_INDEPENDENT_SKILL_GROWTH_ACTIVE");
            Debug.Log("KAI_CHE_R23_MAX_ACTIVE_SKILLS_" + MaxActiveBaseSkills);
            Debug.Log("KAI_CHE_R24_SKILL_PERSISTENCE_ACTIVE");
            Debug.Log(
                "KAI_CHE_R24_SKILL_STATE_CURRENT_"
                + BuildActiveBaseSkillProgressSummary());
            LogActiveBaseSkillLoadout();
            _initialized = true;
        }

        public void SetMoveInput(Vector2 input)
        {
            _moveInput = IsAttributeAllocationPending
                || IsSkillChoicePending
                || IsBossRewardPending
                ? Vector2.zero
                : Vector2.ClampMagnitude(input, 1f);
        }

        public int GetAttributeValue(CharacterAttribute attribute)
        {
            return _progression.GetAttribute(attribute);
        }

        public bool PrepareRetryAfterDefeat()
        {
            if (!IsDead)
                return false;

            PrepareDefeatRecoveryCheckpoint("MANUAL");
            return true;
        }

        public bool TryUsePotion()
        {
            if (!CanUsePotion)
                return false;

            float previousHealth = PlayerHealth;
            float healedHealth = Mathf.Min(
                MaxPlayerHealth,
                PlayerHealth + MaxPlayerHealth * PotionHealFraction);

            if (!PrototypeAdventureState.TryConsumePotionAndSetHealth(
                    healedHealth,
                    MaxPlayerHealth))
                return false;

            PlayerHealth = healedHealth;
            _adventureHealthSaveDirty = false;
            _adventureHealthSaveTimer = 0f;

            CreateLevelUpPowerEffect(PlayerPosition);
            TriggerImpactFeedback(0.08f, false);
            Debug.Log(
                "KAI_CHE_R29_POTION_USED_REMAINING_"
                + PotionCount
                + "_HEAL_"
                + Mathf.RoundToInt(PlayerHealth - previousHealth)
                + "_CURRENT_"
                + Mathf.RoundToInt(PlayerHealth)
                + "_MAX_"
                + Mathf.RoundToInt(MaxPlayerHealth));
            return true;
        }

        public bool TrySpendAttributePoint(CharacterAttribute attribute)
        {
            float oldMaxHealth = MaxPlayerHealth;
            bool spent = _progression.TrySpendAttributePoints(attribute);
            if (!spent)
                return false;

            if (attribute == CharacterAttribute.Body)
            {
                float healthGain = Mathf.Max(
                    0f,
                    MaxPlayerHealth - oldMaxHealth);
                PlayerHealth = Mathf.Min(
                    MaxPlayerHealth,
                    PlayerHealth + healthGain);
                MarkAdventureHealthDirty();
            }

            SavePrototypeProgression();
            Debug.Log(
                "KAI_CHE_ATTRIBUTE_POWER_"
                + attribute
                + "_"
                + GetAttributeValue(attribute));
            return true;
        }

        private void Update()
        {
            UpdateCinematicTime();

            if (!_initialized)
                return;

            float deltaTime = Time.deltaTime;
            float unscaledDeltaTime = Time.unscaledDeltaTime;
            UpdateSkillMasteryPersistence(unscaledDeltaTime);
            UpdateAdventureHealthPersistence(unscaledDeltaTime);
            _playerHitFlashTimer = Mathf.Max(
                0f,
                _playerHitFlashTimer - deltaTime);

            if (IsDead
                || IsAttributeAllocationPending
                || IsSkillChoicePending
                || IsBossRewardPending)
                return;

            UpdateSurvivalWave(deltaTime);
            UpdateCombatMomentum(deltaTime);
            UpdateWorldMovement(deltaTime);
            UpdateTownEntrance();
            UpdateSpawning(deltaTime);
            UpdateEnemies(deltaTime);
            UpdateShooting(deltaTime);
            UpdateProjectiles(deltaTime);
            UpdatePlayerAnimation(deltaTime);
            UpdateEffects(deltaTime);
        }

        private void OnDisable()
        {
            FlushAdventureHealthPersistence();
            FlushSkillMasteryPersistence();

            if (Time.timeScale != 1f)
                Time.timeScale = 1f;
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused)
                return;

            FlushAdventureHealthPersistence();
            FlushSkillMasteryPersistence();
        }

        private void OnApplicationQuit()
        {
            FlushAdventureHealthPersistence();
            FlushSkillMasteryPersistence();
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<int, Material> pair in _runtimeMaterialCache)
            {
                if (pair.Value != null)
                    Destroy(pair.Value);
            }

            _runtimeMaterialCache.Clear();
        }

        private void UpdateCinematicTime()
        {
            if (_slowMotionTimer <= 0f)
                return;

            _slowMotionTimer = Mathf.Max(
                0f,
                _slowMotionTimer - Time.unscaledDeltaTime);

            if (_slowMotionTimer <= 0f)
                Time.timeScale = 1f;
        }

        private void TriggerHitStop(
            EnemyState enemy,
            float damage)
        {
            if (_slowMotionTimer > 0.10f)
                return;

            bool isBoss = enemy != null
                && enemy.Definition != null
                && enemy.Definition.Id == ContentIds.BossMonster;
            bool isElite = enemy != null && enemy.IsElite;

            float duration = isBoss
                ? 0.070f
                : isElite
                    ? 0.055f
                    : 0.035f;
            float scale = isBoss
                ? 0.35f
                : isElite
                    ? 0.42f
                    : 0.55f;

            _slowMotionTimer = Mathf.Max(
                _slowMotionTimer,
                duration);
            Time.timeScale = Mathf.Min(Time.timeScale, scale);
            CreateDamageNumber(
                enemy != null && enemy.View != null
                    ? RuntimeWorldSpace.ToLogical(enemy.View.transform.position)
                    : PlayerPosition,
                damage,
                isElite || isBoss);
        }

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            camera.orthographic = false;
            camera.fieldOfView = PerspectiveCameraFieldOfView;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 120f;
            PositionPerspectiveCamera(camera, Vector2.zero);
            camera.backgroundColor = new Color(0.018f, 0.022f, 0.018f);
            Debug.Log("KAI_CHE_R19_PERSPECTIVE_CAMERA_ACTIVE");
            Debug.Log("KAI_CHE_R20_XZ_WORLD_ACTIVE");
        }

        private static void PositionPerspectiveCamera(
            Camera camera,
            Vector2 focus)
        {
            if (camera == null)
                return;

            Vector3 target = RuntimeWorldSpace.ToWorld(
                focus,
                RuntimeWorldSpace.CameraTargetHeight);
            camera.transform.position = new Vector3(
                focus.x + PerspectiveCameraSideOffset,
                PerspectiveCameraHeight,
                focus.y - PerspectiveCameraBackDistance);
            camera.transform.LookAt(target, Vector3.up);
        }

        private void UpdateSurvivalWave(float deltaTime)
        {
            _survivalTime += deltaTime;

            int currentWave = Wave;
            if (currentWave > _lastWaveLog)
            {
                _lastWaveLog = currentWave;
                Debug.Log("KAI_CHE_WAVE_START_" + currentWave);
                LogActiveBaseSkillLoadout();
            }
        }

        private bool IsBaseSkillActive(int skillIndex)
        {
            if (skillIndex < 0
                || skillIndex >= RuntimeContentCatalog.BaseSkills.Length)
                return false;

            return IsBaseSkillOwned(
                RuntimeContentCatalog.BaseSkills[skillIndex].Id);
        }

        private bool IsBaseSkillOwned(string skillId)
        {
            return !string.IsNullOrEmpty(skillId)
                && _activeBaseSkillIds.Contains(skillId);
        }

        private int GetBaseSkillUpgradeRank(string skillId)
        {
            return _baseSkillUpgradeRanks.TryGetValue(
                skillId,
                out int rank)
                ? rank
                : 0;
        }

        private void EnsureBaseSkillRankEntry(string skillId)
        {
            if (!string.IsNullOrEmpty(skillId)
                && !_baseSkillUpgradeRanks.ContainsKey(skillId))
            {
                _baseSkillUpgradeRanks[skillId] = 0;
            }
        }

        private void InitializeBaseSkillLoadout()
        {
            _activeBaseSkillIds.Clear();
            _baseSkillUpgradeRanks.Clear();

            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
                EnsureBaseSkillRankEntry(RuntimeContentCatalog.BaseSkills[i].Id);

            _activeBaseSkillIds.Add(ContentIds.StarterSkill);
            _activeBaseSkillIds.Add(ContentIds.IceBall);

            int saveVersion = PlayerPrefs.GetInt(
                ProgressionSavePrefix + "SkillState.Version",
                0);

            if (saveVersion >= SkillProgressionSaveVersion)
            {
                for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
                {
                    SkillDefinition skill = RuntimeContentCatalog.BaseSkills[i];
                    int rank = Mathf.Max(
                        0,
                        PlayerPrefs.GetInt(
                            ProgressionSavePrefix
                                + "Skill.Rank."
                                + skill.Id,
                            0));
                    _baseSkillUpgradeRanks[skill.Id] = rank;

                    if (_activeBaseSkillIds.Count >= MaxActiveBaseSkills)
                        continue;

                    if (PlayerPrefs.GetInt(
                            ProgressionSavePrefix
                                + "Skill.Owned."
                                + skill.Id,
                            0) > 0)
                    {
                        _activeBaseSkillIds.Add(skill.Id);
                    }
                }

                Debug.Log(
                    "KAI_CHE_R24_SKILL_STATE_LOADED_COUNT_"
                    + ActiveBaseSkillCount
                    + "_"
                    + BuildActiveBaseSkillProgressSummary());
            }
            else
            {
                Debug.Log("KAI_CHE_R24_SKILL_STATE_DEFAULT_FIRE_ICE");
            }

            _fireUpgradeRank =
                GetBaseSkillUpgradeRank(ContentIds.StarterSkill);
            _iceUpgradeRank =
                GetBaseSkillUpgradeRank(ContentIds.IceBall);
            _arcaneUpgradeRank = 0;

            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
            {
                SkillDefinition skill = RuntimeContentCatalog.BaseSkills[i];
                if (skill.Id == ContentIds.StarterSkill
                    || skill.Id == ContentIds.IceBall)
                    continue;

                _arcaneUpgradeRank += GetBaseSkillUpgradeRank(skill.Id);
            }

            if (saveVersion < SkillProgressionSaveVersion)
            {
                SavePrototypeProgression();
                Debug.Log("KAI_CHE_R24_SKILL_STATE_MIGRATED");
            }

            if (DoesPersistedSkillStateMatchRuntime())
                Debug.Log("KAI_CHE_R24_SKILL_STATE_RESTORE_PASS");
            else
                Debug.LogError("KAI_CHE_R24_SKILL_STATE_RESTORE_MISMATCH");

            Debug.Log("KAI_CHE_R24_SKILL_PERSISTENCE_READY");
        }

        private bool DoesPersistedSkillStateMatchRuntime()
        {
            if (PlayerPrefs.GetInt(
                    ProgressionSavePrefix + "SkillState.Version",
                    0) < SkillProgressionSaveVersion)
                return false;

            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
            {
                SkillDefinition skill = RuntimeContentCatalog.BaseSkills[i];
                bool savedOwned = PlayerPrefs.GetInt(
                    ProgressionSavePrefix
                        + "Skill.Owned."
                        + skill.Id,
                    0) > 0;
                int savedRank = Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(
                        ProgressionSavePrefix
                            + "Skill.Rank."
                            + skill.Id,
                        0));

                if (savedOwned != IsBaseSkillOwned(skill.Id))
                    return false;
                if (savedRank != GetBaseSkillUpgradeRank(skill.Id))
                    return false;
            }

            return true;
        }

        private string BuildActiveBaseSkillSummary()
        {
            var names = new List<string>();
            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
            {
                if (IsBaseSkillActive(i))
                    names.Add(RuntimeContentCatalog.BaseSkills[i].DisplayName);
            }

            return string.Join(" · ", names.ToArray());
        }

        private string BuildActiveBaseSkillProgressSummary()
        {
            var names = new List<string>();
            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
            {
                SkillDefinition skill = RuntimeContentCatalog.BaseSkills[i];
                if (!IsBaseSkillOwned(skill.Id))
                    continue;

                names.Add(
                    skill.DisplayName
                    + " Lv."
                    + (GetBaseSkillUpgradeRank(skill.Id) + 1));
            }

            return string.Join(" · ", names.ToArray());
        }

        private void LogActiveBaseSkillLoadout()
        {
            if (_lastActiveLoadoutWave == Wave)
                return;

            _lastActiveLoadoutWave = Wave;
            Debug.Log(
                "KAI_CHE_R23_LOADOUT_WAVE_"
                + Wave
                + "_COUNT_"
                + ActiveBaseSkillCount);
            Debug.Log(
                "KAI_CHE_R23_LOADOUT_NAMES_"
                + BuildActiveBaseSkillSummary());
        }

        private SkillDefinition GetSkillChoiceDefinition(int choiceIndex)
        {
            if (choiceIndex < 0
                || choiceIndex >= _skillChoiceCandidateIds.Length)
                return null;

            string skillId = _skillChoiceCandidateIds[choiceIndex];
            return string.IsNullOrEmpty(skillId)
                ? null
                : RuntimeContentCatalog.FindSkill(skillId);
        }

        private void ClearSkillChoiceCandidates()
        {
            for (int i = 0; i < _skillChoiceCandidateIds.Length; i++)
                _skillChoiceCandidateIds[i] = null;
        }

        private bool ContainsSkillChoiceCandidate(string skillId)
        {
            for (int i = 0; i < _skillChoiceCandidateIds.Length; i++)
            {
                if (_skillChoiceCandidateIds[i] == skillId)
                    return true;
            }

            return false;
        }

        private void GenerateSkillChoiceCandidates()
        {
            ClearSkillChoiceCandidates();

            var owned = new List<SkillDefinition>();
            var unowned = new List<SkillDefinition>();
            var eligible = new List<SkillDefinition>();

            bool canAcquireNew =
                _activeBaseSkillIds.Count < MaxActiveBaseSkills;

            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
            {
                SkillDefinition skill = RuntimeContentCatalog.BaseSkills[i];
                if (IsBaseSkillOwned(skill.Id))
                {
                    owned.Add(skill);
                    eligible.Add(skill);
                }
                else if (canAcquireNew)
                {
                    unowned.Add(skill);
                    eligible.Add(skill);
                }
            }

            int generation = _skillChoiceGeneration++;
            int slot = 0;

            if (owned.Count > 0)
            {
                SkillDefinition upgrade =
                    owned[(generation + Mathf.Max(0, Level - 1)) % owned.Count];
                _skillChoiceCandidateIds[slot++] = upgrade.Id;
            }

            if (slot < _skillChoiceCandidateIds.Length
                && unowned.Count > 0)
            {
                SkillDefinition acquire =
                    unowned[(generation * 2 + Mathf.Max(0, Level - 1))
                        % unowned.Count];
                if (!ContainsSkillChoiceCandidate(acquire.Id))
                    _skillChoiceCandidateIds[slot++] = acquire.Id;
            }

            int cursor = eligible.Count > 0
                ? (generation * 3 + Mathf.Max(0, Level - 1))
                    % eligible.Count
                : 0;

            for (int attempts = 0;
                slot < _skillChoiceCandidateIds.Length
                    && attempts < eligible.Count * 2;
                attempts++)
            {
                SkillDefinition candidate =
                    eligible[(cursor + attempts) % eligible.Count];

                if (ContainsSkillChoiceCandidate(candidate.Id))
                    continue;

                _skillChoiceCandidateIds[slot++] = candidate.Id;
            }

            if (slot < _skillChoiceCandidateIds.Length)
            {
                for (int i = 0;
                    slot < _skillChoiceCandidateIds.Length
                        && i < owned.Count;
                    i++)
                {
                    SkillDefinition fallback = owned[i];
                    if (ContainsSkillChoiceCandidate(fallback.Id))
                        continue;

                    _skillChoiceCandidateIds[slot++] = fallback.Id;
                }
            }

            Debug.Log(
                "KAI_CHE_R23_CHOICES_"
                + BuildSkillChoiceSummary());
        }

        private string BuildSkillChoiceSummary()
        {
            var names = new List<string>();
            for (int i = 0; i < _skillChoiceCandidateIds.Length; i++)
            {
                SkillDefinition skill = GetSkillChoiceDefinition(i);
                if (skill != null)
                {
                    names.Add(
                        (IsBaseSkillOwned(skill.Id) ? "UP:" : "NEW:")
                        + skill.DisplayName);
                }
            }

            return string.Join("|", names.ToArray());
        }

        private void UpdateCombatMomentum(float deltaTime)
        {
            if (_comboTimer > 0f)
                _comboTimer = Mathf.Max(0f, _comboTimer - deltaTime);

            if (_frenzyTimer > 0f)
            {
                _frenzyTimer = Mathf.Max(0f, _frenzyTimer - deltaTime);
                if (_frenzyTimer <= 0f)
                {
                    _comboCount = 0;
                    _comboTimer = 0f;
                    Debug.Log("KAI_CHE_R21_FRENZY_END");
                }

                return;
            }

            if (_comboTimer <= 0f)
                _comboCount = 0;
        }

        private void RegisterCombatKill()
        {
            _comboCount = _comboTimer > 0f
                ? _comboCount + 1
                : 1;
            _comboTimer = ComboWindow;

            if (_comboCount < FrenzyComboThreshold || IsFrenzyActive)
                return;

            _frenzyTimer = FrenzyDuration;
            _comboTimer = FrenzyDuration;
            float healAmount = MaxPlayerHealth * FrenzyHealFraction;
            PlayerHealth = Mathf.Min(MaxPlayerHealth, PlayerHealth + healAmount);
            MarkAdventureHealthDirty();
            TriggerImpactFeedback(0.14f, false);
            Debug.Log("KAI_CHE_R21_FRENZY_START_" + _comboCount);
            Debug.Log("KAI_CHE_R21_FRENZY_HEAL_" + Mathf.RoundToInt(healAmount));

            if (!_frenzyFirstLogged)
            {
                _frenzyFirstLogged = true;
                Debug.Log("KAI_CHE_R21_FRENZY_FIRST");
            }
        }

        private void UpdateWorldMovement(float deltaTime)
        {
            float speed =
                4.5f
                * (1f
                    + GetAttributeValue(CharacterAttribute.Agility)
                    * AgilityMoveSpeedPerPoint)
                * (IsFrenzyActive ? FrenzyMoveSpeedMultiplier : 1f);
            _playerWorldPosition += _moveInput * speed * deltaTime;

            if (_player != null)
            {
                _player.transform.position =
                    RuntimeWorldSpace.ToWorld(
                        _playerWorldPosition,
                        RuntimeWorldSpace.PlayerRootHeight);
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                _cameraShakeTimer = Mathf.Max(
                    0f,
                    _cameraShakeTimer - deltaTime);

                if (_cameraShakeTimer <= 0f)
                    _cameraShakeMagnitude = 0f;

                Vector2 shakeOffset = Vector2.zero;
                if (_cameraShakeTimer > 0f)
                {
                    float fade = Mathf.Clamp01(
                        _cameraShakeTimer / 0.30f);
                    shakeOffset = Random.insideUnitCircle
                        * _cameraShakeMagnitude
                        * fade;
                }

                PositionPerspectiveCamera(
                    camera,
                    _playerWorldPosition + shakeOffset);
            }
        }

        private void UpdateTownEntrance()
        {
            if ((_playerWorldPosition - TownEntrancePosition).sqrMagnitude
                > TownEntranceRadius * TownEntranceRadius)
                return;

            PrototypeAdventureState.SetHealth(
                PlayerHealth,
                MaxPlayerHealth);
            _adventureHealthSaveDirty = false;
            _adventureHealthSaveTimer = 0f;
            Debug.Log(
                "KAI_CHE_R27_HEALTH_TOWN_CHECKPOINT_"
                + Mathf.RoundToInt(PlayerHealth)
                + "_MAX_"
                + Mathf.RoundToInt(MaxPlayerHealth));
            _moveInput = Vector2.zero;
            _initialized = false;
            SceneManager.LoadScene("Town");
        }

        private void UpdatePlayerAnimation(float deltaTime)
        {
            if (_player == null) return;

            bool moving = _moveInput.sqrMagnitude > 0.01f;
            if (_playerVisualAnimator != null)
                _playerVisualAnimator.SetMove(
                    moving ? Mathf.Clamp01(_moveInput.magnitude) : 0f);
            if (moving)
                _moveAnimationTime += deltaTime * 9f;
            else
                _moveAnimationTime = Mathf.MoveTowards(
                    _moveAnimationTime,
                    0f,
                    deltaTime * 12f);

            float stride = moving ? Mathf.Sin(_moveAnimationTime) : 0f;
            float bob = moving ? Mathf.Abs(stride) * 0.028f : 0f;
            float sway = moving ? stride * 1.8f : 0f;
            float scalePulse = moving ? Mathf.Abs(stride) * 0.009f : 0f;

            _player.transform.position =
                RuntimeWorldSpace.ToWorld(
                    _playerWorldPosition,
                    RuntimeWorldSpace.PlayerRootHeight + bob);
            _player.transform.localScale =
                Vector3.one * (PlayerBaseScale + scalePulse);
            _player.transform.rotation =
                Quaternion.Euler(0f, 0f, sway);

            _attackAnimationTime = Mathf.Max(
                0f,
                _attackAnimationTime - deltaTime);

            if (_weaponRoot != null)
            {
                float attackProgress = _attackAnimationTime > 0f
                    ? 1f - (_attackAnimationTime / AttackAnimationDuration)
                    : 0f;
                float swing = _attackAnimationTime > 0f
                    ? Mathf.Sin(attackProgress * Mathf.PI) * -62f
                    : 0f;

                _weaponRoot.localRotation =
                    Quaternion.Euler(0f, 0f, swing);
                _weaponRoot.localPosition =
                    new Vector3(0f, bob * 0.2f, 0f);
            }
        }

        private void UpdateSpawning(float deltaTime)
        {
            _spawnTimer -= deltaTime;
            if (_spawnTimer > 0f || _enemies.Count >= MaxVisibleEnemies) return;

            if (IsBossWave && _lastBossWaveSpawned != Wave)
            {
                _lastBossWaveSpawned = Wave;
                SpawnEnemy(RuntimeContentCatalog.BossMonster);
                _spawnTimer = 1.20f;

                if (!_bossSpawnLogged)
                {
                    _bossSpawnLogged = true;
                    Debug.Log("KAI_CHE_BOSS_SPAWN_FIRST");
                }

                Debug.Log("KAI_CHE_BOSS_SPAWN_WAVE_" + Wave);
                return;
            }

            SpawnEnemy();

            float longTermPressure =
                Mathf.Clamp01(_survivalTime / 180f);
            float interval =
                Mathf.Lerp(0.95f, 0.34f, longTermPressure);

            if (IsPressureWave)
                interval *= 0.60f;

            _spawnTimer = Mathf.Max(0.20f, interval);
        }

        private MonsterDefinition SelectMonsterDefinition()
        {
            _spawnSequence += 1;

            if (IsPressureWave && _spawnSequence % 6 == 0)
            {
                if (!_bruteSpawnLogged)
                {
                    _bruteSpawnLogged = true;
                    Debug.Log("KAI_CHE_BRUTE_MONSTER_SPAWN");
                }

                return RuntimeContentCatalog.BruteMonster;
            }

            if (IsPressureWave && _spawnSequence % 2 == 0)
            {
                if (!_runnerSpawnLogged)
                {
                    _runnerSpawnLogged = true;
                    Debug.Log("KAI_CHE_RUNNER_MONSTER_SPAWN");
                }

                return RuntimeContentCatalog.RunnerMonster;
            }

            if (_spawnSequence % 11 == 0)
            {
                if (!_bruteSpawnLogged)
                {
                    _bruteSpawnLogged = true;
                    Debug.Log("KAI_CHE_BRUTE_MONSTER_SPAWN");
                }

                return RuntimeContentCatalog.BruteMonster;
            }

            if (_spawnSequence % 4 == 0)
            {
                if (!_runnerSpawnLogged)
                {
                    _runnerSpawnLogged = true;
                    Debug.Log("KAI_CHE_RUNNER_MONSTER_SPAWN");
                }

                return RuntimeContentCatalog.RunnerMonster;
            }

            return RuntimeContentCatalog.StarterMonster;
        }

        private static float GetMonsterScale(string monsterId)
        {
            if (monsterId == ContentIds.RunnerMonster)
                return Random.Range(0.38f, 0.46f);

            if (monsterId == ContentIds.BruteMonster)
                return Random.Range(0.64f, 0.74f);

            if (monsterId == ContentIds.BossMonster)
                return Random.Range(1.04f, 1.14f);

            return Random.Range(0.46f, 0.58f);
        }

        private void SpawnEnemy(MonsterDefinition forcedDefinition = null)
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            Vector2 center = PlayerPosition;
            float halfHeight;
            float halfWidth;
            if (camera.orthographic)
            {
                halfHeight = camera.orthographicSize + 0.8f;
                halfWidth = camera.orthographicSize * camera.aspect + 0.8f;
            }
            else
            {
                Vector3 focus = RuntimeWorldSpace.ToWorld(
                    center,
                    RuntimeWorldSpace.CameraTargetHeight);
                float distance = Vector3.Distance(
                    camera.transform.position,
                    focus);
                halfHeight =
                    distance
                    * Mathf.Tan(
                        camera.fieldOfView
                        * 0.5f
                        * Mathf.Deg2Rad)
                    + 0.8f;
                halfWidth = halfHeight * camera.aspect + 0.8f;
            }
            Vector2 spawn;
            int edge = Random.Range(0, 4);
            if (edge == 0)
                spawn = center + new Vector2(-halfWidth, Random.Range(-halfHeight, halfHeight));
            else if (edge == 1)
                spawn = center + new Vector2(halfWidth, Random.Range(-halfHeight, halfHeight));
            else if (edge == 2)
                spawn = center + new Vector2(Random.Range(-halfWidth, halfWidth), -halfHeight);
            else
                spawn = center + new Vector2(Random.Range(-halfWidth, halfWidth), halfHeight);

            MonsterDefinition definition =
                forcedDefinition ?? SelectMonsterDefinition();
            float variantScale = GetMonsterScale(definition.Id);
            bool isBoss = definition.Id == ContentIds.BossMonster;
            bool isElite =
                !isBoss
                && forcedDefinition == null
                && Wave >= 2
                && _spawnSequence > 0
                && _spawnSequence % EliteSpawnInterval == 0;

            int eliteKind = isElite
                ? (_spawnSequence / EliteSpawnInterval) % 3
                : -1;
            float eliteHealthMultiplier = 1f;
            float eliteMoveMultiplier = 1f;
            float eliteDamageMultiplier = 1f;
            int eliteExperienceMultiplier = 1;

            if (isElite)
            {
                if (eliteKind == 0)
                {
                    eliteHealthMultiplier = EliteSwiftHealthMultiplier;
                    eliteMoveMultiplier = EliteSwiftMoveMultiplier;
                    eliteDamageMultiplier = EliteSwiftDamageMultiplier;
                }
                else if (eliteKind == 1)
                {
                    eliteHealthMultiplier = EliteBulwarkHealthMultiplier;
                    eliteMoveMultiplier = EliteBulwarkMoveMultiplier;
                    eliteDamageMultiplier = EliteBulwarkDamageMultiplier;
                }
                else
                {
                    eliteHealthMultiplier = EliteFrenzyHealthMultiplier;
                    eliteMoveMultiplier = EliteFrenzyMoveMultiplier;
                    eliteDamageMultiplier = EliteFrenzyDamageMultiplier;
                }

                eliteExperienceMultiplier = 3;
            }

            float healthScale = isBoss
                ? 1f + Mathf.Max(0, Wave - BossWaveInterval)
                    * BossHealthScalePerWave
                : eliteHealthMultiplier;
            float maxHealth = definition.MaxHealth * healthScale;
            var enemy = new EnemyState
            {
                View = CreateEnemyView(
                    definition,
                    RuntimeWorldSpace.ToWorld(
                        spawn,
                        RuntimeWorldSpace.EnemyRootHeight)),
                Definition = definition,
                Health = maxHealth,
                MaxHealth = maxHealth,
                AnimationPhase = Random.Range(0f, Mathf.PI * 2f),
                BaseScale = variantScale * (isElite ? 1.12f : 1f),
                IsAggro = true,
                IsElite = isElite,
                EliteKind = eliteKind,
                MoveMultiplier = eliteMoveMultiplier,
                DamageMultiplier = eliteDamageMultiplier,
                ExperienceMultiplier = eliteExperienceMultiplier,
                AttackTimer = Random.Range(0.15f, EnemyAttackInterval),
                SpecialTimer = Random.Range(0.6f, 1.2f)
            };

            if (enemy.View != null)
            {
                enemy.View.transform.localScale =
                    Vector3.one * enemy.BaseScale;

                if (isElite)
                    CreateEliteAura(enemy);
            }

            if (isElite && !_eliteSpawnLogged)
            {
                _eliteSpawnLogged = true;
                Debug.Log("KAI_CHE_ELITE_SPAWN_FIRST");
            }

            if (isElite)
                Debug.Log("KAI_CHE_ELITE_KIND_" + eliteKind);

            _enemies.Add(enemy);
        }

        private void UpdateEnemies(float deltaTime)
        {
            Vector2 playerPosition = PlayerPosition;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyState enemy = _enemies[i];
                if (enemy.View == null)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                MonsterDefinition definition =
                    enemy.Definition ?? RuntimeContentCatalog.StarterMonster;
                bool isRunner = definition.Id == ContentIds.RunnerMonster;
                bool isBrute = definition.Id == ContentIds.BruteMonster;
                bool isBoss = definition.Id == ContentIds.BossMonster;
                float bossHealthNormalized = isBoss
                    ? enemy.Health / Mathf.Max(1f, enemy.MaxHealth)
                    : 1f;

                if (isBoss
                    && !enemy.PhaseTwoTriggered
                    && bossHealthNormalized <= BossPhaseTwoHealthThreshold)
                {
                    enemy.PhaseTwoTriggered = true;
                    enemy.SpecialArmed = false;
                    enemy.SpecialTimer = 0.35f;
                    SpawnEnemy(RuntimeContentCatalog.RunnerMonster);
                    SpawnEnemy(RuntimeContentCatalog.RunnerMonster);
                    SpawnEnemy(RuntimeContentCatalog.BruteMonster);
                    CreateBossPhaseTwoEffect(
                        RuntimeWorldSpace.ToLogical(enemy.View.transform.position));
                    TriggerImpactFeedback(0.24f, true);
                    Debug.Log("KAI_CHE_BOSS_PHASE_TWO");
                }

                enemy.SlowTimer = Mathf.Max(
                    0f,
                    enemy.SlowTimer - deltaTime);
                enemy.AttackTimer = Mathf.Max(
                    0f,
                    enemy.AttackTimer - deltaTime);
                enemy.SpecialTimer = Mathf.Max(
                    0f,
                    enemy.SpecialTimer - deltaTime);
                enemy.BurstTimer = Mathf.Max(
                    0f,
                    enemy.BurstTimer - deltaTime);

                if (enemy.SlowTimer <= 0f && enemy.SlowAura != null)
                {
                    Destroy(enemy.SlowAura);
                    enemy.SlowAura = null;
                }

                Vector2 position = RuntimeWorldSpace.ToLogical(enemy.View.transform.position);
                Vector2 offset = playerPosition - position;
                float distance = offset.magnitude;

                if (isRunner
                    && enemy.SpecialTimer <= 0f
                    && distance > 1.2f
                    && distance < 8f)
                {
                    enemy.BurstTimer = RunnerDashDuration;
                    enemy.SpecialTimer = RunnerDashInterval;
                    CreateRunnerDashEffect(
                        position,
                        offset / distance);

                    if (!_runnerDashLogged)
                    {
                        _runnerDashLogged = true;
                        Debug.Log("KAI_CHE_RUNNER_DASH_FIRST");
                    }
                }

                if (isBoss
                    && !enemy.SpecialArmed
                    && enemy.SpecialTimer <= 0f
                    && distance < GetBossPulseRadius(enemy) + 1.8f)
                {
                    enemy.SpecialArmed = true;
                    enemy.SpecialSequence += 1;
                    enemy.BossUsesLineSpecial =
                        enemy.PhaseTwoTriggered
                        && enemy.SpecialSequence % 2 == 0;
                    enemy.SpecialDirection = distance > 0.001f
                        ? offset / distance
                        : Vector2.down;
                    enemy.BurstTimer = enemy.PhaseTwoTriggered
                        ? BossPhaseTwoTelegraphDuration
                        : BossTelegraphDuration;
                    enemy.SpecialTimer = enemy.PhaseTwoTriggered
                        ? BossPhaseTwoPulseInterval
                        : BossPulseInterval;

                    if (enemy.BossUsesLineSpecial)
                    {
                        CreateBossLineTelegraph(
                            position,
                            enemy.SpecialDirection);
                        Debug.Log("KAI_CHE_BOSS_LINE_TELEGRAPH");
                    }
                    else
                    {
                        CreateBossTelegraphEffect(
                            position,
                            GetBossPulseRadius(enemy),
                            enemy.PhaseTwoTriggered);
                        Debug.Log("KAI_CHE_BOSS_PULSE_TELEGRAPH");
                    }
                }

                if (distance > 0.001f)
                {
                    float slowMultiplier = enemy.SlowTimer > 0f
                        ? IceSlowMultiplier
                        : 1f;
                    float burstMultiplier =
                        isRunner && enemy.BurstTimer > 0f
                            ? RunnerDashSpeedMultiplier
                            : 1f;
                    float bossChargeMultiplier =
                        isBoss && enemy.SpecialArmed
                            ? 0.22f
                            : 1f;
                    float moveSpeed =
                        definition.MoveSpeed
                        * enemy.MoveMultiplier
                        * slowMultiplier
                        * burstMultiplier
                        * bossChargeMultiplier;
                    position += offset / distance * moveSpeed * deltaTime;
                    enemy.View.transform.position =
                        RuntimeWorldSpace.ToWorld(
                                position,
                                RuntimeWorldSpace.EnemyRootHeight);
                }

                enemy.AnimationPhase += deltaTime * 5.2f;
                float enemyPulse =
                    1f + Mathf.Sin(enemy.AnimationPhase) * 0.035f;
                enemy.View.transform.localScale =
                    Vector3.one * enemy.BaseScale * enemyPulse;
                enemy.View.transform.rotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Sin(enemy.AnimationPhase * 0.73f) * 2.5f);

                if (isBoss
                    && enemy.SpecialArmed
                    && enemy.BurstTimer <= 0f)
                {
                    if (enemy.BossUsesLineSpecial)
                    {
                        Vector2 toPlayer = playerPosition - position;
                        float forward = Vector2.Dot(
                            toPlayer,
                            enemy.SpecialDirection);
                        float lateral = Mathf.Abs(
                            enemy.SpecialDirection.x * toPlayer.y
                            - enemy.SpecialDirection.y * toPlayer.x);

                        if (forward >= 0f
                            && forward <= BossLineLength
                            && lateral <= BossLineHalfWidth)
                        {
                            float lineDamage =
                                definition.ContactDamagePerSecond
                                * enemy.DamageMultiplier
                                * BossPulseDamageWindow
                                * BossLineDamageMultiplier;
                            ApplyPlayerHit(lineDamage, playerPosition);
                            TriggerImpactFeedback(0.22f, true);
                        }

                        CreateBossLineBlast(
                            position,
                            enemy.SpecialDirection);

                        if (!_bossLineLogged)
                        {
                            _bossLineLogged = true;
                            Debug.Log("KAI_CHE_BOSS_LINE_FIRST");
                        }
                    }
                    else
                    {
                        float bossPulseRadius = GetBossPulseRadius(enemy);
                        if (distance < bossPulseRadius)
                        {
                            float phaseMultiplier = enemy.PhaseTwoTriggered
                                ? 1.25f
                                : 1f;
                            float pulseDamage =
                                definition.ContactDamagePerSecond
                                * enemy.DamageMultiplier
                                * BossPulseDamageWindow
                                * phaseMultiplier;
                            ApplyPlayerHit(pulseDamage, playerPosition);
                            TriggerImpactFeedback(0.18f, true);
                        }

                        CreateBossPulseEffect(
                            position,
                            bossPulseRadius,
                            enemy.PhaseTwoTriggered);

                        if (!_bossPulseLogged)
                        {
                            _bossPulseLogged = true;
                            Debug.Log("KAI_CHE_BOSS_PULSE_FIRST");
                        }
                    }

                    enemy.SpecialArmed = false;
                    enemy.BossUsesLineSpecial = false;
                }
                else if (isBrute
                    && distance < BruteSlamRadius
                    && enemy.SpecialTimer <= 0f)
                {
                    float slamDamage =
                        definition.ContactDamagePerSecond
                        * enemy.DamageMultiplier
                        * BruteSlamInterval;
                    ApplyPlayerHit(slamDamage, playerPosition);
                    enemy.SpecialTimer = BruteSlamInterval;
                    CreateBruteSlamEffect(position);

                    if (!_bruteSlamLogged)
                    {
                        _bruteSlamLogged = true;
                        Debug.Log("KAI_CHE_BRUTE_SLAM_FIRST");
                    }

                    if (distance > 0.001f)
                    {
                        Vector2 recoilDirection = -offset / distance;
                        position += recoilDirection * BruteSlamKnockback;
                        enemy.View.transform.position =
                            RuntimeWorldSpace.ToWorld(
                                position,
                                RuntimeWorldSpace.EnemyRootHeight);
                    }
                }
                else if (!isBrute
                    && !isBoss
                    && enemy.IsAggro
                    && distance < EnemyContactRadius
                    && enemy.AttackTimer <= 0f)
                {
                    float attackDamage =
                        definition.ContactDamagePerSecond
                        * enemy.DamageMultiplier
                        * EnemyAttackInterval;
                    ApplyPlayerHit(attackDamage, playerPosition);
                    enemy.AttackTimer = EnemyAttackInterval;

                    if (distance > 0.001f)
                    {
                        Vector2 recoilDirection = -offset / distance;
                        position += recoilDirection * EnemyContactKnockback;
                        enemy.View.transform.position =
                            RuntimeWorldSpace.ToWorld(
                                position,
                                RuntimeWorldSpace.EnemyRootHeight);
                    }
                }
            }
        }

        private void CreateEliteAura(EnemyState enemy)
        {
            if (enemy == null || enemy.View == null)
                return;

            Color color = enemy.EliteKind == 0
                ? new Color(0.18f, 1f, 0.78f, 0.72f)
                : enemy.EliteKind == 1
                    ? new Color(1f, 0.72f, 0.12f, 0.76f)
                    : new Color(1f, 0.16f, 0.18f, 0.78f);

            GameObject root = new GameObject("EliteAura");
            root.transform.SetParent(enemy.View.transform, false);
            root.transform.localPosition = new Vector3(0f, 0f, 0.06f);

            for (int i = 0; i < 8; i++)
            {
                GameObject ray = CreateQuad(
                    "EliteAuraRay_" + i,
                    color,
                    Vector3.zero,
                    1f);
                ray.transform.SetParent(root.transform, false);
                float length = enemy.EliteKind == 0
                    ? 1.02f
                    : enemy.EliteKind == 1
                        ? 0.88f
                        : 1.18f;
                float thickness = enemy.EliteKind == 1
                    ? 0.095f
                    : 0.055f;
                ray.transform.localScale =
                    new Vector3(length, thickness, 1f);
                ray.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * 22.5f);
            }

            if (enemy.EliteKind == 1)
            {
                GameObject shield = CreateQuad(
                    "EliteBulwarkMark",
                    new Color(1f, 0.88f, 0.34f, 0.90f),
                    new Vector3(0f, 0.10f, -0.08f),
                    1f);
                shield.transform.SetParent(enemy.View.transform, false);
                shield.transform.localScale =
                    new Vector3(0.86f, 0.22f, 1f);
            }
            else if (enemy.EliteKind == 2)
            {
                GameObject rage = CreateQuad(
                    "EliteRageMark",
                    new Color(1f, 0.08f, 0.08f, 0.94f),
                    new Vector3(0f, 0.58f, -0.08f),
                    1f);
                rage.transform.SetParent(enemy.View.transform, false);
                rage.transform.localScale =
                    new Vector3(0.26f, 0.58f, 1f);
                rage.transform.localRotation =
                    Quaternion.Euler(0f, 0f, 45f);
            }
        }

        private void ApplyPlayerHit(
            float damage,
            Vector2 playerPosition)
        {
            float damageReduction = Mathf.Min(
                0.60f,
                GetAttributeValue(CharacterAttribute.Will)
                * WillDamageReductionPerPoint);
            float finalDamage =
                damage * Mathf.Max(0.40f, 1f - damageReduction);

            bool wasAlive = PlayerHealth > 0f;
            PlayerHealth = Mathf.Max(
                0f,
                PlayerHealth - finalDamage);

            if (wasAlive && PlayerHealth <= 0f)
                PrepareDefeatRecoveryCheckpoint("DEATH");
            else
                MarkAdventureHealthDirty();

            _playerHitFlashTimer = PlayerHitFlashDuration;
            if (_playerVisualAnimator != null)
            {
                _playerVisualAnimator.TriggerHit();
                if (PlayerHealth <= 0f)
                    _playerVisualAnimator.SetDead(true);
            }
            CreatePlayerHitEffect(playerPosition);

            if (!_playerHitFirstLogged)
            {
                _playerHitFirstLogged = true;
                Debug.Log("KAI_CHE_PLAYER_HIT_FIRST");
            }
        }

        private void UpdateShooting(float deltaTime)
        {
            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
            {
                if (!IsBaseSkillActive(i))
                    continue;

                if (_projectiles.Count >= MaxActiveProjectiles)
                    break;

                SkillDefinition skill = RuntimeContentCatalog.BaseSkills[i];
                float timer = _skillShotTimers.TryGetValue(
                    skill.Id,
                    out float currentTimer)
                    ? currentTimer - deltaTime
                    : 0f;

                _skillShotTimers[skill.Id] = timer;
                if (timer > 0f)
                    continue;

                EnemyState target = FindNearestEnemy(GetSkillRange(skill));
                if (target == null)
                {
                    _skillShotTimers[skill.Id] = 0.08f;
                    continue;
                }

                FireBaseSkill(skill, target);
            }
        }

        private void FireBaseSkill(
            SkillDefinition skill,
            EnemyState target)
        {
            Vector2 origin = PlayerPosition;
            Vector2 targetPosition = RuntimeWorldSpace.ToLogical(
                target.View.transform.position);
            Vector2 direction = (targetPosition - origin).normalized;

            bool isFireball = skill.Id == ContentIds.StarterSkill;
            bool isIceBall = skill.Id == ContentIds.IceBall;

            GameObject projectileView = CreateBaseSkillProjectile(
                origin,
                direction,
                skill);

            _projectiles.Add(new ProjectileState
            {
                View = projectileView,
                Target = target,
                Direction = direction,
                Life = 2.4f,
                Skill = skill,
                IsFireball = isFireball,
                IsIceBall = isIceBall
            });

            if (isFireball && !_fireballFirstShotLogged)
            {
                _fireballFirstShotLogged = true;
                Debug.Log("KAI_CHE_FIREBALL_FIRST_SHOT");
            }

            if (isIceBall && !_iceballFirstShotLogged)
            {
                _iceballFirstShotLogged = true;
                Debug.Log("KAI_CHE_ICEBALL_FIRST_SHOT");
            }

            if (_loggedBaseSkillShots.Add(skill.Id))
                Debug.Log("KAI_CHE_R21_SKILL_FIRST_SHOT_" + skill.Id);

            _attackAnimationTime = AttackAnimationDuration;
            if (_playerVisualAnimator != null)
                _playerVisualAnimator.TriggerAttack();

            CreateBaseSkillCastEffect(origin, direction, skill);
            _skillShotTimers[skill.Id] =
                GetSkillCooldown(skill, isIceBall, isFireball);
        }

        private float GetSkillCooldown(
            SkillDefinition skill,
            bool isIceBall,
            bool isFireball)
        {
            int independentRank =
                GetBaseSkillUpgradeRank(skill.Id);

            float reduction =
                GetAttributeValue(CharacterAttribute.Wisdom)
                    * WisdomCooldownPerPoint
                + _bossTempoRewardRank * BossRewardCooldownPerRank;

            if (isFireball)
            {
                reduction +=
                    _fireUpgradeRank * FireCooldownPerRank;
            }
            else if (!isIceBall)
            {
                reduction +=
                    independentRank
                    * IndependentBaseSkillCooldownPerRank;
            }

            float cooldown =
                skill.Cooldown * Mathf.Max(0.42f, 1f - reduction);
            if (IsFrenzyActive)
                cooldown *= FrenzyCooldownMultiplier;

            return Mathf.Max(0.18f, cooldown);
        }

        private float GetSkillRange(SkillDefinition skill)
        {
            return skill.Range
                * (1f
                    + GetAttributeValue(CharacterAttribute.Perception)
                    * PerceptionRangePerPoint);
        }

        private float GetProjectileSpeed(SkillDefinition skill)
        {
            float perceptionMultiplier =
                1f
                + GetAttributeValue(CharacterAttribute.Perception)
                    * PerceptionProjectileSpeedPerPoint;

            bool usesIndependentGrowth =
                skill.Id != ContentIds.StarterSkill
                && skill.Id != ContentIds.IceBall;
            float skillRankMultiplier = usesIndependentGrowth
                ? 1f
                    + GetBaseSkillUpgradeRank(skill.Id)
                        * IndependentBaseSkillProjectileSpeedPerRank
                : 1f;

            return skill.ProjectileSpeed
                * perceptionMultiplier
                * skillRankMultiplier;
        }

        private float GetProjectileDamage(
            SkillDefinition skill,
            bool isIceBall,
            bool isFireball)
        {
            float buildMultiplier = isIceBall
                ? 1f + _iceUpgradeRank * IceDamagePerRank
                : isFireball
                    ? 1f + _fireUpgradeRank * FireDamagePerRank
                    : 1f
                        + GetBaseSkillUpgradeRank(skill.Id)
                            * IndependentBaseSkillDamagePerRank;

            float levelMultiplier =
                1f
                + Mathf.Min(Mathf.Max(0, Level - 1), 25)
                    * LevelDamagePerLevel;

            float intelligenceMultiplier =
                1f
                + GetAttributeValue(CharacterAttribute.Intelligence)
                    * IntelligenceDamagePerPoint;

            float elementMultiplier = isIceBall
                ? 1f
                    + GetAttributeValue(CharacterAttribute.Good)
                        * GoodIceDamagePerPoint
                : isFireball
                    ? 1f
                        + GetAttributeValue(CharacterAttribute.Evil)
                            * EvilFireDamagePerPoint
                    : 1f;

            float bossRewardMultiplier =
                1f + _bossAttackRewardRank * BossRewardDamagePerRank;

            float damage =
                skill.Damage
                * buildMultiplier
                * levelMultiplier
                * intelligenceMultiplier
                * elementMultiplier
                * bossRewardMultiplier;

            if (IsFrenzyActive)
                damage *= FrenzyDamageMultiplier;

            return damage;
        }

        private GameObject CreateBaseSkillProjectile(
            Vector2 origin,
            Vector2 direction,
            SkillDefinition skill)
        {
            if (skill.Id == ContentIds.StarterSkill)
                return CreateFireballProjectile(origin, direction, skill.Id);

            if (skill.Id == ContentIds.IceBall)
                return CreateIceballProjectile(origin, direction, skill.Id);

            Color mainColor = GetBaseSkillColor(skill.Id);
            var root = new GameObject("BaseSkill_" + skill.Id);
            root.transform.SetParent(transform, false);
            root.transform.position = RuntimeWorldSpace.ToWorld(
                origin,
                RuntimeWorldSpace.ProjectileHeight);
            root.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            GameObject glow = CreateQuad(
                "SkillGlow",
                new Color(mainColor.r, mainColor.g, mainColor.b, 0.36f),
                Vector3.zero,
                1f);
            glow.transform.SetParent(root.transform, false);
            glow.transform.localScale = new Vector3(0.70f, 0.70f, 1f);

            GameObject core = CreateQuad(
                "SkillCore",
                mainColor,
                Vector3.zero,
                1f);
            core.transform.SetParent(root.transform, false);
            core.transform.localScale = new Vector3(0.42f, 0.30f, 1f);

            GameObject trail = CreateQuad(
                "SkillTrail",
                new Color(mainColor.r, mainColor.g, mainColor.b, 0.58f),
                new Vector3(-0.34f, 0f, 0.02f),
                1f);
            trail.transform.SetParent(root.transform, false);
            trail.transform.localScale = new Vector3(0.50f, 0.10f, 1f);

            return root;
        }

        private void CreateBaseSkillCastEffect(
            Vector2 origin,
            Vector2 direction,
            SkillDefinition skill)
        {
            if (skill.Id == ContentIds.StarterSkill)
            {
                CreateFireballCastEffect(origin, direction);
                return;
            }

            if (skill.Id == ContentIds.IceBall)
            {
                CreateIceballCastEffect(origin, direction);
                return;
            }

            Color color = GetBaseSkillColor(skill.Id);
            var root = new GameObject("BaseSkillCast_" + skill.Id);
            root.transform.SetParent(transform, false);
            root.transform.position = RuntimeWorldSpace.ToWorld(
                origin + direction * 0.38f,
                RuntimeWorldSpace.EffectHeight);

            GameObject flash = CreateQuad(
                "BaseSkillCastFlash",
                new Color(color.r, color.g, color.b, 0.88f),
                Vector3.zero,
                1f);
            flash.transform.SetParent(root.transform, false);
            flash.transform.localScale = new Vector3(0.34f, 0.34f, 1f);

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.16f,
                MaxLife = 0.16f,
                StartScale = Vector3.one,
                SpinSpeed = 180f
            });
        }

        private static Color GetBaseSkillColor(string skillId)
        {
            if (skillId == ContentIds.WindBlade)
                return new Color(0.38f, 1f, 0.72f, 0.96f);
            if (skillId == ContentIds.ArcSpark)
                return new Color(0.74f, 0.48f, 1f, 0.98f);
            if (skillId == ContentIds.PoisonOrb)
                return new Color(0.46f, 0.92f, 0.18f, 0.96f);
            if (skillId == ContentIds.StoneShot)
                return new Color(0.64f, 0.52f, 0.34f, 0.98f);
            if (skillId == ContentIds.LightOrb)
                return new Color(1f, 0.92f, 0.46f, 0.98f);
            if (skillId == ContentIds.DarkOrb)
                return new Color(0.46f, 0.12f, 0.66f, 0.98f);
            if (skillId == ContentIds.WaterOrb)
                return new Color(0.12f, 0.62f, 1f, 0.96f);
            if (skillId == ContentIds.MagicArrow)
                return new Color(0.92f, 0.92f, 1f, 0.98f);

            return Color.white;
        }

        private GameObject CreateFireballProjectile(
            Vector2 origin,
            Vector2 direction,
            string skillId)
        {
            var root = new GameObject("Fireball_" + skillId);
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                    origin,
                    RuntimeWorldSpace.ProjectileHeight);
            root.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            GameObject outerGlow = CreateQuad(
                "FireballGlow",
                new Color(1f, 0.20f, 0.02f, 0.38f),
                Vector3.zero,
                1f);
            outerGlow.transform.SetParent(root.transform, false);
            outerGlow.transform.localScale =
                new Vector3(0.82f, 0.82f, 1f);

            GameObject flame = CreateQuad(
                "FireballFlame",
                new Color(1f, 0.38f, 0.035f, 0.96f),
                Vector3.zero,
                1f);
            flame.transform.SetParent(root.transform, false);
            flame.transform.localScale =
                new Vector3(0.58f, 0.58f, 1f);
            flame.transform.localRotation =
                Quaternion.Euler(0f, 0f, 45f);

            GameObject core = CreateQuad(
                "FireballCore",
                new Color(1f, 0.88f, 0.28f, 1f),
                new Vector3(0.05f, 0f, -0.02f),
                1f);
            core.transform.SetParent(root.transform, false);
            core.transform.localScale =
                new Vector3(0.34f, 0.34f, 1f);

            GameObject tailOuter = CreateQuad(
                "FireballTailOuter",
                new Color(0.96f, 0.16f, 0.015f, 0.72f),
                new Vector3(-0.45f, 0f, 0.03f),
                1f);
            tailOuter.transform.SetParent(root.transform, false);
            tailOuter.transform.localScale =
                new Vector3(0.68f, 0.22f, 1f);

            GameObject tailInner = CreateQuad(
                "FireballTailInner",
                new Color(1f, 0.64f, 0.08f, 0.86f),
                new Vector3(-0.32f, 0f, 0.01f),
                1f);
            tailInner.transform.SetParent(root.transform, false);
            tailInner.transform.localScale =
                new Vector3(0.48f, 0.10f, 1f);

            return root;
        }

        private GameObject CreateIceballProjectile(
            Vector2 origin,
            Vector2 direction,
            string skillId)
        {
            var root = new GameObject("IceBall_" + skillId);
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                    origin,
                    RuntimeWorldSpace.ProjectileHeight);
            root.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            GameObject outerGlow = CreateQuad(
                "IceBallGlow",
                new Color(0.22f, 0.78f, 1f, 0.40f),
                Vector3.zero,
                1f);
            outerGlow.transform.SetParent(root.transform, false);
            outerGlow.transform.localScale =
                new Vector3(0.78f, 0.78f, 1f);

            GameObject shard = CreateQuad(
                "IceBallShard",
                new Color(0.30f, 0.84f, 1f, 0.98f),
                Vector3.zero,
                1f);
            shard.transform.SetParent(root.transform, false);
            shard.transform.localScale =
                new Vector3(0.62f, 0.42f, 1f);
            shard.transform.localRotation =
                Quaternion.Euler(0f, 0f, 45f);

            GameObject core = CreateQuad(
                "IceBallCore",
                new Color(0.88f, 0.98f, 1f, 1f),
                new Vector3(0.05f, 0f, -0.02f),
                1f);
            core.transform.SetParent(root.transform, false);
            core.transform.localScale =
                new Vector3(0.28f, 0.28f, 1f);

            GameObject trailOuter = CreateQuad(
                "IceBallTrailOuter",
                new Color(0.12f, 0.55f, 1f, 0.56f),
                new Vector3(-0.42f, 0f, 0.03f),
                1f);
            trailOuter.transform.SetParent(root.transform, false);
            trailOuter.transform.localScale =
                new Vector3(0.62f, 0.18f, 1f);

            GameObject trailInner = CreateQuad(
                "IceBallTrailInner",
                new Color(0.72f, 0.94f, 1f, 0.82f),
                new Vector3(-0.28f, 0f, 0.01f),
                1f);
            trailInner.transform.SetParent(root.transform, false);
            trailInner.transform.localScale =
                new Vector3(0.42f, 0.08f, 1f);

            return root;
        }

        private EnemyState FindNearestEnemy(float range)
        {
            EnemyState nearest = null;
            float bestDistanceSquared = range * range;
            Vector2 origin = PlayerPosition;            for (int i = 0; i < _enemies.Count; i++)
            {
                EnemyState enemy = _enemies[i];
                if (enemy.View == null) continue;

                Vector2 enemyPosition = RuntimeWorldSpace.ToLogical(enemy.View.transform.position);
                float distanceSquared = (enemyPosition - origin).sqrMagnitude;
                if (distanceSquared >= bestDistanceSquared) continue;

                bestDistanceSquared = distanceSquared;
                nearest = enemy;
            }

            return nearest;
        }

        private void UpdateProjectiles(float deltaTime)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                ProjectileState projectile = _projectiles[i];
                if (projectile.View == null)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                SkillDefinition skill =
                    projectile.Skill ?? RuntimeContentCatalog.StarterSkill;

                if (projectile.Target != null && projectile.Target.View != null)
                {
                    Vector2 current = RuntimeWorldSpace.ToLogical(
                        projectile.View.transform.position);
                    Vector2 target = RuntimeWorldSpace.ToLogical(
                        projectile.Target.View.transform.position);
                    Vector2 offset = target - current;
                    if (offset.sqrMagnitude > 0.0001f)
                        projectile.Direction = offset.normalized;
                }

                Vector2 position = RuntimeWorldSpace.ToLogical(
                    projectile.View.transform.position);
                position +=
                    projectile.Direction * GetProjectileSpeed(skill) * deltaTime;
                projectile.View.transform.position =
                    RuntimeWorldSpace.ToWorld(
                        position,
                        RuntimeWorldSpace.ProjectileHeight);
                projectile.View.transform.rotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Atan2(
                        projectile.Direction.y,
                        projectile.Direction.x) * Mathf.Rad2Deg);
                projectile.Life -= deltaTime;

                bool hit = projectile.Target != null
                    && projectile.Target.View != null
                    && (RuntimeWorldSpace.ToLogical(
                            projectile.Target.View.transform.position)
                        - position).sqrMagnitude < 0.16f;

                if (hit)
                {
                    RecordSkillMasteryUse(skill.Id);

                    if (projectile.IsIceBall)
                    {
                        projectile.Target.SlowTimer = Mathf.Max(
                            projectile.Target.SlowTimer,
                            IceSlowDuration
                            + _iceUpgradeRank * IceSlowDurationPerRank);
                        EnsureIceSlowAura(projectile.Target);
                        CreateIceImpactEffect(position);

                        if (_iceUpgradeRank >= IceEvolutionRank)
                            ApplyIceEvolutionPulse(projectile.Target, position);
                    }
                    else
                    {
                        CreateImpactEffect(position);
                    }

                    float hitDamage = GetProjectileDamage(
                        skill,
                        projectile.IsIceBall,
                        projectile.IsFireball);

                    _projectileHitSequence += 1;
                    bool arcaneChain =
                        IsArcaneChainUnlocked
                        && _projectileHitSequence % ArcaneChainHitInterval == 0;

                    bool thermalShock =
                        projectile.IsFireball
                        && IsThermalShockUnlocked
                        && projectile.Target.SlowTimer > 0f;

                    if (thermalShock)
                    {
                        hitDamage *= 1f + ThermalShockBonusMultiplier;
                        ApplyThermalShock(
                            projectile.Target,
                            position,
                            hitDamage * ThermalShockSplashMultiplier);
                    }

                    TriggerHitStop(
                        projectile.Target,
                        hitDamage);
                    DamageEnemy(projectile.Target, hitDamage);

                    if (arcaneChain)
                    {
                        ApplyArcaneChain(
                            projectile.Target,
                            position,
                            hitDamage * ArcaneChainDamageMultiplier,
                            projectile.IsIceBall);
                    }

                    if (projectile.IsFireball
                        && _fireUpgradeRank >= FireEvolutionRank)
                    {
                        ApplyFireEvolutionSplash(
                            projectile.Target,
                            position,
                            hitDamage * FireSplashDamageMultiplier);
                    }
                    Destroy(projectile.View);
                    _projectiles.RemoveAt(i);
                }
                else if (projectile.Life <= 0f)
                {
                    Destroy(projectile.View);
                    _projectiles.RemoveAt(i);
                }
            }
        }

        private void ApplyArcaneChain(
            EnemyState primaryTarget,
            Vector2 center,
            float chainDamage,
            bool isIceBall)
        {
            float radiusSq = ArcaneChainRadius * ArcaneChainRadius;
            EnemyState nearest = null;
            float nearestDistanceSq = radiusSq;

            for (int i = 0; i < _enemies.Count; i++)
            {
                EnemyState enemy = _enemies[i];
                if (enemy == null
                    || enemy == primaryTarget
                    || enemy.View == null)
                    continue;

                Vector2 enemyPosition = RuntimeWorldSpace.ToLogical(enemy.View.transform.position);
                float distanceSq = (enemyPosition - center).sqrMagnitude;
                if (distanceSq >= nearestDistanceSq)
                    continue;

                nearest = enemy;
                nearestDistanceSq = distanceSq;
            }

            if (nearest == null)
                return;

            Vector2 targetPosition = RuntimeWorldSpace.ToLogical(nearest.View.transform.position);
            CreateArcaneChainEffect(center, targetPosition);

            if (isIceBall)
            {
                nearest.SlowTimer = Mathf.Max(
                    nearest.SlowTimer,
                    IceSlowDuration + 0.6f);
                EnsureIceSlowAura(nearest);
            }

            DamageEnemy(nearest, chainDamage);
            TriggerImpactFeedback(0.08f, false);

            if (!_arcaneChainFirstLogged)
            {
                _arcaneChainFirstLogged = true;
                Debug.Log("KAI_CHE_ARCANE_CHAIN_FIRST");
            }
        }

        private void ApplyThermalShock(
            EnemyState primaryTarget,
            Vector2 center,
            float splashDamage)
        {
            float radiusSq = ThermalShockRadius * ThermalShockRadius;
            int hits = 0;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyState enemy = _enemies[i];
                if (enemy == null
                    || enemy == primaryTarget
                    || enemy.View == null)
                    continue;

                Vector2 enemyPosition = RuntimeWorldSpace.ToLogical(enemy.View.transform.position);
                if ((enemyPosition - center).sqrMagnitude > radiusSq)
                    continue;

                DamageEnemy(enemy, splashDamage);
                hits += 1;
            }

            CreateThermalShockEffect(center);
            TriggerImpactFeedback(0.12f, false);

            if (!_thermalShockFirstLogged)
            {
                _thermalShockFirstLogged = true;
                Debug.Log("KAI_CHE_THERMAL_SHOCK_FIRST");
            }

            if (hits > 0)
                Debug.Log("KAI_CHE_THERMAL_SHOCK_SPLASH_" + hits);
        }

        private void ApplyFireEvolutionSplash(
            EnemyState primaryTarget,
            Vector2 center,
            float splashDamage)
        {
            float radiusSq = FireSplashRadius * FireSplashRadius;
            int hits = 0;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyState enemy = _enemies[i];
                if (enemy == null
                    || enemy == primaryTarget
                    || enemy.View == null)
                    continue;

                Vector2 enemyPosition = RuntimeWorldSpace.ToLogical(enemy.View.transform.position);
                if ((enemyPosition - center).sqrMagnitude > radiusSq)
                    continue;

                DamageEnemy(enemy, splashDamage);
                hits += 1;
            }

            if (hits > 0)
                Debug.Log("KAI_CHE_FIRE_EVOLUTION_SPLASH_" + hits);

            CreateImpactEffect(center);
        }

        private void ApplyIceEvolutionPulse(
            EnemyState primaryTarget,
            Vector2 center)
        {
            float radiusSq = IcePulseRadius * IcePulseRadius;
            int affected = 0;
            float pulseSlow = IceSlowDuration
                + _iceUpgradeRank * IceSlowDurationPerRank
                + 0.8f;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyState enemy = _enemies[i];
                if (enemy == null
                    || enemy == primaryTarget
                    || enemy.View == null)
                    continue;

                Vector2 enemyPosition = RuntimeWorldSpace.ToLogical(enemy.View.transform.position);
                if ((enemyPosition - center).sqrMagnitude > radiusSq)
                    continue;

                enemy.SlowTimer = Mathf.Max(enemy.SlowTimer, pulseSlow);
                EnsureIceSlowAura(enemy);
                affected += 1;
            }

            if (affected > 0)
                Debug.Log("KAI_CHE_ICE_EVOLUTION_PULSE_" + affected);

            CreateIceImpactEffect(center);
        }

        private void UpdateEffects(float deltaTime)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                EffectState effect = _effects[i];
                if (effect.View == null)
                {
                    _effects.RemoveAt(i);
                    continue;
                }

                effect.Life -= deltaTime;
                float t = 1f - Mathf.Clamp01(effect.Life / effect.MaxLife);
                float scale = Mathf.Lerp(1f, 0.08f, t);
                effect.View.transform.localScale =
                    effect.StartScale * scale;
                effect.View.transform.Rotate(
                    0f,
                    0f,
                    effect.SpinSpeed * deltaTime);
                effect.View.transform.position +=
                    effect.Velocity * deltaTime;

                if (effect.Life > 0f) continue;

                Destroy(effect.View);
                _effects.RemoveAt(i);
            }
        }

        private void CreateFireballCastEffect(
            Vector2 origin,
            Vector2 direction)
        {
            var root = new GameObject("FireballCastEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                    origin + direction * 0.42f,
                    RuntimeWorldSpace.EffectHeight);

            GameObject ring = CreateQuad(
                "FireballCastRing",
                new Color(1f, 0.34f, 0.04f, 0.62f),
                Vector3.zero,
                1f);
            ring.transform.SetParent(root.transform, false);
            ring.transform.localScale =
                new Vector3(0.72f, 0.72f, 1f);
            ring.transform.localRotation =
                Quaternion.Euler(0f, 0f, 45f);

            GameObject flash = CreateQuad(
                "FireballCastFlash",
                new Color(1f, 0.88f, 0.32f, 0.92f),
                Vector3.zero,
                1f);
            flash.transform.SetParent(root.transform, false);
            flash.transform.localScale =
                new Vector3(0.28f, 0.28f, 1f);

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.18f,
                MaxLife = 0.18f,
                StartScale = Vector3.one,
                SpinSpeed = 210f
            });
        }

        private void CreateIceballCastEffect(
            Vector2 origin,
            Vector2 direction)
        {
            var root = new GameObject("IceBallCastEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                    origin + direction * 0.42f,
                    RuntimeWorldSpace.EffectHeight);

            GameObject ring = CreateQuad(
                "IceBallCastRing",
                new Color(0.20f, 0.72f, 1f, 0.58f),
                Vector3.zero,
                1f);
            ring.transform.SetParent(root.transform, false);
            ring.transform.localScale =
                new Vector3(0.68f, 0.68f, 1f);
            ring.transform.localRotation =
                Quaternion.Euler(0f, 0f, 45f);

            GameObject flash = CreateQuad(
                "IceBallCastFlash",
                new Color(0.86f, 0.98f, 1f, 0.94f),
                Vector3.zero,
                1f);
            flash.transform.SetParent(root.transform, false);
            flash.transform.localScale =
                new Vector3(0.24f, 0.24f, 1f);

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.20f,
                MaxLife = 0.20f,
                StartScale = Vector3.one,
                SpinSpeed = -180f
            });
        }

        private void EnsureIceSlowAura(EnemyState enemy)
        {
            if (enemy == null
                || enemy.View == null
                || enemy.SlowAura != null)
                return;

            var root = new GameObject("IceSlowAura");
            root.transform.SetParent(enemy.View.transform, false);
            root.transform.localPosition = new Vector3(0f, 0f, -0.08f);

            GameObject horizontal = CreateQuad(
                "IceSlowAuraHorizontal",
                new Color(0.18f, 0.72f, 1f, 0.34f),
                Vector3.zero,
                1f);
            horizontal.transform.SetParent(root.transform, false);
            horizontal.transform.localScale =
                new Vector3(1.75f, 0.12f, 1f);

            GameObject vertical = CreateQuad(
                "IceSlowAuraVertical",
                new Color(0.72f, 0.95f, 1f, 0.28f),
                Vector3.zero,
                1f);
            vertical.transform.SetParent(root.transform, false);
            vertical.transform.localScale =
                new Vector3(0.12f, 1.75f, 1f);

            enemy.SlowAura = root;
        }

        private void CreateRunnerDashEffect(
            Vector2 position,
            Vector2 direction)
        {
            var root = new GameObject("RunnerDashEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);
            root.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            for (int i = 0; i < 3; i++)
            {
                GameObject streak = CreateQuad(
                    "RunnerDashStreak_" + i,
                    i == 0
                        ? new Color(0.36f, 1f, 0.86f, 0.64f)
                        : new Color(0.10f, 0.58f, 0.62f, 0.42f),
                    new Vector3(-0.30f - i * 0.16f, 0f, 0f),
                    1f);
                streak.transform.SetParent(root.transform, false);
                streak.transform.localScale =
                    new Vector3(0.48f - i * 0.08f, 0.08f, 1f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.18f,
                MaxLife = 0.18f,
                StartScale = Vector3.one,
                SpinSpeed = 0f
            });
        }

        private void CreateDamageNumber(
            Vector2 position,
            float damage,
            bool emphasized)
        {
            var root = new GameObject("DamageNumber");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                    position,
                    RuntimeWorldSpace.LabelHeight);

            TextMesh text = root.AddComponent<TextMesh>();
            text.text = Mathf.Max(1, Mathf.RoundToInt(damage)).ToString();
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = emphasized ? 64 : 50;
            text.characterSize = emphasized ? 0.075f : 0.060f;
            text.fontStyle = FontStyle.Bold;
            text.color = emphasized
                ? new Color(1f, 0.82f, 0.22f, 1f)
                : new Color(1f, 0.96f, 0.86f, 0.96f);

            MeshRenderer renderer = root.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sortingOrder = 100;

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.48f,
                MaxLife = 0.48f,
                StartScale = Vector3.one,
                SpinSpeed = 0f,
                Velocity = new Vector3(0f, 1.15f, 0f)
            });
        }

        private void CreateBossLineTelegraph(
            Vector2 position,
            Vector2 direction)
        {
            var root = new GameObject("BossLineTelegraph");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);
            root.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            for (int i = 0; i < 7; i++)
            {
                GameObject segment = CreateQuad(
                    "BossLineTelegraph_" + i,
                    new Color(1f, 0.16f, 0.66f, 0.64f),
                    new Vector3(0.65f + i * 0.95f, 0f, 0f),
                    1f);
                segment.transform.SetParent(root.transform, false);
                segment.transform.localScale =
                    new Vector3(0.72f, 0.16f, 1f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = BossPhaseTwoTelegraphDuration,
                MaxLife = BossPhaseTwoTelegraphDuration,
                StartScale = Vector3.one,
                SpinSpeed = 0f
            });
        }

        private void CreateBossLineBlast(
            Vector2 position,
            Vector2 direction)
        {
            var root = new GameObject("BossLineBlast");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);
            root.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            GameObject outer = CreateQuad(
                "BossLineBlastOuter",
                new Color(0.92f, 0.12f, 1f, 0.72f),
                new Vector3(BossLineLength * 0.5f, 0f, 0f),
                1f);
            outer.transform.SetParent(root.transform, false);
            outer.transform.localScale =
                new Vector3(BossLineLength, BossLineHalfWidth * 1.8f, 1f);

            GameObject core = CreateQuad(
                "BossLineBlastCore",
                new Color(1f, 0.68f, 1f, 0.96f),
                new Vector3(BossLineLength * 0.5f, 0f, -0.01f),
                1f);
            core.transform.SetParent(root.transform, false);
            core.transform.localScale =
                new Vector3(BossLineLength, BossLineHalfWidth * 0.45f, 1f);

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.26f,
                MaxLife = 0.26f,
                StartScale = Vector3.one,
                SpinSpeed = 0f
            });
        }

        private float GetBossPulseRadius(EnemyState enemy)
        {
            return enemy != null && enemy.PhaseTwoTriggered
                ? BossPhaseTwoPulseRadius
                : BossPulseRadius;
        }

        private void TriggerImpactFeedback(
            float magnitude,
            bool vibrate)
        {
            _cameraShakeTimer = Mathf.Max(_cameraShakeTimer, 0.30f);
            _cameraShakeMagnitude = Mathf.Max(
                _cameraShakeMagnitude,
                magnitude);

#if UNITY_ANDROID || UNITY_IOS
            if (vibrate)
                Handheld.Vibrate();
#endif
        }

        private void CreateArcaneChainEffect(
            Vector2 from,
            Vector2 to)
        {
            Vector2 delta = to - from;
            float distance = delta.magnitude;
            if (distance <= 0.001f)
                return;

            var root = new GameObject("ArcaneChainEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                from,
                RuntimeWorldSpace.EffectHeight);
            root.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            GameObject beam = CreateQuad(
                "ArcaneChainBeam",
                new Color(0.62f, 0.28f, 1f, 0.88f),
                new Vector3(distance * 0.5f, 0f, 0f),
                1f);
            beam.transform.SetParent(root.transform, false);
            beam.transform.localScale =
                new Vector3(distance, 0.07f, 1f);

            GameObject core = CreateQuad(
                "ArcaneChainCore",
                new Color(0.92f, 0.78f, 1f, 0.96f),
                new Vector3(distance * 0.5f, 0f, -0.01f),
                1f);
            core.transform.SetParent(root.transform, false);
            core.transform.localScale =
                new Vector3(distance * 0.92f, 0.025f, 1f);

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.16f,
                MaxLife = 0.16f,
                StartScale = Vector3.one,
                SpinSpeed = 0f
            });
        }

        private void CreateThermalShockEffect(Vector2 position)
        {
            var root = new GameObject("ThermalShockEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 12; i++)
            {
                GameObject ray = CreateQuad(
                    "ThermalShockRay_" + i,
                    i % 2 == 0
                        ? new Color(1f, 0.24f, 0.04f, 0.92f)
                        : new Color(0.16f, 0.76f, 1f, 0.88f),
                    Vector3.zero,
                    1f);
                ray.transform.SetParent(root.transform, false);
                ray.transform.localScale =
                    new Vector3(1.15f, 0.08f, 1f);
                ray.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * 15f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.28f,
                MaxLife = 0.28f,
                StartScale = Vector3.one * 1.12f,
                SpinSpeed = 180f
            });
        }

        private void CreateBossPhaseTwoEffect(Vector2 position)
        {
            var root = new GameObject("BossPhaseTwoEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 20; i++)
            {
                GameObject ray = CreateQuad(
                    "BossPhaseTwoRay_" + i,
                    i % 2 == 0
                        ? new Color(1f, 0.14f, 0.88f, 0.94f)
                        : new Color(0.48f, 0.10f, 1f, 0.82f),
                    Vector3.zero,
                    1f);
                ray.transform.SetParent(root.transform, false);
                ray.transform.localScale =
                    new Vector3(1.80f, 0.10f, 1f);
                ray.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * 9f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.48f,
                MaxLife = 0.48f,
                StartScale = Vector3.one * 1.35f,
                SpinSpeed = 145f
            });
        }

        private void CreateBossTelegraphEffect(
            Vector2 position,
            float radius,
            bool phaseTwo)
        {
            var root = new GameObject("BossPulseTelegraph");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f;
                float radians = angle * Mathf.Deg2Rad;
                Vector3 localPosition = new Vector3(
                    Mathf.Cos(radians) * radius * 0.62f,
                    Mathf.Sin(radians) * radius * 0.62f,
                    0f);

                GameObject segment = CreateQuad(
                    "BossTelegraph_" + i,
                    phaseTwo
                        ? new Color(1f, 0.16f, 0.74f, 0.68f)
                        : new Color(0.72f, 0.20f, 1f, 0.58f),
                    localPosition,
                    1f);
                segment.transform.SetParent(root.transform, false);
                segment.transform.localScale =
                    new Vector3(0.56f, 0.075f, 1f);
                segment.transform.localRotation =
                    Quaternion.Euler(0f, 0f, angle);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = phaseTwo
                    ? BossPhaseTwoTelegraphDuration
                    : BossTelegraphDuration,
                MaxLife = phaseTwo
                    ? BossPhaseTwoTelegraphDuration
                    : BossTelegraphDuration,
                StartScale = Vector3.one,
                SpinSpeed = -32f
            });
        }

        private void CreateBossPulseEffect(
            Vector2 position,
            float radius,
            bool phaseTwo)
        {
            var root = new GameObject("BossPulseImpact");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 16; i++)
            {
                GameObject ray = CreateQuad(
                    "BossPulseRay_" + i,
                    i % 2 == 0
                        ? (phaseTwo
                            ? new Color(1f, 0.14f, 0.80f, 0.96f)
                            : new Color(0.94f, 0.34f, 1f, 0.92f))
                        : (phaseTwo
                            ? new Color(0.54f, 0.08f, 0.92f, 0.82f)
                            : new Color(0.38f, 0.12f, 0.72f, 0.76f)),
                    Vector3.zero,
                    1f);
                ray.transform.SetParent(root.transform, false);
                ray.transform.localScale =
                    new Vector3(radius * 0.70f, 0.09f, 1f);
                ray.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * 11.25f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.34f,
                MaxLife = 0.34f,
                StartScale = Vector3.one * 1.18f,
                SpinSpeed = 110f
            });
        }

        private void CreateBruteSlamEffect(Vector2 position)
        {
            var root = new GameObject("BruteSlamEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 8; i++)
            {
                GameObject ray = CreateQuad(
                    "BruteSlamRay_" + i,
                    i % 2 == 0
                        ? new Color(1f, 0.64f, 0.12f, 0.88f)
                        : new Color(0.62f, 0.38f, 0.10f, 0.70f),
                    Vector3.zero,
                    1f);
                ray.transform.SetParent(root.transform, false);
                ray.transform.localScale =
                    new Vector3(0.76f, 0.08f, 1f);
                ray.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * 22.5f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.28f,
                MaxLife = 0.28f,
                StartScale = Vector3.one,
                SpinSpeed = 95f
            });
        }

        private void CreatePlayerHitEffect(Vector2 position)
        {
            var root = new GameObject("PlayerHitEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 4; i++)
            {
                GameObject slash = CreateQuad(
                    "PlayerHitSlash_" + i,
                    i % 2 == 0
                        ? new Color(1f, 0.18f, 0.10f, 0.92f)
                        : new Color(1f, 0.66f, 0.20f, 0.84f),
                    Vector3.zero,
                    1f);
                slash.transform.SetParent(root.transform, false);
                slash.transform.localScale =
                    new Vector3(0.54f, 0.07f, 1f);
                slash.transform.localRotation =
                    Quaternion.Euler(0f, 0f, 45f + i * 45f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = PlayerHitFlashDuration,
                MaxLife = PlayerHitFlashDuration,
                StartScale = Vector3.one,
                SpinSpeed = 180f
            });
        }

        private void CreateIceImpactEffect(Vector3 position)
        {
            var root = new GameObject("IceImpactEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 6; i++)
            {
                GameObject shard = CreateQuad(
                    "IceImpactShard_" + i,
                    i % 2 == 0
                        ? new Color(0.72f, 0.95f, 1f, 0.96f)
                        : new Color(0.16f, 0.62f, 1f, 0.90f),
                    Vector3.zero,
                    1f);
                shard.transform.SetParent(root.transform, false);
                shard.transform.localScale =
                    new Vector3(0.48f, 0.06f, 1f);
                shard.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * 30f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.24f,
                MaxLife = 0.24f,
                StartScale = Vector3.one,
                SpinSpeed = -220f
            });
        }

        private void CreateImpactEffect(Vector3 position)
        {
            var root = new GameObject("ImpactEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 4; i++)
            {
                GameObject ray = CreateQuad(
                    "ImpactRay_" + i,
                    i % 2 == 0
                        ? new Color(1f, 0.72f, 0.20f, 0.96f)
                        : new Color(0.92f, 0.26f, 0.08f, 0.90f),
                    Vector3.zero,
                    1f);
                ray.transform.SetParent(root.transform, false);
                ray.transform.localScale =
                    new Vector3(0.42f, 0.07f, 1f);
                ray.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * 45f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.20f,
                MaxLife = 0.20f,
                StartScale = Vector3.one,
                SpinSpeed = 240f
            });
        }

        private void DamageEnemy(EnemyState enemy, float damage)
        {
            if (enemy == null || enemy.View == null) return;

            enemy.Health -= damage;
            if (enemy.Health > 0f) return;

            MonsterDefinition definition =
                enemy.Definition ?? RuntimeContentCatalog.StarterMonster;
            bool isBoss = definition.Id == ContentIds.BossMonster;
            Vector2 defeatedPosition = RuntimeWorldSpace.ToLogical(enemy.View.transform.position);
            int experienceReward =
                definition.ExperienceReward
                * Mathf.Max(1, enemy.ExperienceMultiplier);
            Experience += experienceReward;
            int levelsGained =
                _progression.AddExperienceAndApplyPrototypeLevelUp(
                    experienceReward);
            Kills += 1;
            RegisterCombatKill();

            if (levelsGained > 0)
            {
                bool wasSkillChoicePending = _pendingSkillChoices > 0;
                _pendingSkillChoices += levelsGained;
                if (!wasSkillChoicePending)
                    GenerateSkillChoiceCandidates();

                _moveInput = Vector2.zero;

                float healAmount =
                    MaxPlayerHealth * LevelHealFraction;
                PlayerHealth = Mathf.Min(
                    MaxPlayerHealth,
                    PlayerHealth + healAmount);
                MarkAdventureHealthDirty();

                CreateLevelUpPowerEffect(PlayerPosition);
                TriggerImpactFeedback(0.10f, false);
                Debug.Log(
                    "KAI_CHE_LEVEL_POWER_"
                    + Level
                    + "_DAMAGE_"
                    + Mathf.RoundToInt(LevelDamageBonusPercent));

                if (!_skillChoiceFirstLogged)
                {
                    _skillChoiceFirstLogged = true;
                    Debug.Log("KAI_CHE_SKILL_CHOICE_READY");
                }
            }

            if (isBoss)
            {
                _pendingBossRewards += 1;
                _moveInput = Vector2.zero;
                TriggerBossDefeat(defeatedPosition);
                Debug.Log("KAI_CHE_BOSS_REWARD_READY");
            }

            SavePrototypeProgression();
            if (isBoss)
            {
                Debug.Log(
                    "KAI_CHE_R25_BOSS_REWARD_PENDING_SAVED_"
                    + BuildBossRewardStateSummary());
            }

            Destroy(enemy.View);
            enemy.View = null;
        }

        private void CreateLevelUpPowerEffect(Vector2 position)
        {
            var root = new GameObject("LevelUpPowerEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 16; i++)
            {
                GameObject ray = CreateQuad(
                    "LevelUpRay_" + i,
                    i % 2 == 0
                        ? new Color(1f, 0.86f, 0.28f, 0.94f)
                        : new Color(0.34f, 0.78f, 1f, 0.82f),
                    Vector3.zero,
                    1f);
                ray.transform.SetParent(root.transform, false);
                ray.transform.localScale =
                    new Vector3(1.35f, 0.075f, 1f);
                ray.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * 11.25f);
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.38f,
                MaxLife = 0.38f,
                StartScale = Vector3.one * 1.18f,
                SpinSpeed = 120f
            });
        }

        private void TriggerBossDefeat(Vector2 position)
        {
            CreateBossDefeatEffect(position);
            TriggerImpactFeedback(0.32f, true);

            _slowMotionTimer = BossFinishSlowMotionDuration;
            Time.timeScale = BossFinishSlowMotionScale;

            if (!_bossDefeatLogged)
            {
                _bossDefeatLogged = true;
                Debug.Log("KAI_CHE_BOSS_DEFEATED_FIRST");
            }

            Debug.Log("KAI_CHE_BOSS_DEFEATED_WAVE_" + Wave);
        }

        private void CreateBossDefeatEffect(Vector2 position)
        {
            var root = new GameObject("BossDefeatEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                RuntimeWorldSpace.ToWorld(
                position,
                RuntimeWorldSpace.EffectHeight);

            for (int i = 0; i < 28; i++)
            {
                GameObject shard = CreateQuad(
                    "BossDefeatShard_" + i,
                    i % 3 == 0
                        ? new Color(1f, 0.78f, 0.30f, 0.96f)
                        : i % 3 == 1
                            ? new Color(0.94f, 0.20f, 1f, 0.92f)
                            : new Color(0.34f, 0.14f, 0.82f, 0.86f),
                    Vector3.zero,
                    1f);
                shard.transform.SetParent(root.transform, false);
                shard.transform.localScale =
                    new Vector3(2.2f, 0.09f, 1f);
                shard.transform.localRotation =
                    Quaternion.Euler(0f, 0f, i * (180f / 28f));
            }

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.52f,
                MaxLife = 0.52f,
                StartScale = Vector3.one * 1.45f,
                SpinSpeed = 170f
            });
        }

        private void CreateBattlefieldBackdrop()
        {
            GameObject floor = CreateQuad(
                "BattlefieldFloor",
                new Color(0.092f, 0.084f, 0.064f),
                new Vector3(0f, 0f, 2.5f),
                70f);
            floor.transform.localScale = new Vector3(70f, 70f, 1f);

            GameObject road = CreateQuad(
                "BattlefieldRoad",
                new Color(0.145f, 0.126f, 0.084f),
                new Vector3(0f, 0f, 2.2f),
                1f);
            road.transform.localScale = new Vector3(4.8f, 70f, 1f);

            CreateRoadEdge(-2.52f);
            CreateRoadEdge(2.52f);
            CreateTownEntranceMarker();

            for (int i = -18; i <= 18; i++)
            {
                float y = i * 2.25f;
                float width = i % 3 == 0 ? 4.35f : 3.85f;
                GameObject slab = CreateQuad(
                    "RoadSlab_" + i,
                    i % 2 == 0
                        ? new Color(0.178f, 0.150f, 0.096f, 0.55f)
                        : new Color(0.120f, 0.105f, 0.076f, 0.38f),
                    new Vector3(0f, y, 2.08f),
                    1f);
                slab.transform.localScale = new Vector3(width, 0.055f, 1f);
            }

            for (int i = 0; i < 42; i++)
            {
                float x = Mathf.Sin(i * 2.17f) * 28f;
                float y = Mathf.Cos(i * 1.61f) * 28f;
                float size = 0.16f + (i % 5) * 0.05f;
                GameObject stone = CreateQuad(
                    "BattlefieldStone_" + i,
                    new Color(0.19f, 0.17f, 0.13f, 0.94f),
                    new Vector3(x, y, 2.0f),
                    size);
                stone.transform.localScale =
                    new Vector3(1.35f, 0.72f, 1f) * size;
                stone.transform.rotation =
                    Quaternion.Euler(0f, 0f, (i * 37f) % 360f);
            }
        }

        private void CreateTownEntranceMarker()
        {
            GameObject glow = CreateQuad(
                "TownEntrance",
                new Color(0.24f, 0.46f, 0.22f, 0.52f),
                RuntimeWorldSpace.ToWorld(
                    TownEntrancePosition,
                    RuntimeWorldSpace.EffectHeight),
                1f);
            glow.transform.localScale = new Vector3(1.35f, 0.72f, 1f);

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject post = CreateQuad(
                    side < 0 ? "TownGateLeft" : "TownGateRight",
                    new Color(0.31f, 0.24f, 0.13f),
                    RuntimeWorldSpace.ToWorld(
                        new Vector2(side * 0.95f, TownEntrancePosition.y),
                        0.90f),
                    1f);
                post.transform.localScale = new Vector3(0.22f, 1.75f, 1f);
            }
        }

        private void CreateRoadEdge(float x)
        {
            GameObject edge = CreateQuad(
                x < 0f ? "RoadEdgeLeft" : "RoadEdgeRight",
                new Color(0.28f, 0.22f, 0.12f),
                new Vector3(x, 0f, 2.1f),
                1f);
            edge.transform.localScale = new Vector3(0.18f, 70f, 1f);
        }

        private GameObject CreateEnemyView(
            MonsterDefinition definition,
            Vector3 position)
        {
            GameObject runtime3D = RuntimeCombatVisualFactory.CreateEnemy(
                transform,
                _materialTemplate,
                definition,
                position);
            if (runtime3D != null)
                return runtime3D;

            string id = definition.Id;
            bool isRunner = id == ContentIds.RunnerMonster;
            bool isBrute = id == ContentIds.BruteMonster;
            bool isBoss = id == ContentIds.BossMonster;

            Color limbColor = isRunner
                ? new Color(0.08f, 0.18f, 0.20f)
                : isBrute
                    ? new Color(0.20f, 0.16f, 0.12f)
                    : new Color(0.19f, 0.045f, 0.038f);
            Color bodyColor = isRunner
                ? new Color(0.10f, 0.34f, 0.38f)
                : isBrute
                    ? new Color(0.31f, 0.25f, 0.17f)
                    : new Color(0.30f, 0.075f, 0.060f);
            Color chestColor = isRunner
                ? new Color(0.18f, 0.58f, 0.60f)
                : isBrute
                    ? new Color(0.49f, 0.39f, 0.22f)
                    : new Color(0.50f, 0.12f, 0.075f);
            Color headColor = isRunner
                ? new Color(0.08f, 0.14f, 0.15f)
                : isBrute
                    ? new Color(0.18f, 0.17f, 0.14f)
                    : new Color(0.12f, 0.12f, 0.11f);
            Color hornColor = isRunner
                ? new Color(0.36f, 0.78f, 0.78f)
                : isBrute
                    ? new Color(0.72f, 0.62f, 0.36f)
                    : new Color(0.60f, 0.46f, 0.25f);
            Color eyeColor = isRunner
                ? new Color(0.30f, 1f, 0.86f)
                : isBrute
                    ? new Color(1f, 0.76f, 0.10f)
                    : new Color(1f, 0.34f, 0.06f);

            if (isBoss)
            {
                limbColor = new Color(0.16f, 0.06f, 0.24f);
                bodyColor = new Color(0.27f, 0.08f, 0.40f);
                chestColor = new Color(0.55f, 0.16f, 0.72f);
                headColor = new Color(0.12f, 0.035f, 0.20f);
                hornColor = new Color(0.88f, 0.52f, 1f);
                eyeColor = new Color(1f, 0.42f, 0.96f);
            }

            var root = new GameObject("Enemy_" + id);
            root.transform.SetParent(transform, false);
            root.transform.position = position;

            GameObject shadow = CreateQuad(
                "EnemyShadow",
                new Color(0.015f, 0.010f, 0.008f, 0.55f),
                new Vector3(0f, -0.38f, 0.08f),
                1f);
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localScale = new Vector3(0.92f, 0.28f, 1f);

            GameObject leftLeg = CreateQuad(
                "EnemyLegLeft",
                limbColor,
                new Vector3(-0.18f, -0.48f, 0.01f),
                0.30f);
            leftLeg.transform.SetParent(root.transform, false);
            leftLeg.transform.localScale = new Vector3(0.22f, 0.52f, 1f);

            GameObject rightLeg = CreateQuad(
                "EnemyLegRight",
                limbColor,
                new Vector3(0.18f, -0.48f, 0.01f),
                0.30f);
            rightLeg.transform.SetParent(root.transform, false);
            rightLeg.transform.localScale = new Vector3(0.22f, 0.52f, 1f);

            GameObject body = CreateQuad(
                "EnemyBody",
                bodyColor,
                Vector3.zero,
                0.72f);
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.66f, 0.82f, 1f);

            GameObject chest = CreateQuad(
                "EnemyChest",
                chestColor,
                new Vector3(0f, -0.03f, -0.02f),
                0.42f);
            chest.transform.SetParent(root.transform, false);
            chest.transform.localScale = new Vector3(0.48f, 0.48f, 1f);

            if (isRunner)
            {
                GameObject runnerMark = CreateQuad(
                    "EnemyRunnerMark",
                    new Color(0.54f, 1f, 0.88f, 0.88f),
                    new Vector3(0f, 0.10f, -0.045f),
                    0.24f);
                runnerMark.transform.SetParent(root.transform, false);
                runnerMark.transform.localScale =
                    new Vector3(0.10f, 0.62f, 1f);
            }
            else if (isBrute)
            {
                GameObject armorPlate = CreateQuad(
                    "EnemyBruteArmor",
                    new Color(0.58f, 0.48f, 0.28f),
                    new Vector3(0f, 0.02f, -0.05f),
                    0.56f);
                armorPlate.transform.SetParent(root.transform, false);
                armorPlate.transform.localScale =
                    new Vector3(0.78f, 0.34f, 1f);
            }
            else if (isBoss)
            {
                GameObject core = CreateQuad(
                    "EnemyBossCore",
                    new Color(0.92f, 0.26f, 1f, 0.96f),
                    new Vector3(0f, 0.02f, -0.06f),
                    0.58f);
                core.transform.SetParent(root.transform, false);
                core.transform.localScale =
                    new Vector3(0.62f, 0.62f, 1f);
                core.transform.localRotation =
                    Quaternion.Euler(0f, 0f, 45f);

                GameObject crown = CreateQuad(
                    "EnemyBossCrown",
                    new Color(0.82f, 0.46f, 1f, 0.94f),
                    new Vector3(0f, 0.70f, -0.08f),
                    0.42f);
                crown.transform.SetParent(root.transform, false);
                crown.transform.localScale =
                    new Vector3(0.86f, 0.18f, 1f);
            }

            GameObject leftArm = CreateQuad(
                "EnemyArmLeft",
                limbColor,
                new Vector3(-0.43f, 0.02f, -0.01f),
                0.28f);
            leftArm.transform.SetParent(root.transform, false);
            leftArm.transform.localScale = new Vector3(0.22f, 0.52f, 1f);
            leftArm.transform.localRotation = Quaternion.Euler(0f, 0f, 16f);

            GameObject rightArm = CreateQuad(
                "EnemyArmRight",
                limbColor,
                new Vector3(0.43f, 0.02f, -0.01f),
                0.28f);
            rightArm.transform.SetParent(root.transform, false);
            rightArm.transform.localScale = new Vector3(0.22f, 0.52f, 1f);
            rightArm.transform.localRotation = Quaternion.Euler(0f, 0f, -16f);

            GameObject head = CreateQuad(
                "EnemyHead",
                headColor,
                new Vector3(0f, 0.43f, -0.035f),
                0.36f);
            head.transform.SetParent(root.transform, false);
            head.transform.localScale = new Vector3(0.40f, 0.34f, 1f);

            GameObject hornLeft = CreateQuad(
                "EnemyHornLeft",
                hornColor,
                new Vector3(-0.20f, 0.68f, -0.04f),
                0.22f);
            hornLeft.transform.SetParent(root.transform, false);
            hornLeft.transform.localScale = new Vector3(0.08f, 0.30f, 1f);
            hornLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 28f);

            GameObject hornRight = CreateQuad(
                "EnemyHornRight",
                hornColor,
                new Vector3(0.20f, 0.68f, -0.04f),
                0.22f);
            hornRight.transform.SetParent(root.transform, false);
            hornRight.transform.localScale = new Vector3(0.08f, 0.30f, 1f);
            hornRight.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);

            GameObject leftEye = CreateQuad(
                "EnemyEyeLeft",
                eyeColor,
                new Vector3(-0.10f, 0.44f, -0.06f),
                0.075f);
            leftEye.transform.SetParent(root.transform, false);

            GameObject rightEye = CreateQuad(
                "EnemyEyeRight",
                eyeColor,
                new Vector3(0.10f, 0.44f, -0.06f),
                0.075f);
            rightEye.transform.SetParent(root.transform, false);

            return root;
        }

        private GameObject CreatePlayerView()
        {
            GameObject runtime3D = RuntimeCombatVisualFactory.CreateNoviceHuman(
                transform,
                _materialTemplate,
                PlayerBaseScale,
                out Transform runtimeWeaponRoot);
            if (runtime3D != null)
            {
                _weaponRoot = runtimeWeaponRoot;
                RuntimeCombatVisualSlot visualSlot =
                    runtime3D.GetComponent<RuntimeCombatVisualSlot>();
                _playerVisualAnimator =
                    visualSlot != null
                        ? visualSlot.AnimatorBridge
                        : null;
                return runtime3D;
            }

            var root = new GameObject("Player");
            root.transform.SetParent(transform, false);
            root.transform.position = Vector3.zero;
            root.transform.localScale = Vector3.one * PlayerBaseScale;

            GameObject shadow = CreateQuad(
                "PlayerShadow",
                new Color(0.015f, 0.012f, 0.010f, 0.62f),
                new Vector3(0f, -0.42f, 0.08f),
                1f);
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localScale = new Vector3(1.28f, 0.42f, 1f);

            GameObject cape = CreateQuad(
                "PlayerCape",
                new Color(0.26f, 0.07f, 0.045f),
                new Vector3(0.02f, -0.08f, 0.035f),
                0.82f);
            cape.transform.SetParent(root.transform, false);
            cape.transform.localScale = new Vector3(0.72f, 0.92f, 1f);

            GameObject leftLeg = CreateQuad(
                "PlayerLegLeft",
                new Color(0.14f, 0.16f, 0.17f),
                new Vector3(-0.20f, -0.55f, 0.015f),
                0.26f);
            leftLeg.transform.SetParent(root.transform, false);
            leftLeg.transform.localScale = new Vector3(0.23f, 0.56f, 1f);

            GameObject rightLeg = CreateQuad(
                "PlayerLegRight",
                new Color(0.14f, 0.16f, 0.17f),
                new Vector3(0.20f, -0.55f, 0.015f),
                0.26f);
            rightLeg.transform.SetParent(root.transform, false);
            rightLeg.transform.localScale = new Vector3(0.23f, 0.56f, 1f);

            GameObject body = CreateQuad(
                "PlayerBody",
                new Color(0.20f, 0.23f, 0.25f),
                new Vector3(0f, -0.02f, 0f),
                0.92f);
            body.transform.SetParent(root.transform, false);

            GameObject chest = CreateQuad(
                "PlayerChest",
                new Color(0.58f, 0.43f, 0.17f),
                new Vector3(0f, 0.00f, -0.03f),
                0.54f);
            chest.transform.SetParent(root.transform, false);
            chest.transform.localScale = new Vector3(0.62f, 0.58f, 1f);

            GameObject chestLight = CreateQuad(
                "PlayerChestLight",
                new Color(0.76f, 0.58f, 0.22f),
                new Vector3(0f, 0.13f, -0.045f),
                0.22f);
            chestLight.transform.SetParent(root.transform, false);
            chestLight.transform.localScale = new Vector3(0.42f, 0.08f, 1f);

            GameObject belt = CreateQuad(
                "PlayerBelt",
                new Color(0.11f, 0.075f, 0.045f),
                new Vector3(0f, -0.31f, -0.04f),
                0.38f);
            belt.transform.SetParent(root.transform, false);
            belt.transform.localScale = new Vector3(0.62f, 0.10f, 1f);

            GameObject helmet = CreateQuad(
                "PlayerHelmet",
                new Color(0.34f, 0.36f, 0.35f),
                new Vector3(0f, 0.50f, -0.04f),
                0.42f);
            helmet.transform.SetParent(root.transform, false);
            helmet.transform.localScale = new Vector3(0.46f, 0.40f, 1f);

            GameObject helmetRidge = CreateQuad(
                "PlayerHelmetRidge",
                new Color(0.66f, 0.51f, 0.23f),
                new Vector3(0f, 0.72f, -0.055f),
                0.18f);
            helmetRidge.transform.SetParent(root.transform, false);
            helmetRidge.transform.localScale = new Vector3(0.09f, 0.30f, 1f);

            GameObject visor = CreateQuad(
                "PlayerVisor",
                new Color(0.10f, 0.12f, 0.13f),
                new Vector3(0f, 0.51f, -0.06f),
                0.24f);
            visor.transform.SetParent(root.transform, false);
            visor.transform.localScale = new Vector3(0.34f, 0.08f, 1f);

            GameObject leftShoulder = CreateQuad(
                "PlayerShoulderLeft",
                new Color(0.38f, 0.31f, 0.20f),
                new Vector3(-0.43f, 0.22f, -0.035f),
                0.30f);
            leftShoulder.transform.SetParent(root.transform, false);

            GameObject rightShoulder = CreateQuad(
                "PlayerShoulderRight",
                new Color(0.38f, 0.31f, 0.20f),
                new Vector3(0.43f, 0.22f, -0.035f),
                0.30f);
            rightShoulder.transform.SetParent(root.transform, false);

            GameObject shieldRim = CreateQuad(
                "PlayerShieldRim",
                new Color(0.57f, 0.43f, 0.19f),
                new Vector3(-0.60f, -0.04f, -0.055f),
                0.66f);
            shieldRim.transform.SetParent(root.transform, false);
            shieldRim.transform.localScale = new Vector3(0.56f, 0.72f, 1f);

            GameObject shield = CreateQuad(
                "PlayerShield",
                new Color(0.13f, 0.15f, 0.16f),
                new Vector3(-0.60f, -0.04f, -0.065f),
                0.58f);
            shield.transform.SetParent(root.transform, false);
            shield.transform.localScale = new Vector3(0.48f, 0.64f, 1f);

            GameObject shieldBoss = CreateQuad(
                "PlayerShieldBoss",
                new Color(0.62f, 0.47f, 0.19f),
                new Vector3(-0.58f, -0.03f, -0.075f),
                0.20f);
            shieldBoss.transform.SetParent(root.transform, false);

            var weapon = new GameObject("PlayerWeapon");
            weapon.transform.SetParent(root.transform, false);
            _weaponRoot = weapon.transform;

            GameObject sword = CreateQuad(
                "PlayerSword",
                new Color(0.84f, 0.83f, 0.72f),
                new Vector3(0.67f, 0.10f, -0.05f),
                0.16f);
            sword.transform.SetParent(_weaponRoot, false);
            sword.transform.localScale = new Vector3(0.11f, 1.16f, 1f);
            sword.transform.rotation = Quaternion.Euler(0f, 0f, -24f);

            GameObject swordGuard = CreateQuad(
                "PlayerSwordGuard",
                new Color(0.64f, 0.46f, 0.17f),
                new Vector3(0.47f, -0.36f, -0.065f),
                0.28f);
            swordGuard.transform.SetParent(_weaponRoot, false);
            swordGuard.transform.localScale = new Vector3(0.42f, 0.09f, 1f);
            swordGuard.transform.rotation = Quaternion.Euler(0f, 0f, -24f);

            GameObject swordGrip = CreateQuad(
                "PlayerSwordGrip",
                new Color(0.12f, 0.075f, 0.04f),
                new Vector3(0.39f, -0.52f, -0.055f),
                0.20f);
            swordGrip.transform.SetParent(_weaponRoot, false);
            swordGrip.transform.localScale = new Vector3(0.10f, 0.34f, 1f);
            swordGrip.transform.rotation = Quaternion.Euler(0f, 0f, -24f);

            return root;
        }

        private GameObject CreateQuad(
            string objectName,
            Color color,
            Vector3 position,
            float scale)
        {
            var view = GameObject.CreatePrimitive(PrimitiveType.Quad);
            view.name = objectName;
            view.transform.SetParent(transform, false);
            view.transform.position = position;
            view.transform.localScale = Vector3.one * scale;

            Collider collider = view.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);            Renderer renderer = view.GetComponent<Renderer>();
            if (renderer != null && _materialTemplate != null)
                renderer.sharedMaterial = GetSharedRuntimeMaterial(color);

            return view;
        }

        private Material GetSharedRuntimeMaterial(Color color)
        {
            Color32 packed = color;
            int key =
                packed.r
                | (packed.g << 8)
                | (packed.b << 16)
                | (packed.a << 24);

            if (_runtimeMaterialCache.TryGetValue(key, out Material cached)
                && cached != null)
                return cached;

            var material = new Material(_materialTemplate);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else
                material.color = color;

            _runtimeMaterialCache[key] = material;
            return material;
        }

        private void LoadPrototypeProgression()
        {
            string levelKey = ProgressionSavePrefix + "Level";
            if (PlayerPrefs.HasKey(levelKey))
            {
                int[] attributes = new int[8];
                foreach (CharacterAttribute attribute in
                         System.Enum.GetValues(typeof(CharacterAttribute)))
                {
                    attributes[(int)attribute] = PlayerPrefs.GetInt(
                        ProgressionSavePrefix + "Attribute." + attribute,
                        0);
                }

                long experience = ParseSavedLong("Experience");
                long levelExperience = ParseSavedLong("LevelExperience");

                _progression.RestorePrototypeState(
                    PlayerPrefs.GetInt(levelKey, 1),
                    experience,
                    levelExperience,
                    PlayerPrefs.GetInt(
                        ProgressionSavePrefix + "UnspentAttributePoints",
                        0),
                    attributes);
            }

            LoadSkillMasteryProgression();
            LoadBossRewardProgression();
        }

        private void LoadSkillMasteryProgression()
        {
            int saveVersion = PlayerPrefs.GetInt(
                ProgressionSavePrefix + "SkillMasteryState.Version",
                0);

            _skillMasteryStateNeedsMigration =
                saveVersion < SkillMasterySaveVersion;

            if (_skillMasteryStateNeedsMigration)
            {
                _skillMastery.RebuildUnlocksFromMastery();
                Debug.Log("KAI_CHE_R26_SKILL_MASTERY_STATE_DEFAULT");
                return;
            }

            for (int i = 0; i < SkillProgressionCatalog.All.Length; i++)
            {
                SkillProgressionDefinition definition =
                    SkillProgressionCatalog.All[i];

                int mastery = Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(
                        ProgressionSavePrefix
                            + "SkillMastery."
                            + definition.Id,
                        0));

                _skillMastery.RestoreMastery(
                    definition.Id,
                    mastery);
            }

            _skillMastery.RebuildUnlocksFromMastery();
            _skillMastery.RestoreLastUnlockedSkillId(
                PlayerPrefs.GetString(
                    ProgressionSavePrefix
                        + "SkillMastery.LastUnlockedSkillId",
                    string.Empty));

            Debug.Log(
                "KAI_CHE_R26_SKILL_MASTERY_STATE_LOADED_"
                + BuildSkillMasteryStateSummary());
        }

        private void LoadBossRewardProgression()
        {
            int saveVersion = PlayerPrefs.GetInt(
                ProgressionSavePrefix + "BossRewardState.Version",
                0);

            _bossRewardStateNeedsMigration =
                saveVersion < BossRewardSaveVersion;

            if (_bossRewardStateNeedsMigration)
            {
                _pendingBossRewards = 0;
                _bossAttackRewardRank = 0;
                _bossVitalityRewardRank = 0;
                _bossTempoRewardRank = 0;
                Debug.Log("KAI_CHE_R25_BOSS_REWARD_STATE_DEFAULT");
                return;
            }

            _pendingBossRewards = Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    ProgressionSavePrefix + "PendingBossRewards",
                    0));
            _bossAttackRewardRank = Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    ProgressionSavePrefix + "BossReward.AttackRank",
                    0));
            _bossVitalityRewardRank = Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    ProgressionSavePrefix + "BossReward.VitalityRank",
                    0));
            _bossTempoRewardRank = Mathf.Max(
                0,
                PlayerPrefs.GetInt(
                    ProgressionSavePrefix + "BossReward.TempoRank",
                    0));

            Debug.Log(
                "KAI_CHE_R25_BOSS_REWARD_STATE_LOADED_"
                + BuildBossRewardStateSummary());
        }

        private void SavePrototypeProgression()
        {
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "Level",
                _progression.Level);
            PlayerPrefs.SetString(
                ProgressionSavePrefix + "Experience",
                _progression.Experience.ToString());
            PlayerPrefs.SetString(
                ProgressionSavePrefix + "LevelExperience",
                _progression.LevelExperience.ToString());
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "UnspentAttributePoints",
                _progression.UnspentAttributePoints);

            foreach (CharacterAttribute attribute in
                     System.Enum.GetValues(typeof(CharacterAttribute)))
            {
                PlayerPrefs.SetInt(
                    ProgressionSavePrefix + "Attribute." + attribute,
                    _progression.GetAttribute(attribute));
            }

            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "SkillState.Version",
                SkillProgressionSaveVersion);
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "PendingSkillChoices",
                Mathf.Max(0, _pendingSkillChoices));
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "SkillChoiceGeneration",
                Mathf.Max(0, _skillChoiceGeneration));

            for (int i = 0; i < RuntimeContentCatalog.BaseSkills.Length; i++)
            {
                SkillDefinition skill = RuntimeContentCatalog.BaseSkills[i];
                PlayerPrefs.SetInt(
                    ProgressionSavePrefix
                        + "Skill.Owned."
                        + skill.Id,
                    IsBaseSkillOwned(skill.Id) ? 1 : 0);
                PlayerPrefs.SetInt(
                    ProgressionSavePrefix
                        + "Skill.Rank."
                        + skill.Id,
                    GetBaseSkillUpgradeRank(skill.Id));
            }

            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "BossRewardState.Version",
                BossRewardSaveVersion);
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "PendingBossRewards",
                Mathf.Max(0, _pendingBossRewards));
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "BossReward.AttackRank",
                Mathf.Max(0, _bossAttackRewardRank));
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "BossReward.VitalityRank",
                Mathf.Max(0, _bossVitalityRewardRank));
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "BossReward.TempoRank",
                Mathf.Max(0, _bossTempoRewardRank));

            SaveSkillMasterySnapshotToPlayerPrefs();
            PlayerPrefs.Save();
            _skillMasterySaveDirty = false;
            _skillMasterySaveTimer = 0f;
        }

        private void SaveSkillMasterySnapshotToPlayerPrefs()
        {
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "SkillMasteryState.Version",
                SkillMasterySaveVersion);

            for (int i = 0; i < SkillProgressionCatalog.All.Length; i++)
            {
                SkillProgressionDefinition definition =
                    SkillProgressionCatalog.All[i];
                PlayerPrefs.SetInt(
                    ProgressionSavePrefix
                        + "SkillMastery."
                        + definition.Id,
                    Mathf.Max(
                        0,
                        _skillMastery.GetMastery(definition.Id)));
            }

            PlayerPrefs.SetString(
                ProgressionSavePrefix
                    + "SkillMastery.LastUnlockedSkillId",
                _skillMastery.LastUnlockedSkillId ?? string.Empty);
        }

        private void RecordSkillMasteryUse(string skillId)
        {
            int previousMastery = _skillMastery.GetMastery(skillId);
            int previousUnlockedCount = _skillMastery.UnlockedSkillCount;

            _skillMastery.RecordUse(skillId);

            int currentMastery = _skillMastery.GetMastery(skillId);
            if (currentMastery == previousMastery)
                return;

            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "SkillMasteryState.Version",
                SkillMasterySaveVersion);
            PlayerPrefs.SetInt(
                ProgressionSavePrefix + "SkillMastery." + skillId,
                currentMastery);
            PlayerPrefs.SetString(
                ProgressionSavePrefix
                    + "SkillMastery.LastUnlockedSkillId",
                _skillMastery.LastUnlockedSkillId ?? string.Empty);

            if (!_skillMasterySaveDirty)
                _skillMasterySaveTimer = SkillMasterySaveInterval;
            _skillMasterySaveDirty = true;

            if (_skillMastery.UnlockedSkillCount > previousUnlockedCount)
            {
                FlushSkillMasteryPersistence();
                Debug.Log(
                    "KAI_CHE_R26_SKILL_MASTERY_UNLOCKED_"
                    + (_skillMastery.LastUnlockedSkillId
                        ?? "UNKNOWN"));
            }
        }

        private void PrepareDefeatRecoveryCheckpoint(string reason)
        {
            PrototypeAdventureState.PrepareCombatRetry(MaxPlayerHealth);
            _adventureHealthSaveDirty = false;
            _adventureHealthSaveTimer = 0f;

            Debug.Log(
                "KAI_CHE_R28_DEFEAT_RECOVERY_CHECKPOINT_"
                + reason
                + "_MAX_"
                + Mathf.RoundToInt(MaxPlayerHealth));
        }

        private void MarkAdventureHealthDirty()
        {
            if (!_adventureHealthSaveDirty)
                _adventureHealthSaveTimer = AdventureHealthSaveInterval;

            _adventureHealthSaveDirty = true;
        }

        private void UpdateAdventureHealthPersistence(float unscaledDeltaTime)
        {
            if (!_adventureHealthSaveDirty)
                return;

            _adventureHealthSaveTimer -= Mathf.Max(0f, unscaledDeltaTime);
            if (_adventureHealthSaveTimer <= 0f)
                FlushAdventureHealthPersistence();
        }

        private void FlushAdventureHealthPersistence()
        {
            if (!_adventureHealthSaveDirty)
                return;

            PrototypeAdventureState.SetHealth(
                PlayerHealth,
                MaxPlayerHealth);
            _adventureHealthSaveDirty = false;
            _adventureHealthSaveTimer = 0f;

            Debug.Log(
                "KAI_CHE_R27_HEALTH_PERSISTED_CURRENT_"
                + Mathf.RoundToInt(PlayerHealth)
                + "_MAX_"
                + Mathf.RoundToInt(MaxPlayerHealth));

            if (MaxPlayerHealth > PrototypeAdventureState.BaseMaxHealth + 0.01f)
            {
                Debug.Log(
                    "KAI_CHE_R27_DYNAMIC_MAX_HEALTH_PERSISTED_"
                    + Mathf.RoundToInt(MaxPlayerHealth));
            }
        }

        private bool DoesPersistedAdventureHealthMatchRuntime()
        {
            return Mathf.Abs(
                       PrototypeAdventureState.Health
                       - PlayerHealth) <= 0.01f
                && Mathf.Abs(
                       PrototypeAdventureState.MaxHealth
                       - MaxPlayerHealth) <= 0.01f;
        }

        private void UpdateSkillMasteryPersistence(float unscaledDeltaTime)
        {
            if (!_skillMasterySaveDirty)
                return;

            _skillMasterySaveTimer -= Mathf.Max(0f, unscaledDeltaTime);
            if (_skillMasterySaveTimer <= 0f)
                FlushSkillMasteryPersistence();
        }

        private void FlushSkillMasteryPersistence()
        {
            if (!_skillMasterySaveDirty)
                return;

            SaveSkillMasterySnapshotToPlayerPrefs();
            PlayerPrefs.Save();
            _skillMasterySaveDirty = false;
            _skillMasterySaveTimer = 0f;
        }

        private bool DoesPersistedSkillMasteryStateMatchRuntime()
        {
            if (PlayerPrefs.GetInt(
                    ProgressionSavePrefix + "SkillMasteryState.Version",
                    0) < SkillMasterySaveVersion)
                return false;

            for (int i = 0; i < SkillProgressionCatalog.All.Length; i++)
            {
                SkillProgressionDefinition definition =
                    SkillProgressionCatalog.All[i];
                int persistedMastery = Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(
                        ProgressionSavePrefix
                            + "SkillMastery."
                            + definition.Id,
                        0));

                if (persistedMastery
                    != _skillMastery.GetMastery(definition.Id))
                    return false;
            }

            string persistedLastUnlocked = PlayerPrefs.GetString(
                ProgressionSavePrefix
                    + "SkillMastery.LastUnlockedSkillId",
                string.Empty);
            string runtimeLastUnlocked =
                _skillMastery.LastUnlockedSkillId ?? string.Empty;

            return persistedLastUnlocked == runtimeLastUnlocked;
        }

        private string BuildSkillMasteryStateSummary()
        {
            return "FIRE_"
                + _skillMastery.GetMastery(
                    SkillProgressionCatalog.Fireball)
                + "_ICE_"
                + _skillMastery.GetMastery(
                    SkillProgressionCatalog.IceBall)
                + "_UNLOCKED_"
                + _skillMastery.UnlockedSkillCount
                + "_LAST_"
                + (_skillMastery.LastUnlockedSkillId
                    ?? "NONE");
        }

        private bool DoesPersistedBossRewardStateMatchRuntime()
        {
            return PlayerPrefs.GetInt(
                       ProgressionSavePrefix + "BossRewardState.Version",
                       0) >= BossRewardSaveVersion
                && Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(
                        ProgressionSavePrefix + "PendingBossRewards",
                        0)) == _pendingBossRewards
                && Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(
                        ProgressionSavePrefix + "BossReward.AttackRank",
                        0)) == _bossAttackRewardRank
                && Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(
                        ProgressionSavePrefix + "BossReward.VitalityRank",
                        0)) == _bossVitalityRewardRank
                && Mathf.Max(
                    0,
                    PlayerPrefs.GetInt(
                        ProgressionSavePrefix + "BossReward.TempoRank",
                        0)) == _bossTempoRewardRank;
        }

        private string BuildBossRewardStateSummary()
        {
            return "PENDING_"
                + _pendingBossRewards
                + "_ATTACK_"
                + _bossAttackRewardRank
                + "_VITALITY_"
                + _bossVitalityRewardRank
                + "_TEMPO_"
                + _bossTempoRewardRank;
        }

        private static long ParseSavedLong(string suffix)
        {
            string raw = PlayerPrefs.GetString(
                ProgressionSavePrefix + suffix,
                "0");
            return long.TryParse(raw, out long value) && value > 0L
                ? value
                : 0L;
        }

        private Vector2 PlayerPosition => _playerWorldPosition;
    }
}