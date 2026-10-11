using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Gameplay.Survivor
{
    internal static class TopDownRoguelikeBootstrap
    {
        private static bool _registered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Register()
        {
            if (_registered)
                return;

            _registered = true;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Install(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Install(scene);
        }

        private static void Install(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || scene.name != "Game")
                return;

            TopDownRoguelikeRuntime existing = UnityEngine.Object.FindFirstObjectByType<TopDownRoguelikeRuntime>();
            if (existing != null)
                return;

            SurvivorArenaController[] legacyControllers = UnityEngine.Object.FindObjectsByType<SurvivorArenaController>(FindObjectsSortMode.None);
            for (int i = 0; i < legacyControllers.Length; i++)
                legacyControllers[i].enabled = false;

            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;

                string typeName = behaviour.GetType().Name;
                if (typeName == "SurvivorHudController" || typeName == "UguiSurvivorHudController")
                    behaviour.enabled = false;
            }

            RuntimeTopDownShooterCamera[] oldCameras = UnityEngine.Object.FindObjectsByType<RuntimeTopDownShooterCamera>(FindObjectsSortMode.None);
            for (int i = 0; i < oldCameras.Length; i++)
                oldCameras[i].enabled = false;

            GameObject host = new GameObject("KaiChe.TopDownRoguelikeRuntime");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<TopDownRoguelikeRuntime>();
            Debug.Log("KAI_CHE_TOPDOWN_BOOTSTRAP_ACTIVE");
        }
    }

    public sealed class TopDownRoguelikeRuntime : MonoBehaviour
    {
        private sealed class EnemyState
        {
            public GameObject View;
            public float Health;
            public float Speed;
            public float ContactCooldown;
            public int XpValue;
            public bool Elite;
            public bool Boss;
        }

        private sealed class ProjectileState
        {
            public GameObject View;
            public Vector2 Direction;
            public float Life;
            public float Damage;
        }

        private sealed class XpOrbState
        {
            public GameObject View;
            public int Value;
        }

        private enum UpgradeKind
        {
            Power,
            RapidFire,
            MultiShot,
            MoveSpeed,
            Vitality,
            Magnet
        }

        private readonly List<EnemyState> _enemies = new List<EnemyState>(96);
        private readonly List<ProjectileState> _projectiles = new List<ProjectileState>(96);
        private readonly List<XpOrbState> _xpOrbs = new List<XpOrbState>(128);
        private readonly Stack<GameObject> _enemyPool = new Stack<GameObject>(96);
        private readonly Stack<GameObject> _projectilePool = new Stack<GameObject>(96);
        private readonly Stack<GameObject> _xpPool = new Stack<GameObject>(128);
        private readonly HashSet<RuntimeDirection8> _seenDirections = new HashSet<RuntimeDirection8>();
        private readonly UpgradeKind[] _choices = new UpgradeKind[3];

        private Camera _camera;
        private GameObject _player;
        private GameObject _facingMarker;
        private Material _playerMaterial;
        private Material _enemyMaterial;
        private Material _eliteMaterial;
        private Material _bossMaterial;
        private Material _projectileMaterial;
        private Material _xpMaterial;
        private Material _groundMaterial;

        private Vector2 _moveInput;
        private RuntimeDirection8 _facing = RuntimeDirection8.South;
        private int _joystickFingerId = -1;
        private Vector2 _joystickOrigin;
        private Vector2 _joystickCurrent;

        private float _spawnTimer;
        private float _shotTimer;
        private float _survivalTime;
        private float _nextBossTime = 75f;
        private float _playerHealth = 100f;
        private float _maxHealth = 100f;
        private float _moveSpeed = 4.4f;
        private float _damage = 1.35f;
        private float _fireInterval = 0.42f;
        private float _magnetRadius = 2.3f;
        private int _multiShot = 1;
        private int _kills;
        private int _level = 1;
        private int _xp;
        private int _xpRequired = 6;
        private int _spawnSequence;
        private int _bossSequence;
        private bool _choicePending;
        private bool _gameOver;
        private bool _firstEnemyLogged;
        private bool _firstShotLogged;
        private bool _firstPickupLogged;
        private bool _coreLoopLogged;

        private const int MaxEnemies = 90;
        private const int MaxProjectiles = 96;
        private const float WorldHalfWidth = 8.2f;
        private const float WorldHalfHeight = 11.5f;
        private const float WeaponRange = 10.5f;
        private const float ProjectileSpeed = 13.5f;
        private const float ProjectileLifetime = 1.2f;
        private const float EnemyHitRadius = 0.42f;
        private const float PlayerHitRadius = 0.55f;
        private const float PickupRadius = 0.52f;
        private const float JoystickRadiusPixels = 115f;

        public int Level => _level;
        public int Kills => _kills;
        public int Experience => _xp;
        public int ExperienceRequired => _xpRequired;
        public float Health => _playerHealth;
        public float MaxHealth => _maxHealth;
        public int EnemyCount => _enemies.Count;

        private void Awake()
        {
            Time.timeScale = 1f;
            CreateMaterials();
            CreateArena();
            CreatePlayer();
            ConfigureCamera();
            ValidateEightDirections();
            RollUpgradeChoices();
            Debug.Log("KAI_CHE_TOPDOWN_CORE_READY R1");
        }

        private void OnDestroy()
        {
            if (Time.timeScale <= 0.001f)
                Time.timeScale = 1f;
        }

        private void Update()
        {
            if (_choicePending || _gameOver)
                return;

            float dt = Time.deltaTime;
            _survivalTime += dt;

            ReadMovementInput();
            UpdatePlayer(dt);
            UpdateSpawning(dt);
            UpdateEnemies(dt);
            UpdateShooting(dt);
            UpdateProjectiles(dt);
            UpdateXpOrbs(dt);
            UpdateCamera();
            TryQueueLevelUp();
        }

        private void CreateMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            _playerMaterial = CreateMaterial(shader, new Color(0.20f, 0.62f, 1.00f));
            _enemyMaterial = CreateMaterial(shader, new Color(0.86f, 0.22f, 0.20f));
            _eliteMaterial = CreateMaterial(shader, new Color(0.72f, 0.22f, 0.92f));
            _bossMaterial = CreateMaterial(shader, new Color(1.00f, 0.48f, 0.08f));
            _projectileMaterial = CreateMaterial(shader, new Color(0.35f, 0.92f, 1.00f));
            _xpMaterial = CreateMaterial(shader, new Color(0.30f, 1.00f, 0.40f));
            _groundMaterial = CreateMaterial(shader, new Color(0.10f, 0.13f, 0.16f));
        }

        private static Material CreateMaterial(Shader shader, Color color)
        {
            if (shader == null)
                return null;
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private void CreateArena()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "TopDownArenaGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(2.0f, 1f, 2.6f);
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null && _groundMaterial != null)
                renderer.sharedMaterial = _groundMaterial;
            RemoveCollider(ground);
        }

        private void CreatePlayer()
        {
            _player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _player.name = "TopDownPlayer";
            _player.transform.position = new Vector3(0f, 0.62f, 0f);
            _player.transform.localScale = new Vector3(0.62f, 0.62f, 0.62f);
            Renderer renderer = _player.GetComponent<Renderer>();
            if (renderer != null && _playerMaterial != null)
                renderer.sharedMaterial = _playerMaterial;
            RemoveCollider(_player);

            _facingMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _facingMarker.name = "FacingMarker";
            _facingMarker.transform.SetParent(_player.transform, false);
            _facingMarker.transform.localPosition = new Vector3(0f, 0f, 0.72f);
            _facingMarker.transform.localScale = new Vector3(0.20f, 0.16f, 0.58f);
            Renderer markerRenderer = _facingMarker.GetComponent<Renderer>();
            if (markerRenderer != null && _projectileMaterial != null)
                markerRenderer.sharedMaterial = _projectileMaterial;
            RemoveCollider(_facingMarker);
        }

        private void ConfigureCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject cameraObject = new GameObject("TopDownCamera");
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            _camera.orthographic = true;
            _camera.orthographicSize = 7.2f;
            _camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _camera.backgroundColor = new Color(0.035f, 0.045f, 0.055f);
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (_camera == null || _player == null)
                return;

            Vector3 p = _player.transform.position;
            _camera.transform.position = new Vector3(p.x, 18f, p.z - 0.25f);
        }

        private void ReadMovementInput()
        {
            Vector2 keyboard = Vector2.zero;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) keyboard.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) keyboard.x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) keyboard.y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) keyboard.y += 1f;

            if (keyboard.sqrMagnitude > 0.01f)
            {
                _moveInput = Vector2.ClampMagnitude(keyboard, 1f);
                return;
            }

            if (Input.touchCount == 0)
            {
                _joystickFingerId = -1;
                _moveInput = Vector2.zero;
                return;
            }

            bool foundTrackedTouch = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (_joystickFingerId >= 0 && touch.fingerId != _joystickFingerId)
                    continue;

                if (_joystickFingerId < 0)
                {
                    if (touch.phase != TouchPhase.Began || touch.position.x > Screen.width * 0.62f)
                        continue;
                    _joystickFingerId = touch.fingerId;
                    _joystickOrigin = touch.position;
                }

                foundTrackedTouch = true;
                _joystickCurrent = touch.position;
                Vector2 delta = (_joystickCurrent - _joystickOrigin) / JoystickRadiusPixels;
                _moveInput = Vector2.ClampMagnitude(delta, 1f);

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _joystickFingerId = -1;
                    _moveInput = Vector2.zero;
                }
                break;
            }

            if (!foundTrackedTouch && _joystickFingerId >= 0)
            {
                _joystickFingerId = -1;
                _moveInput = Vector2.zero;
            }
        }

        private void UpdatePlayer(float dt)
        {
            if (_player == null)
                return;

            if (_moveInput.sqrMagnitude > 0.001f)
            {
                Vector2 normalized = Vector2.ClampMagnitude(_moveInput, 1f);
                Vector3 delta = new Vector3(normalized.x, 0f, normalized.y) * (_moveSpeed * dt);
                Vector3 next = _player.transform.position + delta;
                next.x = Mathf.Clamp(next.x, -WorldHalfWidth, WorldHalfWidth);
                next.z = Mathf.Clamp(next.z, -WorldHalfHeight, WorldHalfHeight);
                _player.transform.position = next;

                RuntimeDirection8 nextFacing = RuntimeDirection8Utility.FromVector(normalized, _facing);
                if (nextFacing != _facing)
                {
                    _facing = nextFacing;
                    _seenDirections.Add(_facing);
                }

                float angle = Mathf.Atan2(normalized.x, normalized.y) * Mathf.Rad2Deg;
                _player.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        private void UpdateSpawning(float dt)
        {
            if (_enemies.Count < MaxEnemies)
            {
                _spawnTimer -= dt;
                if (_spawnTimer <= 0f)
                {
                    SpawnEnemy(false);
                    float difficulty = Mathf.Clamp01(_survivalTime / 240f);
                    _spawnTimer = Mathf.Lerp(0.72f, 0.22f, difficulty);
                }
            }

            if (_survivalTime >= _nextBossTime)
            {
                SpawnEnemy(true);
                _nextBossTime += 105f;
            }
        }

        private void SpawnEnemy(bool boss)
        {
            _spawnSequence++;
            bool elite = !boss && _spawnSequence % 12 == 0;
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(7.0f, 9.5f);
            Vector3 center = _player != null ? _player.transform.position : Vector3.zero;
            Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, 0.58f, Mathf.Sin(angle) * radius);
            position.x = Mathf.Clamp(position.x, -WorldHalfWidth, WorldHalfWidth);
            position.z = Mathf.Clamp(position.z, -WorldHalfHeight, WorldHalfHeight);

            GameObject view = AcquireEnemyView();
            view.transform.position = position;
            view.transform.localScale = boss ? new Vector3(1.55f, 1.55f, 1.55f) : elite ? new Vector3(0.92f, 0.92f, 0.92f) : new Vector3(0.68f, 0.68f, 0.68f);
            Renderer renderer = view.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = boss ? _bossMaterial : elite ? _eliteMaterial : _enemyMaterial;
            view.SetActive(true);

            float difficulty = 1f + _survivalTime / 120f;
            EnemyState enemy = new EnemyState
            {
                View = view,
                Health = boss ? 85f * difficulty : elite ? 10f * difficulty : 3.2f * difficulty,
                Speed = boss ? 1.35f : elite ? 2.25f : 1.75f + Mathf.Min(0.85f, _survivalTime / 300f),
                ContactCooldown = 0f,
                XpValue = boss ? 45 : elite ? 8 : 1,
                Elite = elite,
                Boss = boss
            };
            _enemies.Add(enemy);

            if (!_firstEnemyLogged)
            {
                _firstEnemyLogged = true;
                Debug.Log("KAI_CHE_TOPDOWN_ENEMY_SPAWNED");
            }

            if (boss)
            {
                _bossSequence++;
                Debug.Log("KAI_CHE_TOPDOWN_BOSS_SPAWNED sequence=" + _bossSequence);
            }
        }

        private void UpdateEnemies(float dt)
        {
            if (_player == null)
                return;

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyState enemy = _enemies[i];
                if (enemy == null || enemy.View == null)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                if (enemy.Health <= 0f)
                {
                    KillEnemy(i, enemy);
                    continue;
                }

                Vector3 playerPos = _player.transform.position;
                Vector3 enemyPos = enemy.View.transform.position;
                Vector3 flat = playerPos - enemyPos;
                flat.y = 0f;
                float distance = flat.magnitude;
                if (distance > 0.001f)
                    enemy.View.transform.position += flat / distance * (enemy.Speed * dt);

                enemy.ContactCooldown = Mathf.Max(0f, enemy.ContactCooldown - dt);
                if (distance <= PlayerHitRadius && enemy.ContactCooldown <= 0f)
                {
                    float damage = enemy.Boss ? 18f : enemy.Elite ? 11f : 7f;
                    DamagePlayer(damage);
                    enemy.ContactCooldown = enemy.Boss ? 0.8f : 0.65f;
                }
            }
        }

        private void DamagePlayer(float damage)
        {
            if (_gameOver)
                return;

            _playerHealth = Mathf.Max(0f, _playerHealth - damage);
            if (_playerHealth > 0f)
                return;

            _gameOver = true;
            Time.timeScale = 0f;
            Debug.Log("KAI_CHE_TOPDOWN_RUN_GAME_OVER level=" + _level + " kills=" + _kills);
        }

        private void UpdateShooting(float dt)
        {
            _shotTimer -= dt;
            if (_shotTimer > 0f || _projectiles.Count >= MaxProjectiles || _enemies.Count == 0)
                return;

            EnemyState target = FindNearestEnemy();
            if (target == null || target.View == null || _player == null)
                return;

            Vector3 delta3 = target.View.transform.position - _player.transform.position;
            Vector2 direction = new Vector2(delta3.x, delta3.z);
            float distance = direction.magnitude;
            if (distance <= 0.001f || distance > WeaponRange)
                return;

            direction /= distance;
            int count = Mathf.Clamp(_multiShot, 1, 5);
            float step = 11f;
            float start = -step * (count - 1) * 0.5f;
            for (int i = 0; i < count; i++)
                SpawnProjectile(Rotate(direction, start + step * i));

            _shotTimer = _fireInterval;
            if (!_firstShotLogged)
            {
                _firstShotLogged = true;
                Debug.Log("KAI_CHE_TOPDOWN_AUTO_SHOT");
            }

            if (!_coreLoopLogged && _firstEnemyLogged)
            {
                _coreLoopLogged = true;
                Debug.Log("KAI_CHE_TOPDOWN_CORE_LOOP_ACTIVE");
            }
        }

        private EnemyState FindNearestEnemy()
        {
            if (_player == null)
                return null;

            Vector3 playerPos = _player.transform.position;
            EnemyState best = null;
            float bestSqr = WeaponRange * WeaponRange;
            for (int i = 0; i < _enemies.Count; i++)
            {
                EnemyState enemy = _enemies[i];
                if (enemy == null || enemy.View == null || enemy.Health <= 0f)
                    continue;
                Vector3 delta = enemy.View.transform.position - playerPos;
                delta.y = 0f;
                float sqr = delta.sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = enemy;
                }
            }
            return best;
        }

        private void SpawnProjectile(Vector2 direction)
        {
            GameObject view = AcquireProjectileView();
            Vector3 playerPos = _player.transform.position;
            view.transform.position = playerPos + new Vector3(direction.x, 0.08f, direction.y) * 0.55f;
            view.transform.localScale = Vector3.one * 0.20f;
            Renderer renderer = view.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = _projectileMaterial;
            view.SetActive(true);

            _projectiles.Add(new ProjectileState
            {
                View = view,
                Direction = direction,
                Life = ProjectileLifetime,
                Damage = _damage
            });
        }

        private void UpdateProjectiles(float dt)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                ProjectileState projectile = _projectiles[i];
                if (projectile == null || projectile.View == null)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                projectile.Life -= dt;
                projectile.View.transform.position += new Vector3(projectile.Direction.x, 0f, projectile.Direction.y) * (ProjectileSpeed * dt);
                bool consumed = false;

                for (int e = 0; e < _enemies.Count; e++)
                {
                    EnemyState enemy = _enemies[e];
                    if (enemy == null || enemy.View == null || enemy.Health <= 0f)
                        continue;

                    Vector3 delta = enemy.View.transform.position - projectile.View.transform.position;
                    delta.y = 0f;
                    float radius = enemy.Boss ? 0.85f : enemy.Elite ? 0.56f : EnemyHitRadius;
                    if (delta.sqrMagnitude > radius * radius)
                        continue;

                    enemy.Health -= projectile.Damage;
                    consumed = true;
                    break;
                }

                if (consumed || projectile.Life <= 0f)
                    ReleaseProjectile(i, projectile);
            }
        }

        private void KillEnemy(int index, EnemyState enemy)
        {
            Vector3 position = enemy.View.transform.position;
            SpawnXpOrb(position, enemy.XpValue);
            _kills++;
            if (enemy.Boss)
                Debug.Log("KAI_CHE_TOPDOWN_BOSS_DEFEATED sequence=" + _bossSequence);

            enemy.View.SetActive(false);
            _enemyPool.Push(enemy.View);
            _enemies.RemoveAt(index);
        }

        private void SpawnXpOrb(Vector3 position, int value)
        {
            GameObject view = AcquireXpView();
            view.transform.position = new Vector3(position.x, 0.22f, position.z);
            view.transform.localScale = Vector3.one * (value >= 8 ? 0.30f : 0.20f);
            Renderer renderer = view.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = _xpMaterial;
            view.SetActive(true);
            _xpOrbs.Add(new XpOrbState { View = view, Value = value });
        }

        private void UpdateXpOrbs(float dt)
        {
            if (_player == null)
                return;

            Vector3 playerPos = _player.transform.position;
            for (int i = _xpOrbs.Count - 1; i >= 0; i--)
            {
                XpOrbState orb = _xpOrbs[i];
                if (orb == null || orb.View == null)
                {
                    _xpOrbs.RemoveAt(i);
                    continue;
                }

                Vector3 delta = playerPos - orb.View.transform.position;
                delta.y = 0f;
                float distance = delta.magnitude;
                if (distance <= _magnetRadius && distance > 0.001f)
                    orb.View.transform.position += delta / distance * (Mathf.Lerp(4f, 10f, 1f - Mathf.Clamp01(distance / _magnetRadius)) * dt);

                if (distance > PickupRadius)
                    continue;

                _xp += orb.Value;
                orb.View.SetActive(false);
                _xpPool.Push(orb.View);
                _xpOrbs.RemoveAt(i);
                if (!_firstPickupLogged)
                {
                    _firstPickupLogged = true;
                    Debug.Log("KAI_CHE_TOPDOWN_XP_PICKUP");
                }
            }
        }

        private void TryQueueLevelUp()
        {
            if (_choicePending || _xp < _xpRequired)
                return;

            _xp -= _xpRequired;
            _level++;
            _xpRequired = 6 + (_level - 1) * 4;
            RollUpgradeChoices();
            _choicePending = true;
            Time.timeScale = 0f;
            Debug.Log("KAI_CHE_TOPDOWN_LEVELUP_CHOICE level=" + _level);
        }

        private void RollUpgradeChoices()
        {
            int count = Enum.GetValues(typeof(UpgradeKind)).Length;
            int a = UnityEngine.Random.Range(0, count);
            int b;
            int c;
            do { b = UnityEngine.Random.Range(0, count); } while (b == a);
            do { c = UnityEngine.Random.Range(0, count); } while (c == a || c == b);
            _choices[0] = (UpgradeKind)a;
            _choices[1] = (UpgradeKind)b;
            _choices[2] = (UpgradeKind)c;
        }

        private void ApplyUpgrade(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Power:
                    _damage *= 1.25f;
                    break;
                case UpgradeKind.RapidFire:
                    _fireInterval = Mathf.Max(0.12f, _fireInterval / 1.18f);
                    break;
                case UpgradeKind.MultiShot:
                    _multiShot = Mathf.Min(5, _multiShot + 1);
                    break;
                case UpgradeKind.MoveSpeed:
                    _moveSpeed *= 1.12f;
                    break;
                case UpgradeKind.Vitality:
                    _maxHealth += 25f;
                    _playerHealth = Mathf.Min(_maxHealth, _playerHealth + 25f);
                    break;
                case UpgradeKind.Magnet:
                    _magnetRadius *= 1.30f;
                    break;
            }

            _choicePending = false;
            Time.timeScale = 1f;
            Debug.Log("KAI_CHE_TOPDOWN_UPGRADE_APPLIED kind=" + kind);
        }

        private void ValidateEightDirections()
        {
            Vector2[] vectors =
            {
                Vector2.down,
                new Vector2(1f, -1f),
                Vector2.right,
                new Vector2(1f, 1f),
                Vector2.up,
                new Vector2(-1f, 1f),
                Vector2.left,
                new Vector2(-1f, -1f)
            };

            HashSet<RuntimeDirection8> found = new HashSet<RuntimeDirection8>();
            for (int i = 0; i < vectors.Length; i++)
                found.Add(RuntimeDirection8Utility.FromVector(vectors[i]));

            if (found.Count == 8)
                Debug.Log("KAI_CHE_TOPDOWN_8DIR_LOGIC_PASS count=8");
            else
                Debug.LogError("KAI_CHE_TOPDOWN_8DIR_LOGIC_FAIL count=" + found.Count);
        }

        private GameObject AcquireEnemyView()
        {
            if (_enemyPool.Count > 0)
                return _enemyPool.Pop();
            GameObject view = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            view.name = "TopDownEnemy";
            RemoveCollider(view);
            return view;
        }

        private GameObject AcquireProjectileView()
        {
            if (_projectilePool.Count > 0)
                return _projectilePool.Pop();
            GameObject view = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            view.name = "TopDownProjectile";
            RemoveCollider(view);
            return view;
        }

        private GameObject AcquireXpView()
        {
            if (_xpPool.Count > 0)
                return _xpPool.Pop();
            GameObject view = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            view.name = "TopDownXpOrb";
            RemoveCollider(view);
            return view;
        }

        private void ReleaseProjectile(int index, ProjectileState projectile)
        {
            projectile.View.SetActive(false);
            _projectilePool.Push(projectile.View);
            _projectiles.RemoveAt(index);
        }

        private static void RemoveCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos).normalized;
        }

        private static string UpgradeTitle(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Power: return "POWER +25%";
                case UpgradeKind.RapidFire: return "FIRE RATE +18%";
                case UpgradeKind.MultiShot: return "MULTI SHOT +1";
                case UpgradeKind.MoveSpeed: return "MOVE SPEED +12%";
                case UpgradeKind.Vitality: return "MAX HP +25";
                case UpgradeKind.Magnet: return "XP MAGNET +30%";
                default: return kind.ToString();
            }
        }

        private void OnGUI()
        {
            int baseFont = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.025f), 14, 28);
            GUI.skin.label.fontSize = baseFont;
            GUI.skin.button.fontSize = baseFont;
            GUI.color = Color.white;

            GUI.Box(new Rect(16f, 16f, Mathf.Min(560f, Screen.width * 0.58f), 82f), string.Empty);
            GUI.Label(new Rect(30f, 24f, 540f, 30f), "HP " + Mathf.CeilToInt(_playerHealth) + "/" + Mathf.CeilToInt(_maxHealth) + "   LV " + _level + "   KILLS " + _kills);
            GUI.Label(new Rect(30f, 55f, 540f, 30f), "XP " + _xp + "/" + _xpRequired + "   ENEMY " + _enemies.Count + "   TIME " + Mathf.FloorToInt(_survivalTime) + "s");

            if (_joystickFingerId >= 0)
            {
                Vector2 origin = new Vector2(_joystickOrigin.x, Screen.height - _joystickOrigin.y);
                Vector2 current = new Vector2(_joystickCurrent.x, Screen.height - _joystickCurrent.y);
                GUI.Box(new Rect(origin.x - 58f, origin.y - 58f, 116f, 116f), "MOVE");
                GUI.Box(new Rect(current.x - 24f, current.y - 24f, 48f, 48f), string.Empty);
            }
            else
            {
                GUI.Label(new Rect(24f, Screen.height - 58f, 420f, 40f), "LEFT TOUCH: MOVE   /   AUTO FIRE");
            }

            if (_choicePending)
            {
                float panelWidth = Mathf.Min(Screen.width - 40f, 920f);
                float x = (Screen.width - panelWidth) * 0.5f;
                float y = Screen.height * 0.30f;
                GUI.Box(new Rect(x, y, panelWidth, 240f), "LEVEL UP - CHOOSE 1");
                float gap = 12f;
                float buttonWidth = (panelWidth - gap * 4f) / 3f;
                for (int i = 0; i < 3; i++)
                {
                    Rect rect = new Rect(x + gap + i * (buttonWidth + gap), y + 78f, buttonWidth, 112f);
                    if (GUI.Button(rect, UpgradeTitle(_choices[i])))
                        ApplyUpgrade(_choices[i]);
                }
            }

            if (_gameOver)
            {
                float width = Mathf.Min(Screen.width - 40f, 620f);
                float x = (Screen.width - width) * 0.5f;
                float y = Screen.height * 0.32f;
                GUI.Box(new Rect(x, y, width, 230f), "RUN OVER");
                GUI.Label(new Rect(x + 30f, y + 70f, width - 60f, 42f), "LV " + _level + "   KILLS " + _kills + "   TIME " + Mathf.FloorToInt(_survivalTime) + "s");
                if (GUI.Button(new Rect(x + 60f, y + 130f, width - 120f, 64f), "RESTART"))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("Game");
                }
            }
        }
    }
}
