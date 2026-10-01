using System.Collections.Generic;
using KaiChe.Core.Content;
using KaiChe.Core.Progression;
using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    public sealed class SurvivorArenaController : MonoBehaviour
    {
        private sealed class EnemyState
        {
            public GameObject View;
            public float Health;
        }

        private sealed class ProjectileState
        {
            public GameObject View;
            public EnemyState Target;
            public Vector2 Direction;
            public float Life;
        }

        private readonly List<EnemyState> _enemies = new List<EnemyState>();
        private readonly List<ProjectileState> _projectiles = new List<ProjectileState>();
        private Material _materialTemplate;
        private GameObject _player;
        private Vector2 _moveInput;
        private Vector2 _playerWorldPosition;
        private float _spawnTimer;
        private float _shotTimer;
        private bool _initialized;
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
            ConfigureCamera();

            _player = CreateQuad(
                "Player",
                new Color(0.2f, 0.65f, 1f),
                Vector3.zero,
                0.72f);

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
            UpdateSpawning(deltaTime);
            UpdateEnemies(deltaTime);
            UpdateShooting(deltaTime);
            UpdateProjectiles(deltaTime);
        }

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            camera.orthographic = true;
            camera.orthographicSize = 8f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.065f);
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

        private void UpdateSpawning(float deltaTime)
        {
            _spawnTimer -= deltaTime;
            if (_spawnTimer > 0f || _enemies.Count >= 60) return;

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
            var enemy = new EnemyState
            {
                View = CreateQuad(
                    "Enemy_" + definition.Id,
                    new Color(0.95f, 0.25f, 0.22f),
                    new Vector3(spawn.x, spawn.y, 0f),
                    0.58f),
                Health = definition.MaxHealth
            };

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

                if (distance > 0.001f)
                {
                    position += offset / distance * definition.MoveSpeed * deltaTime;
                    enemy.View.transform.position =
                        new Vector3(position.x, position.y, 0f);
                }

                if (distance < 0.62f)
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

            var projectile = new ProjectileState
            {
                View = CreateQuad(
                    "Projectile_" + skill.Id,
                    new Color(1f, 0.85f, 0.18f),
                    new Vector3(origin.x, origin.y, -0.1f),
                    0.18f),
                Target = target,
                Direction = direction,
                Life = 2.2f
            };

            _projectiles.Add(projectile);
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
                projectile.Life -= deltaTime;

                bool hit = projectile.Target != null
                    && projectile.Target.View != null
                    && ((Vector2)projectile.Target.View.transform.position
                        - (Vector2)position).sqrMagnitude < 0.16f;

                if (hit)
                {
                    _skillMastery.RecordUse(skill.Id);
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
        }        private void DamageEnemy(EnemyState enemy, float damage)
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