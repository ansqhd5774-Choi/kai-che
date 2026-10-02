using System.Collections.Generic;
using KaiChe.Core.Content;
using KaiChe.Core.Progression;
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
            public float Health;
            public float AnimationPhase;
            public float BaseScale;
            public bool IsAggro;
        }

        private sealed class ProjectileState
        {
            public GameObject View;
            public EnemyState Target;
            public Vector2 Direction;
            public float Life;
        }

        private sealed class EffectState
        {
            public GameObject View;
            public float Life;
            public float MaxLife;
            public Vector3 StartScale;
            public float SpinSpeed;
        }

        private readonly List<EnemyState> _enemies = new List<EnemyState>();
        private readonly List<ProjectileState> _projectiles = new List<ProjectileState>();
        private readonly List<EffectState> _effects = new List<EffectState>();
        private Material _materialTemplate;
        private GameObject _player;
        private Transform _weaponRoot;
        private Vector2 _moveInput;
        private Vector2 _playerWorldPosition;
        private float _spawnTimer;
        private float _shotTimer;
        private float _moveAnimationTime;
        private float _attackAnimationTime;
        private bool _initialized;
        private const float PlayerBaseScale = 0.58f;
        private const float AttackAnimationDuration = 0.18f;
        private const float EnemyAggroRadius = 3.0f;
        private const float EnemyContactRadius = 0.34f;
        private const int MaxVisibleEnemies = 100;
        private static readonly Vector2 TownEntrancePosition =
            new Vector2(0f, -8.5f);
        private const float TownEntranceRadius = 0.72f;
        private const string ProgressionSavePrefix =
            "KaiChe.PrototypeProgression.";

        private readonly SkillMasteryState _skillMastery = new SkillMasteryState();
        private readonly CharacterProgressionState _progression = new CharacterProgressionState();

        public float PlayerHealth { get; private set; } = 100f;
        public int Kills { get; private set; }
        public int Experience { get; private set; }
        public bool IsDead => PlayerHealth <= 0f;
        public int EnemyCount => _enemies.Count;
        public Vector2 PlayerWorldPosition => _playerWorldPosition;
        public string ActiveSkillId => RuntimeContentCatalog.StarterSkill.Id;
        public string ActiveSkillName => RuntimeContentCatalog.StarterSkill.DisplayName;
        public string ActiveMonsterId => RuntimeContentCatalog.StarterMonster.Id;
        public int ActiveSkillMastery => _skillMastery.GetMastery(ActiveSkillId);
        public int UnlockedSkillCount => _skillMastery.UnlockedSkillCount;
        public string LastUnlockedSkillId => _skillMastery.LastUnlockedSkillId;
        public int Level => _progression.Level;
        public long LevelExperience => _progression.LevelExperience;
        public long ExperienceRequiredForNextLevel =>
            _progression.ExperienceRequiredForNextLevel;
        public int UnspentAttributePoints => _progression.UnspentAttributePoints;
        public bool IsAttributeAllocationPending =>
            _progression.UnspentAttributePoints > 0;

        public void Initialize(Material materialTemplate)
        {
            _materialTemplate = materialTemplate;
            LoadPrototypeProgression();
            PrototypeAdventureState.EnsureLoaded();
            PlayerHealth = PrototypeAdventureState.Health;
            ConfigureCamera();

            CreateBattlefieldBackdrop();
            _player = CreateWarriorView();

            _spawnTimer = 0.2f;
            _shotTimer = 0.1f;
            _initialized = true;
        }

        public void SetMoveInput(Vector2 input)
        {
            _moveInput = IsAttributeAllocationPending
                ? Vector2.zero
                : Vector2.ClampMagnitude(input, 1f);
        }

        public int GetAttributeValue(CharacterAttribute attribute)
        {
            return _progression.GetAttribute(attribute);
        }

        public bool TrySpendAttributePoint(CharacterAttribute attribute)
        {
            bool spent = _progression.TrySpendAttributePoints(attribute);
            if (spent)
                SavePrototypeProgression();
            return spent;
        }

        private void Update()
        {
            if (!_initialized || IsDead || IsAttributeAllocationPending)
                return;

            float deltaTime = Time.deltaTime;
            UpdateWorldMovement(deltaTime);
            UpdateTownEntrance();
            UpdateSpawning(deltaTime);
            UpdateEnemies(deltaTime);
            UpdateShooting(deltaTime);
            UpdateProjectiles(deltaTime);
            UpdatePlayerAnimation(deltaTime);
            UpdateEffects(deltaTime);
        }

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            camera.orthographic = true;
            camera.orthographicSize = 11f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;
            camera.backgroundColor = new Color(0.018f, 0.022f, 0.018f);
        }

        private void UpdateWorldMovement(float deltaTime)
        {
            const float speed = 4.5f;
            _playerWorldPosition += _moveInput * speed * deltaTime;

            if (_player != null)
            {
                _player.transform.position = new Vector3(
                    _playerWorldPosition.x,
                    _playerWorldPosition.y,
                    0f);
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(
                    _playerWorldPosition.x,
                    _playerWorldPosition.y,
                    -10f);
            }
        }

        private void UpdateTownEntrance()
        {
            if ((_playerWorldPosition - TownEntrancePosition).sqrMagnitude
                > TownEntranceRadius * TownEntranceRadius)
                return;

            PrototypeAdventureState.SetHealth(PlayerHealth);
            _moveInput = Vector2.zero;
            _initialized = false;
            SceneManager.LoadScene("Town");
        }

        private void UpdatePlayerAnimation(float deltaTime)
        {
            if (_player == null) return;

            bool moving = _moveInput.sqrMagnitude > 0.01f;
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

            _player.transform.position = new Vector3(
                _playerWorldPosition.x,
                _playerWorldPosition.y + bob,
                0f);
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

            SpawnEnemy();

            float pressure = Mathf.Clamp01(Time.timeSinceLevelLoad / 90f);
            _spawnTimer = Mathf.Lerp(0.95f, 0.28f, pressure);
        }

        private void SpawnEnemy()
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            float halfHeight = camera.orthographicSize + 0.8f;
            float halfWidth = camera.orthographicSize * camera.aspect + 0.8f;            Vector2 center = PlayerPosition;
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

            MonsterDefinition definition = RuntimeContentCatalog.StarterMonster;
            float variantScale = Random.Range(0.46f, 0.58f);
            var enemy = new EnemyState
            {
                View = CreateEnemyView(
                    definition.Id,
                    new Vector3(spawn.x, spawn.y, 0f)),
                Health = definition.MaxHealth,
                AnimationPhase = Random.Range(0f, Mathf.PI * 2f),
                BaseScale = variantScale,
                IsAggro = false
            };

            if (enemy.View != null)
                enemy.View.transform.localScale = Vector3.one * variantScale;

            _enemies.Add(enemy);
        }

        private void UpdateEnemies(float deltaTime)
        {
            Vector2 playerPosition = PlayerPosition;
            MonsterDefinition definition = RuntimeContentCatalog.StarterMonster;            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyState enemy = _enemies[i];
                if (enemy.View == null)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                Vector2 position = enemy.View.transform.position;
                Vector2 offset = playerPosition - position;
                float distance = offset.magnitude;

                if (!enemy.IsAggro && distance <= EnemyAggroRadius)
                    enemy.IsAggro = true;

                if (enemy.IsAggro && distance > 0.001f)
                {
                    position += offset / distance * definition.MoveSpeed * deltaTime;
                    enemy.View.transform.position =
                        new Vector3(position.x, position.y, 0f);
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

                if (enemy.IsAggro && distance < EnemyContactRadius)
                {
                    PlayerHealth = Mathf.Max(
                        0f,
                        PlayerHealth - definition.ContactDamagePerSecond * deltaTime);
                }
            }
        }

        private void UpdateShooting(float deltaTime)
        {
            _shotTimer -= deltaTime;
            if (_shotTimer > 0f) return;            SkillDefinition skill = RuntimeContentCatalog.StarterSkill;
            EnemyState target = FindNearestEnemy(skill.Range);

            if (target == null)
            {
                _shotTimer = 0.08f;
                return;
            }

            Vector2 origin = PlayerPosition;
            Vector2 targetPosition = target.View.transform.position;
            Vector2 direction = (targetPosition - origin).normalized;

            GameObject projectileView = CreateQuad(
                "Projectile_" + skill.Id,
                new Color(0.96f, 0.78f, 0.30f),
                new Vector3(origin.x, origin.y, -0.1f),
                1f);
            projectileView.transform.localScale =
                new Vector3(0.46f, 0.10f, 1f);
            projectileView.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            var projectile = new ProjectileState
            {
                View = projectileView,
                Target = target,
                Direction = direction,
                Life = 2.2f
            };

            _projectiles.Add(projectile);
            _attackAnimationTime = AttackAnimationDuration;
            CreateSlashEffect(origin, direction);
            _shotTimer = skill.Cooldown;
        }

        private EnemyState FindNearestEnemy(float range)
        {
            EnemyState nearest = null;
            float bestDistanceSquared = range * range;
            Vector2 origin = PlayerPosition;            for (int i = 0; i < _enemies.Count; i++)
            {
                EnemyState enemy = _enemies[i];
                if (enemy.View == null) continue;

                Vector2 enemyPosition = enemy.View.transform.position;
                float distanceSquared = (enemyPosition - origin).sqrMagnitude;
                if (distanceSquared >= bestDistanceSquared) continue;

                bestDistanceSquared = distanceSquared;
                nearest = enemy;
            }

            return nearest;
        }

        private void UpdateProjectiles(float deltaTime)
        {
            SkillDefinition skill = RuntimeContentCatalog.StarterSkill;

            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                ProjectileState projectile = _projectiles[i];
                if (projectile.View == null)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                if (projectile.Target != null && projectile.Target.View != null)
                {
                    Vector2 current = projectile.View.transform.position;
                    Vector2 target = projectile.Target.View.transform.position;                    Vector2 offset = target - current;
                    if (offset.sqrMagnitude > 0.0001f)
                        projectile.Direction = offset.normalized;
                }

                Vector3 position = projectile.View.transform.position;
                position += (Vector3)(
                    projectile.Direction * skill.ProjectileSpeed * deltaTime);
                projectile.View.transform.position = position;
                projectile.View.transform.rotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Atan2(
                        projectile.Direction.y,
                        projectile.Direction.x) * Mathf.Rad2Deg);
                projectile.Life -= deltaTime;

                bool hit = projectile.Target != null
                    && projectile.Target.View != null
                    && ((Vector2)projectile.Target.View.transform.position
                        - (Vector2)position).sqrMagnitude < 0.16f;

                if (hit)
                {
                    _skillMastery.RecordUse(skill.Id);
                    CreateImpactEffect(position);
                    DamageEnemy(projectile.Target, skill.Damage);
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

                if (effect.Life > 0f) continue;

                Destroy(effect.View);
                _effects.RemoveAt(i);
            }
        }

        private void CreateSlashEffect(Vector2 origin, Vector2 direction)
        {
            var root = new GameObject("SlashEffect");
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(
                origin.x + direction.x * 0.72f,
                origin.y + direction.y * 0.72f,
                -0.16f);
            root.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            GameObject core = CreateQuad(
                "SlashCore",
                new Color(1f, 0.82f, 0.34f, 0.96f),
                Vector3.zero,
                1f);
            core.transform.SetParent(root.transform, false);
            core.transform.localScale = new Vector3(0.95f, 0.10f, 1f);

            GameObject glow = CreateQuad(
                "SlashGlow",
                new Color(0.95f, 0.42f, 0.12f, 0.72f),
                Vector3.zero,
                1f);
            glow.transform.SetParent(root.transform, false);
            glow.transform.localScale = new Vector3(0.58f, 0.22f, 1f);

            _effects.Add(new EffectState
            {
                View = root,
                Life = 0.16f,
                MaxLife = 0.16f,
                StartScale = Vector3.one,
                SpinSpeed = -180f
            });
        }

        private void CreateImpactEffect(Vector3 position)
        {
            var root = new GameObject("ImpactEffect");
            root.transform.SetParent(transform, false);
            root.transform.position =
                new Vector3(position.x, position.y, -0.18f);

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

            MonsterDefinition definition = RuntimeContentCatalog.StarterMonster;
            Experience += definition.ExperienceReward;
            int levelsGained =
                _progression.AddExperienceAndApplyPrototypeLevelUp(
                    definition.ExperienceReward);
            SavePrototypeProgression();
            Kills += 1;

            if (levelsGained > 0)
                _moveInput = Vector2.zero;

            Destroy(enemy.View);
            enemy.View = null;
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
                new Vector3(TownEntrancePosition.x, TownEntrancePosition.y, 1.95f),
                1f);
            glow.transform.localScale = new Vector3(1.35f, 0.72f, 1f);

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject post = CreateQuad(
                    side < 0 ? "TownGateLeft" : "TownGateRight",
                    new Color(0.31f, 0.24f, 0.13f),
                    new Vector3(side * 0.95f, TownEntrancePosition.y, 1.90f),
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

        private GameObject CreateEnemyView(string id, Vector3 position)
        {
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
                new Color(0.19f, 0.045f, 0.038f),
                new Vector3(-0.18f, -0.48f, 0.01f),
                0.30f);
            leftLeg.transform.SetParent(root.transform, false);
            leftLeg.transform.localScale = new Vector3(0.22f, 0.52f, 1f);

            GameObject rightLeg = CreateQuad(
                "EnemyLegRight",
                new Color(0.19f, 0.045f, 0.038f),
                new Vector3(0.18f, -0.48f, 0.01f),
                0.30f);
            rightLeg.transform.SetParent(root.transform, false);
            rightLeg.transform.localScale = new Vector3(0.22f, 0.52f, 1f);

            GameObject body = CreateQuad(
                "EnemyBody",
                new Color(0.30f, 0.075f, 0.060f),
                Vector3.zero,
                0.72f);
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.66f, 0.82f, 1f);

            GameObject chest = CreateQuad(
                "EnemyChest",
                new Color(0.50f, 0.12f, 0.075f),
                new Vector3(0f, -0.03f, -0.02f),
                0.42f);
            chest.transform.SetParent(root.transform, false);
            chest.transform.localScale = new Vector3(0.48f, 0.48f, 1f);

            GameObject leftArm = CreateQuad(
                "EnemyArmLeft",
                new Color(0.25f, 0.055f, 0.045f),
                new Vector3(-0.43f, 0.02f, -0.01f),
                0.28f);
            leftArm.transform.SetParent(root.transform, false);
            leftArm.transform.localScale = new Vector3(0.22f, 0.52f, 1f);
            leftArm.transform.localRotation = Quaternion.Euler(0f, 0f, 16f);

            GameObject rightArm = CreateQuad(
                "EnemyArmRight",
                new Color(0.25f, 0.055f, 0.045f),
                new Vector3(0.43f, 0.02f, -0.01f),
                0.28f);
            rightArm.transform.SetParent(root.transform, false);
            rightArm.transform.localScale = new Vector3(0.22f, 0.52f, 1f);
            rightArm.transform.localRotation = Quaternion.Euler(0f, 0f, -16f);

            GameObject head = CreateQuad(
                "EnemyHead",
                new Color(0.12f, 0.12f, 0.11f),
                new Vector3(0f, 0.43f, -0.035f),
                0.36f);
            head.transform.SetParent(root.transform, false);
            head.transform.localScale = new Vector3(0.40f, 0.34f, 1f);

            GameObject hornLeft = CreateQuad(
                "EnemyHornLeft",
                new Color(0.60f, 0.46f, 0.25f),
                new Vector3(-0.20f, 0.68f, -0.04f),
                0.22f);
            hornLeft.transform.SetParent(root.transform, false);
            hornLeft.transform.localScale = new Vector3(0.08f, 0.30f, 1f);
            hornLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 28f);

            GameObject hornRight = CreateQuad(
                "EnemyHornRight",
                new Color(0.60f, 0.46f, 0.25f),
                new Vector3(0.20f, 0.68f, -0.04f),
                0.22f);
            hornRight.transform.SetParent(root.transform, false);
            hornRight.transform.localScale = new Vector3(0.08f, 0.30f, 1f);
            hornRight.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);

            GameObject leftEye = CreateQuad(
                "EnemyEyeLeft",
                new Color(1f, 0.34f, 0.06f),
                new Vector3(-0.10f, 0.44f, -0.06f),
                0.075f);
            leftEye.transform.SetParent(root.transform, false);

            GameObject rightEye = CreateQuad(
                "EnemyEyeRight",
                new Color(1f, 0.34f, 0.06f),
                new Vector3(0.10f, 0.44f, -0.06f),
                0.075f);
            rightEye.transform.SetParent(root.transform, false);

            return root;
        }

        private GameObject CreateWarriorView()
        {
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
            {
                var material = new Material(_materialTemplate);
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", color);
                else
                    material.color = color;

                renderer.sharedMaterial = material;
            }

            return view;
        }

        private void LoadPrototypeProgression()
        {
            string levelKey = ProgressionSavePrefix + "Level";
            if (!PlayerPrefs.HasKey(levelKey))
                return;

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

            PlayerPrefs.Save();
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